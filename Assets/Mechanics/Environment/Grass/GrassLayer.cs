using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
[System.Serializable]
public struct ConstantBuffer<T> where T : unmanaged
{
	[System.NonSerialized] public ComputeBuffer buffer;
	public string bufferName;
	public T data; //Name of T data type move be the same as the targeted c buffer
	[System.NonSerialized] public int nameID;

	public ConstantBuffer(string bufferName)
	{
		this.buffer = null;
		this.bufferName = bufferName;
		this.data = default;
		this.nameID = 0;
	}

	public unsafe void Init()
	{
		buffer = new ComputeBuffer(1, sizeof(T), ComputeBufferType.Structured, ComputeBufferMode.Dynamic);
		nameID = Shader.PropertyToID(bufferName);
		UploadData();
	}
	public void UploadData()
	{
		Assert.IsNotNull(buffer);
		NativeArray<T> dataArray = new NativeArray<T>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		dataArray[0] = data;
		buffer.SetData(dataArray);
		dataArray.Dispose();
	}
	public void BindBuffer(CommandBuffer cmd, ComputeShader shader)
	{
		cmd.SetComputeConstantBufferParam(shader, nameID, buffer, 0, buffer.stride);
	}

	public void Dispose()
	{
		buffer.Dispose();
	}
}
[System.Serializable]
public struct SplatMapWeights
{
	[Range(0, 1)]
	public float layer0, layer1, layer2, layer3;
	public static implicit operator Vector4(in SplatMapWeights weights)
	{
		return new Vector4(weights.layer0, weights.layer1, weights.layer2, weights.layer3);
	}
}

[System.Serializable]
public struct GrassCreationConstantBufferData
{
	public SplatMapWeights layerWeights;
	public Vector4 layerQuadSizes;

}
[System.Serializable]
public struct GrassMovementConstantBufferData
{
	public float rotationOverride;
	public float sizeOverride;
}
public struct GrassLayerInstance
{

	public readonly GrassLayer profile;
	public readonly GrassController c;
	public readonly Dictionary<int, QuadTreeEnd> loadedChunks;
	public readonly Queue<GrassController.GrassInstruction> localInstructions;
	public readonly int[] occupiedBufferCells;


	public readonly ComputeBuffer grassBladesBuffer;
	public readonly ComputeBuffer culledGrassBladesBuffer;
	public readonly ComputeBuffer grassBladesScanWorkBuffer;
	public readonly ComputeBuffer grassBladesScanBuffer;
	public readonly ComputeBuffer grassBladesCullResultBuffer;
	public readonly ComputeBuffer matrixesBuffer;
	public readonly ComputeBuffer drawIndirectArgsBuffer;
	public readonly ComputeBuffer dispatchIndirectArgsBuffer;
	public readonly int cellsWidth;
	public readonly int maxLoadedCells;
	public readonly int seed;
	public readonly QuadTree chunkTree;
	public bool hasBlades;
	public int loadedCellsCount;
	/// <summary>
	/// Index of the last cell in the loaded cells array that contains real data
	/// Allows the renderer to only render up to the last needed position
	/// </summary>
	public int lastActiveCellIndex;
	private ushort threadGroups;

	//public FullQuadTree fullQuadTree;
	//[System.NonSerialized] public QuadTreeEnd[] endsInRange = new QuadTreeEnd[0];
	readonly MaterialPropertyBlock block;

	Vector2 lastPlayerUV;
	public readonly int loadedBlades => loadedCellsCount * bladesInCell;
	//Frustum cull estimate
	public readonly Bounds bounds => c.bounds;
	public readonly int cellsInGreatestChunk => profile.cellsInGreatestChunk;
	public readonly float groupsOf8PerArea => profile.bladesPerArea / 8;
	public readonly float cellArea => (bounds.size.x / cellsWidth) * (bounds.size.z / cellsWidth);
	public readonly int maxLoadedBlades => maxLoadedCells * bladesInCell + 1;
	public readonly int maxRenderedBlades; //TODO: Make this better
	public readonly int bladesInCell => groupsOf8InCell * 8;
	public readonly int groupsOf8InCell => Mathf.FloorToInt(cellArea * groupsOf8PerArea);

