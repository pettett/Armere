using UnityEngine;
using UnityEngine.Rendering;
using static GrassController;

public class DestroyGrassInChunkInstruction : GrassInstruction
{
	public readonly QuadTreeEnd end;

	public DestroyGrassInChunkInstruction(QuadTreeEnd end)
	{
		this.end = end;
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

		cmd.SetComputeIntParam(c.destroyGrassInChunk, ID_chunkID, end.id);

		//dispatch a compute shader that will take in buffer of all mesh data
		//And return an append buffer of mesh data remaining
		//Then use this buffer as the main buffer



		cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, ID_Grass, layer.grassBladesBuffer);
		cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);


		layer.DispatchComputeWithThreads(cmd, c.destroyGrassInChunk, 0);

		layer.RemoveChunk(end);

		//Lower by max amount of blades in a chunk
		//layer.currentGrassCellCapacityInView -= chunkCellCount * layer.groupsOf8PerCell * 8;
	}
}