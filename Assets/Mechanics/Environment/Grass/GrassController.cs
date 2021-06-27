using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using System;
using UnityEngine.Profiling;
using MyBox;


#if UNITY_EDITOR
using UnityEditor;
#endif
//[ExecuteAlways]
public class GrassController : MonoBehaviour
{
	public static readonly int
		ID_cameraPosition = Shader.PropertyToID("cameraPosition"),
		ID_PusherPositions = Shader.PropertyToID("_PusherPositions"),
		ID_chunkID = Shader.PropertyToID("chunkID"),
		ID_seed = Shader.PropertyToID("seed"),
		ID_grassDensityUVMinMax = Shader.PropertyToID("grassDensityUVMinMax"),
		ID_grassPositionBoundsMinMax = Shader.PropertyToID("grassPositionBoundsMinMax"),
		ID_Grass = Shader.PropertyToID("_Grass"),
		ID_pushers = Shader.PropertyToID("pushers"),
		ID_viewRadiusMinMax = Shader.PropertyToID("viewRadiusMinMax"),
		ID_Output = Shader.PropertyToID("_Output"),
		ID_Properties = Shader.PropertyToID("_Properties"),
		ID_IndirectArgs = Shader.PropertyToID("_IndirectArgs"),
		ID_grassSizeMinMax = Shader.PropertyToID("grassSizeMinMax"),
		ID_densityLayerWeights = Shader.PropertyToID("densityLayerWeights"),
		ID_dispatchSize = Shader.PropertyToID("dispatchSize"),
		ID_Density = Shader.PropertyToID("_Density"),
		ID_Height = Shader.PropertyToID("_Height"),
		ID_grassHeightRange = Shader.PropertyToID("grassHeightRange"),
		ID_GrassBladesOffset = Shader.PropertyToID("grassBladesOffset"),
		ID_boundsExtents = Shader.PropertyToID("boundsExtents"),
		ID_boundsTransform = Shader.PropertyToID("boundsTransform"),
		ID_rangeTransform = Shader.PropertyToID("rangeTransform"),
		ID_time = Shader.PropertyToID("time"),
		ID_windDirection = Shader.PropertyToID("windDirection");

	public GrassLayer[] layers = new GrassLayer[0];

	public GrassLayerInstance[] instances = null;


	private const string k_RenderGrassTag = "Render Grass";
	private ProfilingSampler m_Grass_Profile;


