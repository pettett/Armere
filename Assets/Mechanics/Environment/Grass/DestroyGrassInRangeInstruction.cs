
using UnityEngine;
using UnityEngine.Rendering;
using static GrassController;
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