	public GrassLayerInstance(GrassController c, int index, GrassLayer profile)
	{
		loadedChunks = new Dictionary<int, QuadTreeEnd>();
		localInstructions = new Queue<GrassController.GrassInstruction>();
		loadedCellsCount = 0;
		lastActiveCellIndex = 0;
		hasBlades = false;
		threadGroups = 0;
		inited = false;
		lastPlayerUV = Vector2.negativeInfinity;
		this.c = c;
		this.profile = profile;

		cellsWidth = c.terrain.terrainData.alphamapTextures[0].width / profile.pixelsPerCell;


		UnityEngine.Random.InitState(index + profile.texture.GetHashCode());
		seed = UnityEngine.Random.Range(0, short.MaxValue);


		drawIndirectArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments, ComputeBufferMode.SubUpdates);


		float cellSize = c.terrain.terrainData.size.x / c.terrain.terrainData.alphamapResolution;

		//FIXME: pi * r ^ 2
		//float radius = c.viewRadius * profile.viewRadiusScalar / cellSize;

		maxLoadedCells = 9 * profile.cellsInGreatestChunk;
		//Will show the chunk index or 0 of every cell group in the main buffer
		occupiedBufferCells = new int[maxLoadedCells];


		grassBladesBuffer = null;
		culledGrassBladesBuffer = null;
		grassBladesScanBuffer = null;
		grassBladesScanWorkBuffer = null;
		grassBladesCullResultBuffer = null;
		dispatchIndirectArgsBuffer = null;
		matrixesBuffer = null;
		block = new MaterialPropertyBlock();
		chunkTree = null;
		//Calculate the max number of blades a camera could view at once
		float view = c.viewRadius * profile.viewRadiusScalar;
		float viewArea = c.mainCam.fieldOfView * Mathf.Deg2Rad * view * view * 2;
		maxRenderedBlades = Mathf.CeilToInt(profile.bladesPerArea * viewArea);

		//TODO: Make immutable because should never be touched by cpu processes?
		var bufferMode = ComputeBufferMode.Immutable;
		grassBladesBuffer = new ComputeBuffer(
			maxLoadedBlades, GrassController.MeshProperties.size, ComputeBufferType.Default, bufferMode);

		culledGrassBladesBuffer = new ComputeBuffer(maxRenderedBlades, GrassController.MeshProperties.size, ComputeBufferType.Default, bufferMode);
		grassBladesScanBuffer = new ComputeBuffer(maxLoadedBlades, sizeof(uint), ComputeBufferType.Default, bufferMode);
		grassBladesScanWorkBuffer = new ComputeBuffer(maxLoadedBlades, sizeof(uint), ComputeBufferType.Default, bufferMode);
		grassBladesCullResultBuffer = new ComputeBuffer(maxLoadedBlades, sizeof(uint), ComputeBufferType.Default, bufferMode);
		matrixesBuffer = new ComputeBuffer(maxRenderedBlades, GrassController.MatricesStruct.size, ComputeBufferType.Default, bufferMode);

