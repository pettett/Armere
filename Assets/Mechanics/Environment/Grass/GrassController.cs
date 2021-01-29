using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using System;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GrassController : MonoBehaviour
{
	public static readonly int ID_cameraPosition = Shader.PropertyToID("cameraPosition");
	public static readonly int ID_PusherPositions = Shader.PropertyToID("_PusherPositions");
	public static readonly int ID_chunkID = Shader.PropertyToID("chunkID");
	public static readonly int ID_seed = Shader.PropertyToID("seed");
	public static readonly int ID_grassDensityUVMinMax = Shader.PropertyToID("grassDensityUVMinMax");
	public static readonly int ID_grassPositionBoundsMinMax = Shader.PropertyToID("grassPositionBoundsMinMax");
	public static readonly int ID_Grass = Shader.PropertyToID("_Grass");
	public static readonly int ID_pushers = Shader.PropertyToID("pushers");
	public static readonly int ID_viewRadiusMinMax = Shader.PropertyToID("viewRadiusMinMax");
	public static readonly int ID_Output = Shader.PropertyToID("_Output");
	public static readonly int ID_Properties = Shader.PropertyToID("_Properties");
	public static readonly int ID_IndirectArgs = Shader.PropertyToID("_IndirectArgs");
	public static readonly int ID_grassSizeMinMax = Shader.PropertyToID("grassSizeMinMax");
	public static readonly int ID_densityLayer = Shader.PropertyToID("densityLayer");
	public static readonly int ID_dispatchSize = Shader.PropertyToID("dispatchSize");
	public static readonly int ID_Gradient = Shader.PropertyToID("_Gradient");
	public static readonly int ID_Density = Shader.PropertyToID("_Density");
	public static readonly int ID_Height = Shader.PropertyToID("_Height");
	public static readonly int ID_grassHeightRange = Shader.PropertyToID("grassHeightRange");
	public static readonly int ID_CulledGrass = Shader.PropertyToID("_CulledGrass");
	public static readonly int ID_boundsExtents = Shader.PropertyToID("boundsExtents");
	public static readonly int ID_boundsTransform = Shader.PropertyToID("boundsTransform");
	public static readonly int ID_rangeTransform = Shader.PropertyToID("rangeTransform");


	[System.Serializable]
	public class GrassLayer
	{
		public enum LayerType { Main, Detail }
		public bool enabled = true;
		public LayerType layerType;
		public Vector3Int threadGroups { get; private set; }
		public bool inited { get; private set; }
		public int groupsOf8PerCell = 3;
		[SerializeField] int _currentGrassCellCapacityInView; //Theretical max grass loaded currentley
		public bool hasBlades { get; private set; }
		public int currentGrassCellCapacityInView
		{
			get => _currentGrassCellCapacityInView;
			set
			{
				threadGroups = new Vector3Int(
					Mathf.CeilToInt(value / 64f), 1, 1
					);
				hasBlades = value != 0;
				_currentGrassCellCapacityInView = value;
			}
		}
		[SerializeField] int maxBladesInView;
		public int splatMapLayer = 0;
		public Vector2 quadWidthRange = new Vector2(0.5f, 1f);
		public Vector2 quadHeightRange = new Vector2(0.5f, 1f);

		public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;

		public ComputeBuffer meshPropertiesConsumeBuffer;
		public ComputeBuffer meshPropertiesAppendBuffer;

		public ComputeBuffer matrixesBuffer;
		public ComputeBuffer drawIndirectArgsBuffer;
		public Mesh mesh;
		public Texture2D texture;
		[Range(0f, 1f)]
		public float viewRadiusScalar = 1;


		[Header("Quad tree generation")]
		public ushort smallestCellGroupPower = 1;
		public ushort greatestCellGroupPower = 5;

		public QuadTree chunkTree;
		//public FullQuadTree fullQuadTree;
		Queue<IGrassInstruction> localInstructions = new Queue<IGrassInstruction>();
		//[System.NonSerialized] public QuadTreeEnd[] endsInRange = new QuadTreeEnd[0];
		public int seed { get; private set; }
		MaterialPropertyBlock block;

		Vector2 oldPlayerUV = Vector2.one * -1000;
		float oldPlayerUVRadius = 0;

		public void UpdateChunkTree(GrassController c)
		{
			Texture2D grassDensity = c.terrain.terrainData.alphamapTextures[0];
			int texSize = grassDensity.width;
			bool[,] cells = new bool[texSize, texSize];

			Color32[] pix = grassDensity.GetPixels32();

			for (int x = 0; x < texSize; x++)
			{
				for (int y = 0; y < texSize; y++)
				{
					cells[x, y] = pix[x + y * texSize][splatMapLayer] > 0;
				}
			}


			chunkTree = QuadTree.CreateQuadTree(cells, Vector2.one * 0.5f, Vector2.one, 1 << smallestCellGroupPower, 1 << greatestCellGroupPower);
			//fullQuadTree = new FullQuadTree(cells, 1 << smallestCellGroupPower, 1 << greatestCellGroupPower);
		}


		public void InitLayer(GrassController c, int index)
		{
			seed = Mathf.CeilToInt(c.range * 2 * index);
			currentGrassCellCapacityInView = 0;

			drawIndirectArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);


			int greatestWidth = 1 << greatestCellGroupPower;

			//FIXME: May be too low if view distance is multiple of greatest cell group size
			int maxLoadedChunks = (int)(Mathf.Pow(Mathf.CeilToInt(c.viewRadius * 2 / greatestWidth), 2));
			//Find number of biggest cells the view distance could see at one time
			maxBladesInView = Mathf.FloorToInt(maxLoadedChunks * groupsOf8PerCell * 8 * greatestWidth * greatestWidth * 2.2f);

			//Make immutable because should never be touched by cpu processes?
			meshPropertiesConsumeBuffer = new ComputeBuffer(maxBladesInView, MeshProperties.size, ComputeBufferType.Append, ComputeBufferMode.Immutable);
			meshPropertiesAppendBuffer = new ComputeBuffer(maxBladesInView, MeshProperties.size, ComputeBufferType.Append, ComputeBufferMode.Immutable);
			matrixesBuffer = new ComputeBuffer(maxBladesInView, MatricesStruct.size, ComputeBufferType.Default, ComputeBufferMode.Immutable);

			//material.SetBuffer("_Properties", matrixesBuffer);
			block = new MaterialPropertyBlock();
			block.SetBuffer("_Properties", matrixesBuffer);
			block.SetTexture("_BaseMap", texture);
		}
		public void SetDispatchSize(GrassController c, ComputeShader shader, CommandBuffer cmd)
		{
			cmd.SetComputeIntParams(shader, ID_dispatchSize, threadGroups.x, threadGroups.y);
		}
		public void InitComputeShaders(GrassController c, CommandBuffer cmd)
		{
			// Argument buffer used by DrawMeshInstancedIndirect.
			uint[] args = new uint[5];
			// Arguments for drawing mesh.
			// 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
			args[0] = mesh.GetIndexCount(0);
			args[1] = 0;
			args[2] = mesh.GetIndexStart(0);
			args[3] = mesh.GetBaseVertex(0);
			args[4] = 0; //Start instance location

			cmd.SetComputeBufferData(drawIndirectArgsBuffer, args);

			cmd.SetComputeBufferCounterValue(meshPropertiesAppendBuffer, 0);
			cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);




			cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, ID_Gradient, c.gradientTexture);

			Texture2D grassDensity = c.terrain.terrainData.alphamapTextures[0];

			cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, ID_Density, grassDensity);

			if (c.terrain != null)
			{
				RenderTexture grassHeight = c.terrain.terrainData.heightmapTexture;

				cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, ID_Height, c.terrain.terrainData.heightmapTexture);

				c.test = c.terrain.terrainData.heightmapTexture;

				cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassHeightRange, new Vector2(c.grassHeightOffset, c.terrain.terrainData.heightmapScale.y / 128f));
			}

			inited = true;
		}


		public void OnCameraBeginRendering(GrassController c, CommandBuffer cmd)
		{




			bool grassChanged = false;

			while (localInstructions.Count != 0)
			{
				localInstructions.Dequeue().Execute(c, this, cmd, ref grassChanged);
			}

			if (grassChanged)
			{
				//Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
				cmd.CopyCounterValue(meshPropertiesConsumeBuffer, drawIndirectArgsBuffer, sizeof(uint));
			}

			//Copies Properties -> Output with processing
			if (hasBlades)
			{
				SetDispatchSize(c, c.compute, cmd);

				if (layerType == LayerType.Main)
				{
					cmd.SetComputeIntParam(c.compute, ID_pushers, c.pushersData.Length);
				}
				else
				{
					cmd.SetComputeIntParam(c.compute, ID_pushers, 0);
				}

				cmd.SetComputeVectorParam(c.compute, ID_viewRadiusMinMax, new Vector4(c.viewRadius * viewRadiusScalar - 1, c.viewRadius * viewRadiusScalar));

				cmd.SetComputeBufferParam(c.compute, 0, ID_Properties, meshPropertiesConsumeBuffer);
				cmd.SetComputeBufferParam(c.compute, 0, ID_Output, matrixesBuffer);

				cmd.DispatchCompute(c.compute, c.mainKernel, threadGroups.x, threadGroups.y, threadGroups.z);
			}
		}
		public void DrawGrassLayer(GrassController c)
		{
			//Use circle for less unneeded grass creation instructions
			Vector2 playerUV = new Vector2(c.CameraPosition.x - c.transform.position.x,
								c.CameraPosition.z - c.transform.position.z) / (c.range * 2);

			float uvViewRadius = (c.viewRadius * viewRadiusScalar * 0.5f) / c.range;
			//Destroy grass that was in the old uv but not in this one

			chunkTree.GetLeavesInSingleRange(oldPlayerUV, oldPlayerUVRadius, playerUV, uvViewRadius, chunk =>
				localInstructions.Enqueue(new DestroyGrassInChunkInstruction(chunk.id, chunk.cellsWidth * chunk.cellsWidth)));

			chunkTree.GetLeavesInSingleRange(playerUV, uvViewRadius, oldPlayerUV, oldPlayerUVRadius, chunk =>
				localInstructions.Enqueue(new CreateGrassInstruction(chunk.id, chunk.rect, chunk.cellsWidth * chunk.cellsWidth)));
			//endsInRange = chunksInView;

			oldPlayerUV = playerUV;
			oldPlayerUVRadius = uvViewRadius;

			if (inited && hasBlades)
			{

				Graphics.DrawMeshInstancedIndirect(
					mesh, 0, c.material, c.bounds, drawIndirectArgsBuffer,
					castShadows: shadowCastingMode, receiveShadows: true, properties: block);
			}
		}

		public void DisposeBuffers()
		{
			DisposeBuffer(ref meshPropertiesConsumeBuffer);
			DisposeBuffer(ref meshPropertiesAppendBuffer);
			DisposeBuffer(ref matrixesBuffer);
			DisposeBuffer(ref drawIndirectArgsBuffer);
		}

		private static void DisposeBuffer(ref ComputeBuffer buffer)
		{
			buffer?.Release();
			buffer = null;
		}

	}

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

	public RenderTexture test;

	[Header("Grass Creation")]
	public float range;
	public float offset;
	public ComputeShader createGrassInBoundsCompute;
	public Texture2D gradientTexture;
	public Terrain terrain;

	Queue<IGrassInstruction> grassInstructions = new Queue<IGrassInstruction>();

	[Header("Grass Rendering")]
	public float viewRadius = 10;
	public ComputeShader compute;
	public ComputeShader destroyGrassInBounds;
	public ComputeShader destroyGrassInChunk;
	public ComputeShader destroyGrassInRange;

	public Material material;
	private Bounds bounds;


	Camera mainCamera;
	int mainKernel;
	public float grassHeightOffset;


	[Header("Event Channels")]
	public Vector3FloatEventChannelSO destroyGrassInRangeEventChannel;
	public BoundsFloatEventChannelSO destroyGrassInBoundsEventChannel;


	public interface IGrassInstruction
	{
		void Execute(GrassController controller, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged);
	}

	public readonly struct CreateGrassInstruction : IGrassInstruction
	{
		public readonly int chunkID;
		public readonly Rect textureRect;
		public readonly int chunkCellCount;
		public CreateGrassInstruction(int chunkID, Rect textureRect, int cells)
		{
			this.chunkID = chunkID;
			this.textureRect = textureRect;
			this.chunkCellCount = cells;
		}

		public readonly void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged)
		{

			//These passes could be done once
			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassDensityUVMinMax,
				new Vector4(textureRect.min.x, textureRect.min.y, textureRect.max.x, textureRect.max.y));

			Vector4 bounds = new Vector4(
								textureRect.min.x * c.range * 2 - c.range,
								textureRect.min.y * c.range * 2 - c.range,
								textureRect.max.x * c.range * 2 - c.range,
								textureRect.max.y * c.range * 2 - c.range);

			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassPositionBoundsMinMax, bounds);

			//Debug.Log($"Bounds: min X{bounds.x} max X{bounds.z} min Y{bounds.y} max Y{bounds.w}");

			cmd.SetComputeBufferParam(c.createGrassInBoundsCompute, 0, ID_Grass, layer.meshPropertiesConsumeBuffer);

			cmd.SetComputeBufferParam(c.createGrassInBoundsCompute, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);

			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassSizeMinMax,
				new Vector4(layer.quadWidthRange.x, layer.quadHeightRange.x, layer.quadWidthRange.y, layer.quadHeightRange.y));

			cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_chunkID, chunkID);
			cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_seed, layer.seed);
			cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_densityLayer, layer.splatMapLayer);

			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassHeightRange,
				new Vector2(c.grassHeightOffset, c.terrain.terrainData.heightmapScale.y));

			int dispatch = chunkCellCount * layer.groupsOf8PerCell;

			cmd.DispatchCompute(c.createGrassInBoundsCompute, 0, dispatch, 1, 1);

			//Only the max amount - rejection sampling makes this lower
			layer.currentGrassCellCapacityInView += chunkCellCount * layer.groupsOf8PerCell * 8;
			//Update the sizes
			//cmd.SetComputeBufferData(c.drawIndirectArgsBuffer, new uint[] { (uint)c.currentGrassCellCapacity }, 0, 1, 1);

			grassCountChanged = true;
		}
	}
	public readonly struct DestroyGrassInBoundsInstruction : IGrassInstruction
	{
		public readonly Bounds bounds;
		public readonly float rotation;

		public DestroyGrassInBoundsInstruction(Bounds bounds, float rotation)
		{
			this.bounds = bounds;
			this.rotation = rotation;
		}

		public readonly void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged)
		{
			if (!layer.hasBlades)
			{
				//No blades - nothing to try to destroy
				return;
			}

			grassCountChanged = true;
			if (grassCountChanged)
			{
				//Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
				cmd.CopyCounterValue(layer.meshPropertiesConsumeBuffer, layer.drawIndirectArgsBuffer, sizeof(uint));
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


			cmd.SetComputeBufferCounterValue(layer.meshPropertiesAppendBuffer, 0);
			//  cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

			// destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
			// destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

			cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, ID_Grass, layer.meshPropertiesConsumeBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, ID_CulledGrass, layer.meshPropertiesAppendBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);


			cmd.DispatchCompute(c.destroyGrassInBounds, 0, layer.threadGroups.x, layer.threadGroups.y, layer.threadGroups.z);



			//Swap the buffers around
			(layer.meshPropertiesConsumeBuffer, layer.meshPropertiesAppendBuffer) = (layer.meshPropertiesAppendBuffer, layer.meshPropertiesConsumeBuffer);
		}
	}

	public readonly struct DestroyGrassInRangeInstruction : IGrassInstruction
	{
		public readonly Vector3 center;
		public readonly float size;

		public DestroyGrassInRangeInstruction(Vector3 center, float size)
		{
			this.center = center;
			this.size = size;
		}

		public readonly void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged)
		{
			if (!layer.hasBlades)
			{
				//No blades - nothing to try to destroy
				return;
			}

			grassCountChanged = true;
			if (grassCountChanged)
			{
				//Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
				cmd.CopyCounterValue(layer.meshPropertiesConsumeBuffer, layer.drawIndirectArgsBuffer, sizeof(uint));
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

			cmd.SetComputeBufferCounterValue(layer.meshPropertiesAppendBuffer, 0);
			cmd.SetComputeBufferParam(c.destroyGrassInRange, 0, ID_Grass, layer.meshPropertiesConsumeBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInRange, 0, ID_CulledGrass, layer.meshPropertiesAppendBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInRange, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);
			cmd.DispatchCompute(c.destroyGrassInRange, 0, layer.threadGroups.x, layer.threadGroups.y, layer.threadGroups.z);

			//Swap the buffers around
			(layer.meshPropertiesConsumeBuffer, layer.meshPropertiesAppendBuffer) = (layer.meshPropertiesAppendBuffer, layer.meshPropertiesConsumeBuffer);
		}
	}


	public readonly struct DestroyGrassInChunkInstruction : IGrassInstruction
	{
		public readonly int chunkID;
		public readonly int chunkCellCount;

		public DestroyGrassInChunkInstruction(int chunkID, int area)
		{
			this.chunkID = chunkID;
			this.chunkCellCount = area;
		}

		public readonly void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged)
		{
			if (!layer.hasBlades)
			{
				//No blades - nothing to try to destroy
				return;
			}

			if (grassCountChanged)
			{
				//Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
				cmd.CopyCounterValue(layer.meshPropertiesConsumeBuffer, layer.drawIndirectArgsBuffer, sizeof(uint));
			}


			grassCountChanged = true;



			layer.SetDispatchSize(c, c.destroyGrassInChunk, cmd);
			//Send the data needed and destroy grass

			cmd.SetComputeIntParam(c.destroyGrassInChunk, ID_chunkID, chunkID);

			//dispatch a compute shader that will take in buffer of all mesh data
			//And return an append buffer of mesh data remaining
			//Then use this buffer as the main buffer

			cmd.SetComputeBufferCounterValue(layer.meshPropertiesAppendBuffer, 0);
			//  cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

			// destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
			// destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

			cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, ID_Grass, layer.meshPropertiesConsumeBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, ID_CulledGrass, layer.meshPropertiesAppendBuffer);
			cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);

			cmd.DispatchCompute(c.destroyGrassInChunk, 0, layer.threadGroups.x, layer.threadGroups.y, layer.threadGroups.z);


			//Lower by max amount of blades in a chunk
			layer.currentGrassCellCapacityInView -= chunkCellCount * layer.groupsOf8PerCell * 8;

			//Swap the buffers around
			(layer.meshPropertiesConsumeBuffer, layer.meshPropertiesAppendBuffer) = (layer.meshPropertiesAppendBuffer, layer.meshPropertiesConsumeBuffer);


		}

	}



	// Mesh Properties struct to be read from the GPU.
	// Size() is a convenience funciton which returns the stride of the struct.
	private struct MeshProperties
	{

		//  rotation, position,size
		public const int size = sizeof(float) * (3 + 1 + 2 + 3) + sizeof(int);
	}

	private struct MatricesStruct
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

			cmd.SetComputeVectorParam(compute, ID_cameraPosition, CameraPosition - bounds.center);

			int maxInstructionIterations = 8;


			List<IGrassInstruction> instructionsThisFrame = new List<IGrassInstruction>(Mathf.Min(grassInstructions.Count, maxInstructionIterations));
			while (grassInstructions.Count != 0 && maxInstructionIterations > 0)
			{
				instructionsThisFrame.Add(grassInstructions.Dequeue());
				maxInstructionIterations--;
			}


			for (int i = 0; i < layers.Length; i++)
			{
				if (!layers[i].inited)
				{
					layers[i].InitComputeShaders(this, cmd);

				}
				else if (layers[i].enabled)
				{
					if (layers[i].layerType == GrassLayer.LayerType.Main)
					{
						//Only apply global commands to main layers
						bool grassCountChanged = false;

						//Execute all the commands queued from the last frame


						//Execute global instructions
						for (int ii = 0; ii < instructionsThisFrame.Count; ii++)
						{
							instructionsThisFrame[ii].Execute(this, layers[i], cmd, ref grassCountChanged);
						}
						//Execute local instructions


						if (grassCountChanged)
						{
							//Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
							cmd.CopyCounterValue(layers[i].meshPropertiesConsumeBuffer, layers[i].drawIndirectArgsBuffer, sizeof(uint));
						}
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

	private void OnEnable()
	{
		mainCamera = Camera.main;
		m_Grass_Profile = new ProfilingSampler(k_RenderGrassTag);

		Setup();
		singleton = this;

		RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;
		//Enable event channels
		destroyGrassInRangeEventChannel.OnEventRaised += DestroyBladesInRange;
		destroyGrassInBoundsEventChannel.OnEventRaised += DestroyBladesInBounds;
	}

	private void OnDisable()
	{
		DisposeBuffers();
		singleton = null;

		RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
		//Disable event channels
		destroyGrassInRangeEventChannel.OnEventRaised -= DestroyBladesInRange;
		destroyGrassInBoundsEventChannel.OnEventRaised -= DestroyBladesInBounds;
	}

	Vector4[] pushersData = new Vector4[0];


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