	public Matrix4x4 GenerateFrustum(Camera cam)
	{
		return Matrix4x4.Perspective(
			cam.fieldOfView + 6, cam.aspect, cam.nearClipPlane - 0.05f, Mathf.Min(cam.farClipPlane + 0.05f, viewRadius)
			) * cam.worldToCameraMatrix * Matrix4x4.Translate(bounds.center);
	}
	public Camera mainCam
	{
		get
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				return mainCamera;
			}
			else
			{
				return SceneView.lastActiveSceneView.camera;
			}
#else
			return mainCamera;
#endif
		}
	}



	public float range => terrain.terrainData.bounds.extents.x;
	public float offset => terrain.terrainData.bounds.extents.x;
	[Header("Grass Creation")]
	public ComputeShader createGrassInBoundsCompute;

	public Terrain terrain;

	public TerrainLayerData terrainLayerData;
	List<GrassInstruction> grassInstructions = new List<GrassInstruction>();

	[Header("Grass Rendering")]
	public bool updateGrass;
	public float viewRadius = 10;
	public ComputeShader compute;
	public ComputeShader destroyGrassInBounds;
	public ComputeShader destroyGrassInChunk;
	public ComputeShader destroyGrassInRange;
	public ComputeShader prefixScanCompute;
	public ComputeShader cullGrassCompute;
	public ComputeShader applyGrassCullCompute;

	public Material material;
	[System.NonSerialized] public Bounds bounds;


	Camera mainCamera;
	[System.NonSerialized] public int mainKernel;
	public float grassHeightOffset;


	[Header("Event Channels")]
	public Vector3FloatEventChannelSO destroyGrassInRangeEventChannel;
	public BoundsFloatEventChannelSO destroyGrassInBoundsEventChannel;
	public GlobalVector3SO onWindDirectionChangedGlobal;



	//Pushing


	public static List<GrassPusher> pushers = new List<GrassPusher>();
	public int maxGrassPushers = 10;
	public ComputeBuffer radialGrassPushers;

	public abstract class GrassInstruction
	{
		public abstract void Execute(GrassController controller, ref GrassLayerInstance layer, CommandBuffer cmd);
	}





	// Mesh Properties struct to be read from the GPU.
	// Size() is a convenience function which returns the stride of the struct.
	public struct MeshProperties
	{
		public Vector3 position;
		public float yRot;
		public Vector2 scale;
		public Vector3 color;
		//If chunkID = 0, grass does not exist
		public uint chunkID;
		//  rotation, position,size

		//Vector3 blank0, blank1;
		public const int size = sizeof(float) * (3 + 1 + 2 + 3) + sizeof(int);


		public override string ToString()
		{
			return $"Grass Blade at {position}, chunk {chunkID}";
		}
	}

	public struct MatricesStruct
	{
		public const int size = sizeof(float) * (4 * 4);
	}



	private void Setup()
	{
		InitializeBuffers();
	}


	private void InitializeBuffers()
	{
		UpdateBounds();


		mainKernel = compute.FindKernel("CSMain");
		instances = new GrassLayerInstance[layers.Length];

		//int frustumKernel = frustumCuller.FindKernel("CSMain");
		for (int i = 0; i < layers.Length; i++)
		{
			instances[i] = new GrassLayerInstance(this, i, layers[i]);
		}
	}





	bool inited = false;


	private void OnEnable()
	{
		mainCamera = Camera.main;
		m_Grass_Profile = new ProfilingSampler(k_RenderGrassTag);
		terrainHeight = terrain.terrainData.heightmapTexture;
		Setup();

		RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;
		//Enable event channels
		destroyGrassInRangeEventChannel.OnEventRaised += DestroyBladesInRange;
		destroyGrassInBoundsEventChannel.OnEventRaised += DestroyBladesInBounds;


		radialGrassPushers = new ComputeBuffer(maxGrassPushers, sizeof(float) * 4, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
	}

	private void Update()
	{
		// Profiler.BeginSample("Grass Pushers");
		// int count = Mathf.Min(maxGrassPushers, pushers.Count);
		// NativeArray<Vector4> radialPushers = radialGrassPushers.BeginWrite<Vector4>(0, count);
		// //Big enough
		// for (int i = 0; i < count; i++)
		// {
		// 	radialPushers[i] = pushers[i].Data - new Vector4(bounds.center.x, transform.position.y, bounds.center.z);
		// }

		// radialGrassPushers.EndWrite<Vector3>(count);


		// Profiler.EndSample();

		//Setup the call to draw the grass when the time comes
		for (int i = 0; i < instances.Length; i++)
		{
			//Profiler.BeginSample(layers[i].name);
			instances[i].DrawGrassLayer();
			//Profiler.EndSample();
		}


		// Debug.Log(inited);

		// uint[] temp = new uint[5];
		// drawIndirectArgsBuffer.GetData(temp);
		// Debug.Log($"{temp[1]}, max: {totalPopulation}");
	}
	void OnBeginCameraRendering(ScriptableRenderContext context, Camera[] camera)
	{
		//This is called once per frame no matter the number of cameras

		//DestroyBladesInBounds();



		if (Time.deltaTime == 0 || !updateGrass) return; //No need to update grass - nothing has happened

		CommandBuffer cmd = CommandBufferPool.Get(k_RenderGrassTag);

		// using (new ProfilingScope(cmd, m_Grass_Profile))
		// {




		//meshPropertiesBuffer.SetCounterValue((uint)totalPopulation);

		cmd.Clear();
		//cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);

		//Set common variables for movement

		//cmd.SetComputeVectorArrayParam(compute, ID_PusherPositions, pushersData);

		// cmd.SetComputeIntParam(compute, ID_pushers, Mathf.Min(maxGrassPushers, pushers.Count));
		// cmd.SetComputeBufferParam(compute, 0, ID_PusherPositions, radialGrassPushers);

		cmd.SetComputeVectorParam(compute, ID_cameraPosition, mainCam.transform.position - bounds.center);
		cmd.SetComputeFloatParam(compute, ID_time, Time.time);
		if (onWindDirectionChangedGlobal != null)
			cmd.SetComputeVectorParam(compute, ID_windDirection, onWindDirectionChangedGlobal.value);


		int instructionsThisFrameCount = Mathf.Min(grassInstructions.Count, 8);


		for (int i = 0; i < instances.Length; i++)
		{
			if (!inited)
			{

				Texture2D grassDensity = terrain.terrainData.alphamapTextures[0];
				cmd.SetComputeTextureParam(createGrassInBoundsCompute, 0, ID_Density, grassDensity);
				inited = true;
			}
			else if (!instances[i].inited)
			{
				instances[i].InitComputeShaders(cmd);
			}
			else if (layers[i].enabled)
			{
				if (layers[i].layerType == GrassLayer.LayerType.Main)
				{
					//Only apply global commands to main layers

					//Execute all the commands queued from the last frame


					//Execute global instructions
					for (int ii = 0; ii < instructionsThisFrameCount; ii++)
					{
						grassInstructions[ii].Execute(this, ref instances[i], cmd);
					}
					//Execute local instructions
				}
				instances[i].OnCameraBeginRendering(this, cmd);

			}
		}

		grassInstructions.RemoveRange(0, instructionsThisFrameCount);

		//}


		context.ExecuteCommandBuffer(cmd);
		//context.ExecuteCommandBufferAsync(cmd, ComputeQueueType.Background);
		CommandBufferPool.Release(cmd);

		// material.SetBuffer("_Properties", meshPropertiesBuffer);
	}





	private void DisposeBuffers()
	{
		for (int i = 0; i < instances.Length; i++)
		{
			instances[i].DisposeBuffers();
		}
		instances = null;
		radialGrassPushers.Dispose();
	}

	void UpdateBounds()
	{
		// Boundary surrounding the meshes we will be drawing.  Used for occlusion.
		bounds = new Bounds(transform.position + new Vector3(offset, 0, offset), Vector3.one * (range * 2 + 1));
	}

	public RenderTexture terrainHeight;



	private void OnDisable()
	{
		DisposeBuffers();
		inited = false;


		RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
		//Disable event channels
		destroyGrassInRangeEventChannel.OnEventRaised -= DestroyBladesInRange;
		destroyGrassInBoundsEventChannel.OnEventRaised -= DestroyBladesInBounds;
	}

	[ButtonMethod]
	public void ReInitGrass()
	{
		if (Application.isPlaying)
		{
			if (inited)
				OnDisable();
			OnEnable();

		}
	}

	[ButtonMethod]
	public void TestPrefixSum()
	{



		// //This is all witchcraft :(
		const int testSize = 20;

		const int BLOCK_SIZE = 128;

		int blocks = ((testSize + BLOCK_SIZE * 2 - 1) / (BLOCK_SIZE * 2));
		int scanBlocks = Mathf.NextPowerOfTwo(blocks);



		var source = new ComputeBuffer(testSize, MeshProperties.size, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
		var destination = new ComputeBuffer(testSize, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Dynamic);
		var workBuffer = new ComputeBuffer(testSize, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Dynamic);

		var a = source.BeginWrite<MeshProperties>(0, testSize);

		uint[] correctData = new uint[testSize];
		uint[] input = new uint[testSize];
		uint runningTotal = 0;

		for (int i = 0; i < testSize; i++)
		{
			uint value = (uint)UnityEngine.Random.Range(0, 2);
			a[i] = new MeshProperties() { chunkID = value };
			input[i] = value;

			correctData[i] = runningTotal;

			runningTotal += value;
		}


		source.EndWrite<MeshProperties>(testSize);

		prefixScanCompute.SetBuffer(0, GrassController.ID_Grass, source);
		prefixScanCompute.SetBuffer(0, "dst", destination);
		prefixScanCompute.SetBuffer(0, "sumBuffer", workBuffer);

		prefixScanCompute.SetBuffer(1, "dst", workBuffer);


		prefixScanCompute.SetInt("m_numElems", testSize);
		prefixScanCompute.SetInt("m_numBlocks", blocks); //Number of sharedmemory blokes
		prefixScanCompute.SetInt("m_numScanBlocks", scanBlocks); //Number of thread groups


		prefixScanCompute.Dispatch(0, blocks, 1, 1);


		prefixScanCompute.Dispatch(1, 1, 1, 1);

		if (blocks > 1)
		{
			prefixScanCompute.SetBuffer(2, "dst", destination);
			prefixScanCompute.SetBuffer(2, "blockSum2", workBuffer);
			prefixScanCompute.Dispatch(2, (blocks - 1), 1, 1);
		}

		uint[] result = new uint[testSize];
		destination.GetData(result);


		bool equal = Enumerable.SequenceEqual(result, correctData);
		Debug.Log($"For {testSize} items prefix sum is equal? {equal}");
		if (!equal)
		{
			Debug.Log($"Blocks: {blocks}, scanBlocks: {scanBlocks}");
			Debug.Log(string.Join(",", input));
			Debug.Log(string.Join(",", result));
			Debug.Log(string.Join(",", correctData));
		}

		source.Release();
		destination.Release();
		workBuffer.Release();


		//instances[0].Test();
	}





	public void DestroyBladesInBounds(Bounds bounds, float angleRad)
	{
		//Send the data needed and destroy grass
		grassInstructions.Add(new DestroyGrassInBoundsInstruction(bounds, angleRad));
	}

	public void DestroyBladesInRange(Vector3 center, float size)
	{
		//Send the data needed and destroy grass
		grassInstructions.Add(new DestroyGrassInRangeInstruction(center, size));
	}


	private void OnDrawGizmosSelected()
	{

		//Draw quad tree structure

		Gizmos.DrawWireCube(bounds.center, bounds.size);

		// UpdateChunkTree();

		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.DrawWireSphere(mainCam.transform.position, viewRadius);
		Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

		Vector2 playerUV = new Vector2(mainCam.transform.position.x - transform.position.x,
							mainCam.transform.position.z - transform.position.z) / (range * 2);


		if (instances != null)
			for (int i = 0; i < instances.Length; i++)
			{
				float uvViewRadius = (viewRadius * layers[i].viewRadiusScalar * 0.5f) / range;
				//Debug.Log($"{playerUV}, {uvViewRadius}");

				instances[i].chunkTree.DrawGizmos(terrain, playerUV, uvViewRadius, instances[i].loadedChunks);

				// UnityEngine.Random.InitState(100);

				// foreach (int dataIndex in layers[i].fullQuadTree.GetNodesInRange(playerUV, uvViewRadius))
				// {

				// 	// Gizmos.color = new Color(
				// 	// 	layers[i].fullQuadTree.nodeData[dataIndex].enabled ? 0 : 1,
				// 	// 	layers[i].fullQuadTree.nodeData[dataIndex].enabled ? 1 : 0,
				// 	// 	0,
				// 	// 1
				// 	// );
				// 	Gizmos.color = UnityEngine.Random.ColorHSV(0, 1, 0, 1, 0, 1, 1, 1);

				// 	Gizmos.DrawCube(new Vector3(0, 0, 1) + (Vector3)layers[i].fullQuadTree.nodes[dataIndex].rect.center * 10,
				// 	 (Vector3)layers[i].fullQuadTree.nodes[dataIndex].rect.size * 10);
				// }

			}

	}

}