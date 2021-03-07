using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Game/Environment/Grass Layer")]
public class GrassLayer : ScriptableObject
{
	public enum LayerType { Main, Detail }
	public bool enabled = true;
	public LayerType layerType;
	private ushort threadGroups = 0;

	public bool inited { get; private set; }
	public int groupsOf8PerCell = 3;

	public bool hasBlades { get; private set; } = false;


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
	public SplatMapWeights splatMapWeights = new SplatMapWeights();

	public Vector2 quadWidthRange = new Vector2(0.5f, 1f);
	public Vector2 quadHeightRange = new Vector2(0.5f, 1f);

	public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;

	[System.NonSerialized] public int[] occupiedBufferCells;

	public ComputeBuffer grassBladesBuffer { get; private set; }
	public ComputeBuffer grassBladesScanWorkBuffer { get; private set; }
	public ComputeBuffer grassBladesScanBuffer { get; private set; }
	public ComputeBuffer grassBladesCullResultBuffer { get; private set; }

	public ComputeBuffer matrixesBuffer { get; private set; }
	public ComputeBuffer drawIndirectArgsBuffer { get; private set; }



	public Mesh mesh;
	public Texture2D texture;
	public Texture2D gradientTexture;
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


	number of cells in smallest chunk = (1 << smallestCellGroupPower) * (1 << smallestCellGroupPower)

	*/

	[System.NonSerialized] public HashSet<int> loadedChunks = new HashSet<int>();

	public ushort greatestCellGroupPower = 5;

	public int cellsInSmallestChunk => (1 << smallestCellGroupPower) * (1 << smallestCellGroupPower);
	public int cellsInGreatestChunk => (1 << greatestCellGroupPower) * (1 << greatestCellGroupPower);

	public QuadTree chunkTree { get; private set; }

	//public FullQuadTree fullQuadTree;
	Queue<GrassController.GrassInstruction> localInstructions = new Queue<GrassController.GrassInstruction>();
	//[System.NonSerialized] public QuadTreeEnd[] endsInRange = new QuadTreeEnd[0];
	public int seed { get; private set; }
	MaterialPropertyBlock block;

	Vector2 oldPlayerUV = Vector2.one * -1000;
	float oldPlayerUVRadius = 0;




	[System.NonSerialized] public int maxLoadedCells;
	public int maxLoadedBlades => maxLoadedCells * groupsOf8PerCell * 8;
	public int maxRenderedBlades => maxLoadedBlades / 2; //TODO: Make this better
	[System.NonSerialized] public int loadedCellsCount = 0;

	public int loadedBlades => loadedCellsCount * groupsOf8PerCell * 8;
	public int renderedBlades => loadedBlades / 4; //TODO: Make this better

	public bool TryFitChunk(QuadTreeEnd end, out int cellsOffsetPosition)
	{
		if (loadedChunks.Contains(end.id))
		{
			Debug.LogError("Attempting to load chunk twice");
			cellsOffsetPosition = -1;
			return false;
		}
		//Debug.Log($"cells: {loadedCellsCount},adding: {end.cellsWidth * end.cellsWidth},maximum: {maxLoadedCells}");
		//Do not load too many chunks


		//Attempt to find a large enough space inside the buffer to fit this chunk

		int addedCells = end.cellsWidth * end.cellsWidth;
		//Array stores lengths of cells in smallest cell groups as cells in a length of cellsInSmallestChunk must be the same
		int addedCellGroups = end.cellsWidth * end.cellsWidth / cellsInSmallestChunk;
		int maxLoadedCellGroups = maxLoadedCells / cellsInSmallestChunk;

		cellsOffsetPosition = -1;

		//Search for a starting point for the cell groups to possibly be placed
		for (int i = 0; i < maxLoadedCellGroups - addedCellGroups; i += addedCellGroups)
		{
			if (occupiedBufferCells[i] == 0)
			{
				bool empty = true;
				//This section of cells are empty, search to see if it is fully clear
				for (int ii = i + 1; ii < i + addedCellGroups; ii++)
				{
					if (occupiedBufferCells[ii] != 0)
					{
						empty = false;
						break;
					}
				}

				if (empty)
				{
					cellsOffsetPosition = i;
					break;
				}
			}
		}

		if (cellsOffsetPosition != -1)
		{
			for (int i = 0; i < addedCellGroups; i++)
			{
				//Mark this area as occupied by this chunk
				occupiedBufferCells[cellsOffsetPosition + i] = end.id;
			}

			//Make this value refer to actual cells instead of units of smallest groups
			cellsOffsetPosition *= cellsInSmallestChunk;

			loadedCellsCount = Mathf.Min(Mathf.Max(cellsOffsetPosition + addedCells, loadedCellsCount), maxLoadedCells);
			hasBlades = true; //Blades added, something to draw
			threadGroups = GetThreadGroups();

			loadedChunks.Add(end.id);

			return true;
		}
		else
		{
			Debug.LogError($"Cannot load chunk {end.id}");
			return false;
		}
	}

