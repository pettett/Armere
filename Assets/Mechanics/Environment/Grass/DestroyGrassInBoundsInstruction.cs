using UnityEngine;
using UnityEngine.Rendering;
using static GrassController;
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
		cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_GrassBladesOffset, Mathf.FloorToInt(cellsOffset * layer.cellArea * layer.groupsOf8PerArea * 8));


		cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassSizeMinMax,
			new Vector4(layer.quadWidthRange.x, layer.quadHeightRange.x, layer.quadWidthRange.y, layer.quadHeightRange.y));

		cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_chunkID, chunk.id);
		cmd.SetComputeIntParam(c.createGrassInBoundsCompute, ID_seed, layer.seed);
		cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_densityLayerWeights, layer.splatMapWeights);

		cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, ID_grassHeightRange,
			new Vector2(c.grassHeightOffset, c.terrain.terrainData.heightmapScale.y));

		ushort dispatch = (ushort)(chunk.cellsWidth * chunk.cellsWidth * layer.groupsOf8PerArea);

		if (dispatch > 0)
		{
			//Debug.Log($"Dispatching with {dispatch}");
			cmd.DispatchCompute(c.createGrassInBoundsCompute, 0, dispatch, 1, 1);
		}

		//Update the sizes
		//cmd.SetComputeBufferData(c.drawIndirectArgsBuffer, new uint[] { (uint)c.currentGrassCellCapacity }, 0, 1, 1);
	}
}