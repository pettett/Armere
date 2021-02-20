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
[ExecuteAlways]
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
		ID_Gradient = Shader.PropertyToID("_Gradient"),
		ID_Density = Shader.PropertyToID("_Density"),
		ID_Height = Shader.PropertyToID("_Height"),
		ID_grassHeightRange = Shader.PropertyToID("grassHeightRange"),
		ID_GrassBladesOffset = Shader.PropertyToID("grassBladesOffset"),
		ID_boundsExtents = Shader.PropertyToID("boundsExtents"),
		ID_boundsTransform = Shader.PropertyToID("boundsTransform"),
		ID_rangeTransform = Shader.PropertyToID("rangeTransform");

	public GrassLayer[] layers = new GrassLayer[2];


	private const string k_RenderGrassTag = "Render Grass";
	private ProfilingSampler m_Grass_Profile;
	public static List<GrassPusher> pushers = new List<GrassPusher>();
	public static GrassController singleton;
	public Vector3 CameraPosition
	{
		get
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				return mainCamera.transform.position;
			}
			else
			{
				return SceneView.lastActiveSceneView.pivot;
			}
#else
			return mainCamera.transform.position;
#endif
		}
	}


	[Header("Grass Creation")]
	public float range;
	public float offset;
	public ComputeShader createGrassInBoundsCompute;

	public Terrain terrain;


	Queue<GrassInstruction> grassInstructions = new Queue<GrassInstruction>();

	[Header("Grass Rendering")]
	public float viewRadius = 10;
	public ComputeShader compute;
	public ComputeShader destroyGrassInBounds;
	public ComputeShader destroyGrassInChunk;
	public ComputeShader destroyGrassInRange;
	public ComputeShader prefixScanCompute;

	public Material material;
	[System.NonSerialized] public Bounds bounds;


	Camera mainCamera;
	[System.NonSerialized] public int mainKernel;
	public float grassHeightOffset;


	[Header("Event Channels")]
	public Vector3FloatEventChannelSO destroyGrassInRangeEventChannel;
	public BoundsFloatEventChannelSO destroyGrassInBoundsEventChannel;


	public abstract class GrassInstruction
	{
		public abstract void Execute(GrassController controller, GrassLayer layer, CommandBuffer cmd);
	}

	public class CreateGrassInstruction : GrassInstruction
	{
		public QuadTreeEnd chunk;
		public CreateGrassInstruction(QuadTreeEnd chunk)
		{
			this.chunk = chunk;
		}

		public override void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd)
		{

			if (!layer.TryFitChunk(chunk, out int cellsOffset))
			{
				return;
			}
			//Debug.Log($"Loading {chunk.cellsWidth * chunk.cellsWidth} cells at {cellsOffset} into buffer for chunk {chunk.id}");

			//These passes could be done once
			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassDensityUVMinMax,
				new Vector4(chunk.rect.min.x, chunk.rect.min.y, chunk.rect.max.x, chunk.rect.max.y));

			Vector4 bounds = new Vector4(
								chunk.rect.min.x * c.range * 2 - c.range,
								chunk.rect.min.y * c.range * 2 - c.range,
								chunk.rect.max.x * c.range * 2 - c.range,
								chunk.rect.max.y * c.range * 2 - c.range);

			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassPositionBoundsMinMax, bounds);

			//Debug.Log($"Bounds: min X{bounds.x} max X{bounds.z} min Y{bounds.y} max Y{bounds.w}");

			cmd.SetComputeBufferParam(c.createGrassInBoundsCompute, 0, ID_Grass, layer.grassBladesBuffer);

			cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, ID_Gradient, layer.gradientTexture);

			//cmd.SetComputeBufferParam(c.createGrassInBoundsCompute, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);

			//Place the blades into the layer
			cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_GrassBladesOffset, cellsOffset * layer.groupsOf8PerCell * 8);


			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassSizeMinMax,
				new Vector4(layer.quadWidthRange.x, layer.quadHeightRange.x, layer.quadWidthRange.y, layer.quadHeightRange.y));

			cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_chunkID, chunk.id);
			cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_seed, layer.seed);
			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_densityLayerWeights, layer.splatMapWeights);

			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassHeightRange,
				new Vector2(c.grassHeightOffset, c.terrain.terrainData.heightmapScale.y));

			int dispatch = chunk.cellsWidth * chunk.cellsWidth * layer.groupsOf8PerCell;

			if (dispatch > 0)
			{
				//Debug.Log($"Dispatching with {dispatch}");
				cmd.DispatchCompute(c.createGrassInBoundsCompute, 0, dispatch, 1, 1);
			}

			//Update the sizes
			//cmd.SetComputeBufferData(c.drawIndirectArgsBuffer, new uint[] { (uint)c.currentGrassCellCapacity }, 0, 1, 1);
		}
	}
	public class DestroyGrassInBoundsInstruction : GrassInstruction
	{
		public readonly Bounds bounds;
		public readonly float rotation;

		public DestroyGrassInBoundsInstruction(Bounds bounds, float rotation)
		{
			this.bounds = bounds;
			this.rotation = rotation;
		}

		public override void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd)
		{
			if (!layer.hasBlades)
			{
				//No blades - nothing to try to destroy
				return;
			}



			layer.SetDispatchSize(c, c.destroyGrassInBounds, cmd);

			//Send the data needed and destroy grass
			cmd.SetComputeVectorParam(c.destroyGrassInBounds, ID_boundsTransform,
				new Vector4(bounds.center.x - c.bounds.center.x,
							bounds.center.y - c.bounds.center.y,
							bounds.center.z - c.bounds.center.z,
							rotation));

			cmd.SetComputeVectorParam(c.destroyGrassInBounds, ID_boundsExtents, bounds.extents);



			//dispatch a compute shader that will take in buffer of all mesh data
			//And return an append buffer of mesh data remaining
			//Then use this buffer as the main buffer

			cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, ID_Grass, layer.grassBladesBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);

			layer.DispatchComputeWithThreads(cmd, c.destroyGrassInBounds, 0);

		}
	}

	public class DestroyGrassInRangeInstruction : GrassInstruction
	{
		public readonly Vector3 center;
		public readonly float size;

		public DestroyGrassInRangeInstruction(Vector3 center, float size)
		{
			this.center = center;
			this.size = size;
		}

		public override void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd)
		{
			if (!layer.hasBlades)
			{
				//No blades - nothing to try to destroy
				return;
			}

			layer.SetDispatchSize(c, c.destroyGrassInRange, cmd);

			//Send the data needed and destroy grass
			cmd.SetComputeVectorParam(c.destroyGrassInRange, ID_rangeTransform,
				new Vector4(center.x - c.bounds.center.x,
							center.y - c.bounds.center.y,
							center.z - c.bounds.center.z,
							size));

			//dispatch a compute shader that will take in buffer of all mesh data
			//And return an append buffer of mesh data remaining
			//Then use this buffer as the main buffer

			cmd.SetComputeBufferParam(c.destroyGrassInRange, 0, ID_Grass, layer.grassBladesBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInRange, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);

			layer.DispatchComputeWithThreads(cmd, c.destroyGrassInRange, 0);
		}
	}


	public class DestroyGrassInChunkInstruction : GrassInstruction
	{
		public readonly int chunkID;
		public readonly int chunkCellCount;

		public DestroyGrassInChunkInstruction(int chunkID, int area)
		{
			this.chunkID = chunkID;
			this.chunkCellCount = area;
		}

		public override void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd)
		{
			if (!layer.hasBlades)
			{
				//No blades - nothing to try to destroy
				return;
			}

			layer.SetDispatchSize(c, c.destroyGrassInChunk, cmd);
			//Send the data needed and destroy grass

			cmd.SetComputeIntParam(c.destroyGrassInChunk, ID_chunkID, chunkID);

			//dispatch a compute shader that will take in buffer of all mesh data
			//And return an append buffer of mesh data remaining
			//Then use this buffer as the main buffer



			cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, ID_Grass, layer.grassBladesBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);


			layer.DispatchComputeWithThreads(cmd, c.destroyGrassInChunk, 0);

			layer.RemoveChunk(chunkID);

			//Lower by max amount of blades in a chunk
			//layer.currentGrassCellCapacityInView -= chunkCellCount * layer.groupsOf8PerCell * 8;
		}
	}



	// Mesh Properties struct to be read from the GPU.
	// Size() is a convenience funciton which returns the stride of the struct.
	public struct MeshProperties
	{
		Vector3 position;
		float yRot;
		Vector2 scale;
		Vector3 color;
		//If chunkID = 0, grass does not exist
		public uint chunkID;
		//  rotation, position,size
		public const int size = sizeof(float) * (3 + 1 + 2 + 3) + sizeof(int);
	}

	public struct MatricesStruct
	{
		public const int size = sizeof(float) * (4 * 4 + 3);
	}



	private void Setup()
	{
		InitializeBuffers();
	}


	private void InitializeBuffers()
	{
		UpdateBounds();


		mainKernel = compute.FindKernel("CSMain");

		//int frustumKernel = frustumCuller.FindKernel("CSMain");
		for (int i = 0; i < layers.Length; i++)
		{
			layers[i].InitLayer(this, i);
		}


		//matrixesBuffer.SetCounterValue(0);

		PlaceBlades();
		//frustumCuller.SetBuffer(frustumKernel, "_Properties", meshPropertiesBuffer);

		// cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);

	}





	bool inited = false;


	void OnBeginCameraRendering(ScriptableRenderContext context, Camera[] camera)
	{
		//This is called once per frame no matter the number of cameras

		//DestroyBladesInBounds();



		if (Time.deltaTime == 0) return; //No need to update grass - nothing has happened

		CommandBuffer cmd = CommandBufferPool.Get(k_RenderGrassTag);

		using (new ProfilingScope(cmd, m_Grass_Profile))
		{




			//meshPropertiesBuffer.SetCounterValue((uint)totalPopulation);

			cmd.Clear();
			//cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);

			//Set common variables for movement

			cmd.SetComputeVectorArrayParam(compute, ID_PusherPositions, pushersData);
			cmd.SetComputeIntParam(compute, ID_pushers, pushersData.Length);

			cmd.SetComputeVectorParam(compute, ID_cameraPosition, CameraPosition - bounds.center);

			int maxInstructionIterations = 8;


			List<GrassInstruction> instructionsThisFrame = new List<GrassInstruction>(Mathf.Min(grassInstructions.Count, maxInstructionIterations));
			while (grassInstructions.Count != 0 && maxInstructionIterations > 0)
			{
				instructionsThisFrame.Add(grassInstructions.Dequeue());
				maxInstructionIterations--;
			}


			for (int i = 0; i < layers.Length; i++)
			{
				if (!inited)
				{

					Texture2D grassDensity = terrain.terrainData.alphamapTextures[0];
					cmd.SetComputeTextureParam(createGrassInBoundsCompute, 0, ID_Density, grassDensity);
					inited = true;
				}
				else if (!layers[i].inited)
				{
					layers[i].InitComputeShaders(this, cmd);
				}
				else if (layers[i].enabled)
				{
					if (layers[i].layerType == GrassLayer.LayerType.Main)
					{
						//Only apply global commands to main layers

						//Execute all the commands queued from the last frame


						//Execute global instructions
						for (int ii = 0; ii < instructionsThisFrame.Count; ii++)
						{
							instructionsThisFrame[ii].Execute(this, layers[i], cmd);
						}
						//Execute local instructions
					}
					layers[i].OnCameraBeginRendering(this, cmd);


				}
			}


			context.ExecuteCommandBuffer(cmd);
		}

		CommandBufferPool.Release(cmd);

		// material.SetBuffer("_Properties", meshPropertiesBuffer);
	}





	private void DisposeBuffers()
	{
		for (int i = 0; i < layers.Length; i++)
		{
			layers[i].DisposeBuffers();
		}
	}

	void UpdateBounds()
	{
		if (terrain != null)
		{
			offset = terrain.terrainData.bounds.extents.x;
			range = terrain.terrainData.bounds.extents.x;
		}
		// Boundary surrounding the meshes we will be drawing.  Used for occlusion.
		bounds = new Bounds(transform.position + new Vector3(offset, 0, offset), Vector3.one * (range * 2 + 1));
	}

	public RenderTexture terrainHeight;


	private void PlaceBlades()
	{
		for (int i = 0; i < layers.Length; i++)
		{
			layers[i].UpdateChunkTree(this);
		}


		// void CreateGrassInTree(QuadTree tree)
		// {
		//     foreach (QuadTreeLeaf leaf in tree)
		//     {
		//         if (leaf is QuadTree t)
		//         {
		//             CreateGrassInTree(t);
		//         }
		//         else if (leaf is QuadTreeEnd end && end.enabled)
		//         {
		//             grassInstructions.Enqueue(new CreateGrassInstruction(end.id, end.rect, end.cellsWidth * end.cellsWidth));
		//         }
		//     }
		// }
		//CreateGrassInTree(chunkTree);

		//grassInstructions.Enqueue(new CreateGrassInstruction(0, new Rect(-range, -range, range * 2, range * 2), texSize * texSize));
	}

	private void Start()
	{
		mainCamera = Camera.main;
		m_Grass_Profile = new ProfilingSampler(k_RenderGrassTag);
		terrainHeight = terrain.terrainData.heightmapTexture;
		Setup();
		Debug.Log("Creating grass buffers");
		singleton = this;

		RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;
		//Enable event channels
		destroyGrassInRangeEventChannel.OnEventRaised += DestroyBladesInRange;
		destroyGrassInBoundsEventChannel.OnEventRaised += DestroyBladesInBounds;
	}

	private void OnDestroy()
	{
		DisposeBuffers();
		Debug.Log("Disposed grass buffers");
		singleton = null;
		inited = false;

		RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
		//Disable event channels
		destroyGrassInRangeEventChannel.OnEventRaised -= DestroyBladesInRange;
		destroyGrassInBoundsEventChannel.OnEventRaised -= DestroyBladesInBounds;
	}

	[ButtonMethod]
	public void ReInitGrass()
	{
		OnDestroy();
		Start();
	}

	[ButtonMethod]
	public void TestPrefixSum()
	{



		//This is all witchcraft :(
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
		if (!equal || equal)
		{
			Debug.Log($"Blocks: {blocks}, scanBlocks: {scanBlocks}");
			Debug.Log(string.Join(",", input));
			Debug.Log(string.Join(",", result));
			Debug.Log(string.Join(",", correctData));
		}

		source.Release();
		destination.Release();
		workBuffer.Release();
	}



	[System.NonSerialized] public Vector4[] pushersData = new Vector4[0];


	private void Update()
	{
		Profiler.BeginSample("Grass Pushers");
		if (pushers.Count > 10)
		{
			(Vector4 data, float priority)[] pushingQueue = new (Vector4, float)[pushers.Count];

			if (pushersData == null || pushersData.Length != 10)
				pushersData = new Vector4[10];

			for (int i = 0; i < pushers.Count; i++)
			{
				pushingQueue[i].data = pushers[i].Data;
				pushingQueue[i].priority = Vector3.SqrMagnitude(pushers[i].transform.position - CameraPosition);
			}
			//Order by distance to main pusher
			pushingQueue.OrderBy(x => x.priority);

			for (int i = 0; i < 10; i++)
			{
				pushersData[i] = pushingQueue[i].data;
			}
		}
		else
		{
			//Big enough
			if (pushersData == null || pushersData.Length != pushers.Count)
				pushersData = new Vector4[pushers.Count];

			for (int i = 0; i < pushers.Count; i++)
			{
				pushersData[i] = pushers[i].Data;
			}
		}

		for (int i = 0; i < pushersData.Length; i++)
		{
			pushersData[i] -= new Vector4(bounds.center.x, transform.position.y, bounds.center.z);
		}

		Profiler.EndSample();

		//Setup the call to draw the grass when the time comes
		for (int i = 0; i < layers.Length; i++)
		{
			Profiler.BeginSample($"Grass Layer {i}");
			layers[i].DrawGrassLayer(this);
			Profiler.EndSample();
		}


		// Debug.Log(inited);

		// uint[] temp = new uint[5];
		// drawIndirectArgsBuffer.GetData(temp);
		// Debug.Log($"{temp[1]}, max: {totalPopulation}");
	}

	public void DestroyBladesInBounds(Bounds bounds, float angleRad)
	{
		//Send the data needed and destroy grass
		grassInstructions.Enqueue(new DestroyGrassInBoundsInstruction(bounds, angleRad));
	}

	public void DestroyBladesInRange(Vector3 center, float size)
	{
		//Send the data needed and destroy grass
		grassInstructions.Enqueue(new DestroyGrassInRangeInstruction(center, size));
	}


	private void OnDrawGizmosSelected()
	{
		//Draw quad tree structure

		Gizmos.DrawWireCube(bounds.center, bounds.size);


		// UpdateChunkTree();
		if (Application.isPlaying)
		{
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.DrawWireSphere(CameraPosition, viewRadius);
			Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

			Vector2 playerUV = new Vector2(CameraPosition.x - transform.position.x,
								CameraPosition.z - transform.position.z) / (range * 2);




			for (int i = 0; i < layers.Length; i++)
			{
				float uvViewRadius = (viewRadius * layers[i].viewRadiusScalar * 0.5f) / range;

				foreach (var item in layers[i].chunkTree.GetLeavesInRange(playerUV, uvViewRadius))
				{
					item.DrawGizmos();
				}

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
			Gizmos.DrawCube(bounds.center, bounds.size);

		}
	}
}