	public ushort GetThreadGroups()
	{
		return (ushort)Mathf.CeilToInt(loadedCellsCount * groupsOf8PerCell / 8f);
	}

	public void RemoveChunk(int chunk)
	{
		bool foundEnd = false;
		if (!loadedChunks.Contains(chunk))
		{
			Debug.LogError("Attempting to unload unloaded chunk");
		}
		loadedChunks.Remove(chunk);
		//Work from the back to the front to also find the last active cell
		for (int i = occupiedBufferCells.Length - 1; i >= 0; i--)
		{
			if (occupiedBufferCells[i] == chunk)
			{
				occupiedBufferCells[i] = 0;
			}
			else if (occupiedBufferCells[i] != 0 && !foundEnd)
			{
				foundEnd = true;
				loadedCellsCount = i * cellsInSmallestChunk;
				threadGroups = GetThreadGroups();
				if (threadGroups == 0) hasBlades = false;
			}
		}
	}


	public void UpdateChunkTree(GrassController c)
	{
		Texture2D grassDensity = c.terrain.terrainData.alphamapTextures[0];
		if (grassDensity.isReadable)
		{
			Debug.Log("Reading texture");

			int texSize = grassDensity.width;
			bool[,] cells = new bool[texSize, texSize];

			Color32[] pix = grassDensity.GetPixels32();

			for (int x = 0; x < texSize; x++)
			{
				for (int y = 0; y < texSize; y++)
				{
					var col = pix[x + y * texSize];

					float weight = Vector4.Scale((Vector4)(Color)col, splatMapWeights).sqrMagnitude;

					cells[x, y] = weight > 0;

				}
			}

			Debug.Log($"Creating layer quad tree width {texSize}");
			chunkTree = QuadTree.CreateQuadTree(cells, Vector2.one * 0.5f, Vector2.one, 1 << smallestCellGroupPower, 1 << greatestCellGroupPower);
			//fullQuadTree = new FullQuadTree(cells, 1 << smallestCellGroupPower, 1 << greatestCellGroupPower);
		}
	}


	public void InitLayer(GrassController c, int index)
	{
		loadedCellsCount = 0;
		hasBlades = false;
		threadGroups = 0;

		UnityEngine.Random.InitState(index + texture.GetHashCode());
		seed = UnityEngine.Random.Range(0, short.MaxValue);


		drawIndirectArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments, ComputeBufferMode.SubUpdates);



		//FIXME: pi * r ^ 2
		float radius = c.viewRadius * viewRadiusScalar * 3 / (1 << smallestCellGroupPower);

		maxLoadedCells = Mathf.CeilToInt(radius * radius * Mathf.PI);

		//Will show the chunk index or 0 of every cell group in the main buffer
		occupiedBufferCells = new int[maxLoadedCells];

		maxLoadedCells *= cellsInSmallestChunk;


		//TODO: Make immutable because should never be touched by cpu processes?
		grassBladesBuffer = new ComputeBuffer(maxLoadedBlades, GrassController.MeshProperties.size, ComputeBufferType.Default, ComputeBufferMode.Immutable);
		grassBladesScanBuffer = new ComputeBuffer(maxLoadedBlades, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Immutable);
		grassBladesScanWorkBuffer = new ComputeBuffer(maxLoadedBlades, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Immutable);
		grassBladesCullResultBuffer = new ComputeBuffer(maxLoadedBlades, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Immutable);

		matrixesBuffer = new ComputeBuffer(maxRenderedBlades, GrassController.MatricesStruct.size, ComputeBufferType.Default, ComputeBufferMode.Immutable);