		dispatchIndirectArgsBuffer = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments, bufferMode);
		profile.grassCreationConstantBuffer.Init();
		profile.grassMovementConstantBuffer.Init();


		var args = new NativeArray<uint>(3, Allocator.Temp);
		args[0] = 0;
		args[1] = 1;
		args[2] = 1;
		dispatchIndirectArgsBuffer.SetData(args);
		args.Dispose();

		//material.SetBuffer("_Properties", matrixesBuffer);
		block.SetBuffer("_Properties", matrixesBuffer);
		block.SetTexture("_BaseMap", profile.texture);

		chunkTree = QuadTree.CreateQuadTree(
			GetCells(),
			Vector2.one * 0.5f, //Centre
			Vector2.one, //Size
			profile.greatestChunkWidth);

	}

	public bool TryFitChunk(QuadTreeEnd end, out int chunkStartPosition)
	{
		if (loadedChunks.ContainsKey(end.id))
		{
			Debug.LogError($"Attempting to load chunk {end.id} twice");
			chunkStartPosition = -1;
			return false;
		}
		else if (!end.enabled)
		{
			Debug.LogError("Attempting to load disabled chunk");
			chunkStartPosition = -1;
			return false;
		}

		//Debug.Log($"cells: {loadedCellsCount},adding: {end.cellsWidth * end.cellsWidth},maximum: {maxLoadedCells}");
		//Do not load too many chunks


		//Attempt to find a large enough space inside the buffer to fit this chunk

		int addedChunkSize = end.cellsWidth * end.cellsWidth;
		//Array stores lengths of cells in smallest cell groups as cells in a length of cellsInSmallestChunk must be the same
		chunkStartPosition = -1;

		//Search for a starting point for the cell groups to possibly be placed
		for (int i = 0; i < maxLoadedCells; i += addedChunkSize)
		{
			if (occupiedBufferCells[i] == 0)
			{
				bool empty = true;
				//This section of cells are empty, search to see if it is fully clear
				for (int ii = i + 1; ii < i + addedChunkSize; ii++)
				{
					if (occupiedBufferCells[ii] != 0)
					{
						//At least one cell in the required range is filled, look elsewhere
						empty = false;
						break;
					}
				}

				if (empty)
				{
					chunkStartPosition = i;
					break;
				}
			}
		}

		if (chunkStartPosition != -1)
		{
			for (int i = 0; i < addedChunkSize; i++)
			{
				//Mark this area as occupied by this chunk
				occupiedBufferCells[chunkStartPosition + i] = end.id;
			}

			loadedCellsCount += addedChunkSize;
			//Last active cell index used for rendering
			lastActiveCellIndex = Mathf.Max(chunkStartPosition + addedChunkSize, lastActiveCellIndex);


			hasBlades = true; //Blades added, something to draw
			threadGroups = GetThreadGroups();

			loadedChunks.Add(end.id, end);

			return true;
		}
		else
		{
			Debug.LogError($"Cannot load chunk {end.id}: out of memory");
			chunkStartPosition = -1;
			return false;
		}
	}
	public readonly ushort GetThreadGroups()
	{
		return GetThreadGroups(lastActiveCellIndex * bladesInCell);
	}
	public static ushort GetThreadGroups(int count)
	{
		//whole multiples of 64 plus one more group for straglers
		return (ushort)(count / 64 + (count % 64 == 0 ? 0 : 1));
	}
	public void RemoveChunk(QuadTreeEnd chunk)
	{
		if (!loadedChunks.ContainsKey(chunk.id))
		{
			Debug.LogError($"Attempting to unload unloaded chunk {chunk.id}");
			return;
		}

		loadedChunks.Remove(chunk.id);

		int removedChunkSize = chunk.cellsWidth * chunk.cellsWidth;
		loadedCellsCount -= removedChunkSize;

		bool foundLastActiveCell = false;
		//Work from the back to the front to also find the last active cell
		for (int i = occupiedBufferCells.Length - 1; i >= 0; i--)
		{
			if (occupiedBufferCells[i] == chunk.id)
			{
				occupiedBufferCells[i] = 0;
			}
			else if (occupiedBufferCells[i] != 0 && !foundLastActiveCell)
			{
				foundLastActiveCell = true;
				lastActiveCellIndex = i + 1;
				threadGroups = GetThreadGroups();
				if (threadGroups == 0) hasBlades = false;
			}
		}
	}

	public void PrintBuffer<T>(ComputeBuffer buffer) where T : struct
	{
		Dictionary<T, int> counts = new Dictionary<T, int>();
		var x = buffer.BeginWrite<T>(0, buffer.count);
		for (int i = 0; i < buffer.count; i++)
		{
			T val = x[i];
			if (counts.TryGetValue(val, out var v))
			{
				counts[val] = v + 1;
			}
			else
			{
				counts[val] = 0;
			}
		}
		buffer.EndWrite<T>(0);

		foreach (var pair in counts)
		{
			Debug.Log($"{pair.Key} occurs {pair.Value} times");
		}

	}

	public void OnCameraBeginRendering(GrassController c, CommandBuffer cmd)
	{

		while (localInstructions.Count != 0)
		{
			localInstructions.Dequeue().Execute(c, ref this, cmd);
		}



		//Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
		//cmd.CopyCounterValue(grassBladesBuffer, drawIndirectArgsBuffer, sizeof(uint));



		//Copies Properties -> Output with processing
		if (hasBlades)
		{
			int computeKernel = (int)profile.layerType;

			// if (layerType == LayerType.Main)
			// {
			// 	cmd.SetComputeIntParam(c.compute, GrassController.ID_pushers, c.pushersData.Length);
			// }
			// else
			// {
			// 	cmd.SetComputeIntParam(c.compute, GrassController.ID_pushers, 0);
			// }



			//Setup grass scan for culling
			RunCullGrass(cmd, threadGroups, c.GenerateFrustum(c.mainCam), c.cullGrassCompute, grassBladesBuffer, grassBladesCullResultBuffer);
			//Compact the grass vector
			RunPrefixSum(cmd, c.prefixScanCompute, grassBladesScanBuffer, grassBladesScanWorkBuffer, grassBladesCullResultBuffer);

			ApplyGrassCull(cmd, threadGroups, c.applyGrassCullCompute,
				grassBladesBuffer, culledGrassBladesBuffer,
				grassBladesCullResultBuffer, grassBladesScanBuffer,
				drawIndirectArgsBuffer, dispatchIndirectArgsBuffer);

			cmd.SetComputeVectorParam(c.compute, GrassController.ID_viewRadiusMinMax, new Vector4(c.viewRadius * profile.viewRadiusScalar - 1, c.viewRadius * profile.viewRadiusScalar));
			cmd.SetComputeBufferParam(c.compute, computeKernel, GrassController.ID_Properties, culledGrassBladesBuffer);
			cmd.SetComputeBufferParam(c.compute, computeKernel, GrassController.ID_Output, matrixesBuffer);

			profile.grassMovementConstantBuffer.BindBuffer(cmd, c.compute);

			//Turn blades into matrices
			cmd.DispatchCompute(c.compute, computeKernel, dispatchIndirectArgsBuffer, 0);



			//FillArgsBuffer(cmd, drawIndirectArgsBuffer, profile.mesh, count: (uint)maxRenderedBlades);
		}
	}


	public static void ApplyGrassCull(
		CommandBuffer cmd, int threadGroups,
		ComputeShader applyCullGrassCompute,
		ComputeBuffer bladesBuffer,
		 ComputeBuffer culledBladesBuffer, ComputeBuffer cullResultBuffer,
		  ComputeBuffer scanBuffer, ComputeBuffer indirectRenderArgs, ComputeBuffer indirectDispatchArgs)
	{
		cmd.SetComputeBufferParam(applyCullGrassCompute, 0, "_Properties", bladesBuffer);
		cmd.SetComputeBufferParam(applyCullGrassCompute, 0, "_CulledProperties", culledBladesBuffer);
		cmd.SetComputeBufferParam(applyCullGrassCompute, 0, "_CullResult", cullResultBuffer);
		cmd.SetComputeBufferParam(applyCullGrassCompute, 0, "_IndirectRenderingArgs", indirectRenderArgs);
		cmd.SetComputeBufferParam(applyCullGrassCompute, 0, "_IndirectDispatchingArgs", indirectDispatchArgs);
		cmd.SetComputeBufferParam(applyCullGrassCompute, 0, "_PrefixScanData", scanBuffer);

		cmd.SetComputeIntParams(applyCullGrassCompute, GrassController.ID_dispatchSize, threadGroups, 1);

		cmd.DispatchCompute(applyCullGrassCompute, 0, threadGroups, 1, 1);
	}

	public static void RunCullGrass(CommandBuffer cmd, int threadGroups, Matrix4x4 cameraMat, ComputeShader cullGrassCompute, ComputeBuffer bladesBuffer, ComputeBuffer cullResult)
	{
		cmd.SetComputeBufferParam(cullGrassCompute, 0, GrassController.ID_Grass, bladesBuffer);
		cmd.SetComputeBufferParam(cullGrassCompute, 0, "_CullResult", cullResult);


		cmd.SetComputeMatrixParam(cullGrassCompute, "cameraFrustum", cameraMat /* c.GenerateFrustum(c.mainCam) */);
		//Cull
		cmd.DispatchCompute(cullGrassCompute, 0, threadGroups, 1, 1);
	}


	public static void GetPrefixData(int n, out int blocks, out int scanBlocks)
	{
		const int BLOCK_SIZE = 128;
		blocks = ((n + BLOCK_SIZE * 2 - 1) / (BLOCK_SIZE * 2));
		scanBlocks = Mathf.NextPowerOfTwo(blocks);
	}


	public static void RunPrefixSum(CommandBuffer cmd,
			ComputeShader prefixScanCompute,
			ComputeBuffer destinationBuffer,
			ComputeBuffer workBuffer,
			ComputeBuffer cullResultBuffer
	)
	{
		int n = cullResultBuffer.count;

		GetPrefixData(n, out int blocks, out int scanBlocks);

		//Clean buffer
		cmd.SetComputeBufferParam(prefixScanCompute, 3, "dst", destinationBuffer);
		cmd.SetComputeBufferParam(prefixScanCompute, 3, "sumBuffer", workBuffer);
		//Do scan
		cmd.SetComputeBufferParam(prefixScanCompute, 0, GrassController.ID_Grass, cullResultBuffer);
		cmd.SetComputeBufferParam(prefixScanCompute, 0, "dst", destinationBuffer);
		cmd.SetComputeBufferParam(prefixScanCompute, 0, "sumBuffer", workBuffer);

		cmd.SetComputeBufferParam(prefixScanCompute, 1, "dst", workBuffer);


		cmd.SetComputeIntParam(prefixScanCompute, "m_numElems", n);
		cmd.SetComputeIntParam(prefixScanCompute, "m_numBlocks", blocks);
		cmd.SetComputeIntParam(prefixScanCompute, "m_numScanBlocks", scanBlocks);

		//Clean buffer
		cmd.DispatchCompute(prefixScanCompute, 3, blocks, 1, 1);
		//Do scan
		cmd.DispatchCompute(prefixScanCompute, 0, blocks, 1, 1);
		cmd.DispatchCompute(prefixScanCompute, 1, 1, 1, 1);

		if (blocks > 1)
		{
			cmd.SetComputeBufferParam(prefixScanCompute, 2, "dst", destinationBuffer);

			cmd.SetComputeBufferParam(prefixScanCompute, 2, "blockSum2", workBuffer);

			cmd.DispatchCompute(prefixScanCompute, 2, (blocks - 1), 1, 1);
		}
	}

	public readonly bool[,] GetCells()
	{
		Texture2D grassDensity = c.terrain.terrainData.alphamapTextures[0];
		if (grassDensity.isReadable)
		{
			Debug.Log("Reading texture");

			bool[,] cells = new bool[cellsWidth, cellsWidth];

			Color32[] pix = grassDensity.GetPixels32();

			for (int x = 0; x < cellsWidth; x++)
			{
				for (int y = 0; y < cellsWidth; y++)
				{
					float weight = 0;
					for (int xx = 0; xx < profile.pixelsPerCell; xx++)
						for (int yy = 0; yy < profile.pixelsPerCell; yy++)
						{
							var col = pix[(x * profile.pixelsPerCell + xx) + (y * profile.pixelsPerCell + yy) * grassDensity.width];
							weight += Vector4.Scale((Vector4)(Color)col, profile.grassCreationConstantBuffer.data.layerWeights).sqrMagnitude;
						}


					cells[x, y] = weight > 0;
				}
			}

			Debug.Log($"Creating layer quad tree width {cellsWidth}");
			return cells;
			//fullQuadTree = new FullQuadTree(cells, 1 << smallestCellGroupPower, 1 << greatestCellGroupPower);
		}
		else
		{
			throw new System.ArgumentException("Grass density texture must be readable");
		}
	}


	public readonly void SetDispatchSize(ComputeShader shader, CommandBuffer cmd)
	{
		cmd.SetComputeIntParams(shader, GrassController.ID_dispatchSize, threadGroups, 1);
	}

	public bool inited;
	public static void FillArgsBuffer(CommandBuffer cmd, ComputeBuffer buffer, Mesh mesh, uint count = 0)
	{
		var args = new NativeArray<uint>(5, Allocator.Temp);
		// Arguments for drawing mesh.
		// 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
		args[0] = mesh.GetIndexCount(0);
		//Debug.Log(maxLoadedBlades);
		args[1] = count;
		args[2] = mesh.GetIndexStart(0);
		args[3] = mesh.GetBaseVertex(0);
		args[4] = 0; //Start instance location

		cmd.SetBufferData(buffer, args);

		args.Dispose();
	}
	public void InitComputeShaders(CommandBuffer cmd)
	{
		inited = true;
		// Argument buffer used by DrawMeshInstancedIndirect.
		FillArgsBuffer(cmd, drawIndirectArgsBuffer, profile.mesh);


		if (c.terrain != null)
		{
			RenderTexture grassHeight = c.terrain.terrainData.heightmapTexture;


			cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, GrassController.ID_Height, c.terrain.terrainData.heightmapTexture);

			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, GrassController.ID_grassHeightRange, new Vector2(c.grassHeightOffset, c.terrain.terrainData.heightmapScale.y / 128f));
		}

		//Add grass around current point
		Vector2 playerUV = new Vector2(c.mainCam.transform.position.x - c.transform.position.x,
							c.mainCam.transform.position.z - c.transform.position.z) / (c.range * 2);

		float uvViewRadius = (c.viewRadius * profile.viewRadiusScalar * 0.5f) / c.range;
		//Spawn the first lot of grass with just a range search
		chunkTree.GetLeavesInRange(playerUV, uvViewRadius, CreateGrass);

		lastPlayerUV = playerUV;
	}


	readonly void CreateGrass(QuadTreeEnd end)
	{
		localInstructions.Enqueue(new CreateGrassInstruction(end));
	}
	readonly void RemoveGrass(QuadTreeEnd end)
	{
		localInstructions.Enqueue(new DestroyGrassInChunkInstruction(end));
	}

	public void DrawGrassLayer()
	{
		if (!inited) return;

		//Use circle for less unneeded grass creation instructions
		Vector2 playerUV = new Vector2(c.mainCam.transform.position.x - c.transform.position.x,
							c.mainCam.transform.position.z - c.transform.position.z) / (c.range * 2);

		float uvViewRadius = (c.viewRadius * profile.viewRadiusScalar * 0.5f) / c.range;
		//Destroy grass that was in the old uv but not in this one


		//Use existing action to save GC
		chunkTree.GetLeavesInSingleRange(playerUV, lastPlayerUV, uvViewRadius, CreateGrass);

		foreach (var item in loadedChunks.Values)
		{
			//Remove chunks no longer in range
			if (!item.RectCircleOverlap(playerUV, uvViewRadius))
			{
				RemoveGrass(item);
			}
		}

		lastPlayerUV = playerUV;

		if (hasBlades)
		{
			// //Display actual rendered blades
			// var x = drawIndirectArgsBuffer.BeginWrite<uint>(1, 1);
			// x[0] = (uint)maxRenderedBlades;
			// drawIndirectArgsBuffer.EndWrite<uint>(1);

			// var x = grassBladesBuffer.BeginWrite<GrassController.MeshProperties>(0, 1);
			// Debug.Log(x[0]);
			// grassBladesBuffer.EndWrite<GrassController.MeshProperties>(0);

			//PrintBuffer<uint>(grassBladesCullResultBuffer);

			Graphics.DrawMeshInstancedIndirect(
				profile.mesh, 0, c.material, c.bounds, drawIndirectArgsBuffer,
				castShadows: profile.shadowCastingMode, receiveShadows: true, properties: block);
		}

	}


	public readonly void DispatchComputeWithThreads(CommandBuffer cmd, ComputeShader compute, int kernelIndex)
	{

		if (hasBlades)
		{
			//Debug.Log($"Dispatching with {threadGroups}");
			cmd.DispatchCompute(compute, kernelIndex, threadGroups, 1, 1);
		}
	}


	public readonly void DisposeBuffers()
	{
		DisposeBuffer(grassBladesBuffer);
		DisposeBuffer(grassBladesScanBuffer);
		DisposeBuffer(grassBladesScanWorkBuffer);
		DisposeBuffer(grassBladesCullResultBuffer);
		DisposeBuffer(matrixesBuffer);
		DisposeBuffer(drawIndirectArgsBuffer);
		DisposeBuffer(dispatchIndirectArgsBuffer);
		profile.grassCreationConstantBuffer.Dispose();
		profile.grassMovementConstantBuffer.Dispose();
	}

	private static void DisposeBuffer(ComputeBuffer buffer)
	{
		buffer?.Release();
	}
}