		//material.SetBuffer("_Properties", matrixesBuffer);
		block = new MaterialPropertyBlock();
		block.SetBuffer("_Properties", matrixesBuffer);
		block.SetTexture("_BaseMap", texture);
	}

	public void SetDispatchSize(GrassController c, ComputeShader shader, CommandBuffer cmd)
	{
		cmd.SetComputeIntParams(shader, GrassController.ID_dispatchSize, threadGroups, 1);
	}

	public void Test()
	{
		GrassController.MeshProperties[] grass = new GrassController.MeshProperties[maxLoadedBlades];
		int[] scan = new int[maxLoadedBlades];
		grassBladesBuffer.GetData(grass);
		grassBladesScanBuffer.GetData(scan);

		for (int i = 0; i < 20; i++)
		{
			if (grass[i].chunkID != 0)
			{
				Debug.Log($"Grass {i} to {scan[i]}");
			}
		}
	}

	public void InitComputeShaders(GrassController c, CommandBuffer cmd)
	{
		// Argument buffer used by DrawMeshInstancedIndirect.
		uint[] args = new uint[5];
		// Arguments for drawing mesh.
		// 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
		args[0] = mesh.GetIndexCount(0);
		//Debug.Log(maxLoadedBlades);
		args[1] = (uint)maxLoadedBlades;
		args[2] = mesh.GetIndexStart(0);
		args[3] = mesh.GetBaseVertex(0);
		args[4] = 0; //Start instance location

		cmd.SetComputeBufferData(drawIndirectArgsBuffer, args);


		if (c.terrain != null)
		{
			RenderTexture grassHeight = c.terrain.terrainData.heightmapTexture;


			cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, GrassController.ID_Height, c.terrain.terrainData.heightmapTexture);

			cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, GrassController.ID_grassHeightRange, new Vector2(c.grassHeightOffset, c.terrain.terrainData.heightmapScale.y / 128f));
		}

		inited = true;
	}


	public void OnCameraBeginRendering(GrassController c, CommandBuffer cmd)
	{



		while (localInstructions.Count != 0)
		{
			localInstructions.Dequeue().Execute(c, this, cmd);
		}



		//Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
		//cmd.CopyCounterValue(grassBladesBuffer, drawIndirectArgsBuffer, sizeof(uint));



		//Copies Properties -> Output with processing
		if (hasBlades)
		{
			SetDispatchSize(c, c.compute, cmd);

			if (layerType == LayerType.Main)
			{
				cmd.SetComputeIntParam(c.compute, GrassController.ID_pushers, c.pushersData.Length);
			}
			else
			{
				cmd.SetComputeIntParam(c.compute, GrassController.ID_pushers, 0);
			}
			cmd.SetComputeVectorParam(c.compute, GrassController.ID_viewRadiusMinMax, new Vector4(c.viewRadius * viewRadiusScalar - 1, c.viewRadius * viewRadiusScalar));
			cmd.SetComputeBufferParam(c.compute, 0, GrassController.ID_Properties, grassBladesBuffer);
			cmd.SetComputeBufferParam(c.compute, 0, GrassController.ID_Output, matrixesBuffer);


			//Setup grass scan for culling
			RunCullGrass(cmd, c);
			//Compact the grass vector
			RunPrefixSum(cmd, c);


			cmd.SetComputeBufferParam(c.compute, 0, "_PrefixScanData", grassBladesScanBuffer);
			cmd.SetComputeBufferParam(c.compute, 0, "_CullResult", grassBladesCullResultBuffer);




			//Turn blades into matrices
			cmd.DispatchCompute(c.compute, c.mainKernel, threadGroups, 1, 1);
		}
	}

	public void RunCullGrass(CommandBuffer cmd, GrassController c)
	{
		cmd.SetComputeBufferParam(c.cullGrassCompute, 0, GrassController.ID_Grass, grassBladesBuffer);
		cmd.SetComputeBufferParam(c.cullGrassCompute, 0, "_CullResult", grassBladesCullResultBuffer);


		cmd.SetComputeMatrixParam(c.cullGrassCompute, "cameraFrustum", c.CameraFrustum);
		//Cull
		cmd.DispatchCompute(c.cullGrassCompute, 0, threadGroups, 1, 1);
	}

	public void RunPrefixSum(CommandBuffer cmd, GrassController c)
	{
		const int BLOCK_SIZE = 128;
		int n = maxLoadedBlades;
		int blocks = ((n + BLOCK_SIZE * 2 - 1) / (BLOCK_SIZE * 2));
		int scanBlocks = Mathf.NextPowerOfTwo(blocks);

		//Clean buffer
		cmd.SetComputeBufferParam(c.prefixScanCompute, 3, "dst", grassBladesScanBuffer);
		cmd.SetComputeBufferParam(c.prefixScanCompute, 3, "sumBuffer", grassBladesScanWorkBuffer);
		cmd.DispatchCompute(c.prefixScanCompute, 3, blocks, 1, 1);
		//Do scan

		cmd.SetComputeBufferParam(c.prefixScanCompute, 0, GrassController.ID_Grass, grassBladesCullResultBuffer);
		cmd.SetComputeBufferParam(c.prefixScanCompute, 0, "dst", grassBladesScanBuffer);
		cmd.SetComputeBufferParam(c.prefixScanCompute, 0, "sumBuffer", grassBladesScanWorkBuffer);

		cmd.SetComputeBufferParam(c.prefixScanCompute, 1, "dst", grassBladesScanWorkBuffer);


		cmd.SetComputeIntParam(c.prefixScanCompute, "m_numElems", n);
		cmd.SetComputeIntParam(c.prefixScanCompute, "m_numBlocks", blocks);
		cmd.SetComputeIntParam(c.prefixScanCompute, "m_numScanBlocks", scanBlocks);

		cmd.DispatchCompute(c.prefixScanCompute, 0, blocks, 1, 1);

		cmd.DispatchCompute(c.prefixScanCompute, 1, 1, 1, 1);

		if (blocks > 1)
		{
			cmd.SetComputeBufferParam(c.prefixScanCompute, 2, "dst", grassBladesScanBuffer);
			cmd.SetComputeBufferParam(c.prefixScanCompute, 2, "blockSum2", grassBladesScanWorkBuffer);

			cmd.DispatchCompute(c.prefixScanCompute, 2, (blocks - 1), 1, 1);

		}


	}


	public void DispatchComputeWithThreads(CommandBuffer cmd, ComputeShader compute, int kernelIndex)
	{

		if (hasBlades)
		{
			//Debug.Log($"Dispatching with {threadGroups}");
			cmd.DispatchCompute(compute, kernelIndex, threadGroups, 1, 1);
		}
	}

	int lastRenderedBlades = 0;

	public void DrawGrassLayer(GrassController c)
	{
		//Use circle for less unneeded grass creation instructions
		Vector2 playerUV = new Vector2(c.CameraPosition.x - c.transform.position.x,
							c.CameraPosition.z - c.transform.position.z) / (c.range * 2);

		float uvViewRadius = (c.viewRadius * viewRadiusScalar * 0.5f) / c.range;
		//Destroy grass that was in the old uv but not in this one

		chunkTree.GetLeavesInSingleRange(oldPlayerUV, oldPlayerUVRadius, playerUV, uvViewRadius, chunk =>
			localInstructions.Enqueue(new GrassController.DestroyGrassInChunkInstruction(chunk.id, chunk.cellsWidth * chunk.cellsWidth)));

		chunkTree.GetLeavesInSingleRange(playerUV, uvViewRadius, oldPlayerUV, oldPlayerUVRadius, chunk =>
			localInstructions.Enqueue(new GrassController.CreateGrassInstruction(chunk)));
		//endsInRange = chunksInView;

		oldPlayerUV = playerUV;
		oldPlayerUVRadius = uvViewRadius;


		if (inited && hasBlades)
		{
			if (renderedBlades != lastRenderedBlades) //Dont update the compute buffer every frame if not needed
			{
				var array = drawIndirectArgsBuffer.BeginWrite<int>(1, 1);

				array[0] = renderedBlades;

				drawIndirectArgsBuffer.EndWrite<int>(1);

				lastRenderedBlades = renderedBlades;
			}
			//Debug.Log("drawing grass");
			Graphics.DrawMeshInstancedIndirect(
				mesh, 0, c.material, c.bounds, drawIndirectArgsBuffer,
				castShadows: shadowCastingMode, receiveShadows: true, properties: block);
		}

	}

	public void DisposeBuffers()
	{
		inited = false;
		DisposeBuffer(grassBladesBuffer);
		DisposeBuffer(grassBladesScanBuffer);
		DisposeBuffer(grassBladesScanWorkBuffer);
		DisposeBuffer(grassBladesCullResultBuffer);
		DisposeBuffer(matrixesBuffer);
		DisposeBuffer(drawIndirectArgsBuffer);
	}

	private static void DisposeBuffer(ComputeBuffer buffer)
	{
		buffer?.Release();
	}
}