[CreateAssetMenu(menuName = "Game/Environment/Grass Layer")]
public class GrassLayer : ScriptableObject
{
	public enum LayerType { Main = 0, Detail = 1 }
	public bool enabled = true;
	public LayerType layerType;

	public float bladesPerArea = 3;

	public ConstantBuffer<GrassCreationConstantBufferData> grassCreationConstantBuffer;
	public ConstantBuffer<GrassMovementConstantBufferData> grassMovementConstantBuffer = new ConstantBuffer<GrassMovementConstantBufferData>("MovementConstantBufferData");


	public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;




	public Mesh mesh;
	public Texture2D texture;

	[Range(0f, 1f)]
	public float viewRadiusScalar = 1;


	[Header("Quad tree generation")]
	public ushort smallestCellGroupPower = 1;

	/*
	 1 << smallestCellGroupPower
	  <--->
	|--------|  ^
	|        |  |  1 << smallestCellGroupPower
	|        |  |
	|--------|  v

	number of cells in smallest chunk = 1 => smallest chunk is a cell
	*/
	public int cellsInGreatestChunk => 1 << (2 * greatestCellGroupPower - 2 * smallestCellGroupPower);
	public int greatestChunkWidth => 1 << (greatestCellGroupPower - smallestCellGroupPower);
	public int pixelsPerCell => 1 << smallestCellGroupPower;

	public ushort greatestCellGroupPower = 5;




	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			grassCreationConstantBuffer.UploadData();
			grassMovementConstantBuffer.UploadData();
		}
	}

}
