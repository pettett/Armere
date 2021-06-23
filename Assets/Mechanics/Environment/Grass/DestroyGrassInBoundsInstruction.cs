using UnityEngine;
using UnityEngine.Rendering;
using static GrassController;

public class DestroyGrassInBoundsInstruction : GrassInstruction
{
	public readonly Bounds destructionBounds;
	public readonly float rotation;

	public DestroyGrassInBoundsInstruction(Bounds bounds, float rotation)
	{
		this.destructionBounds = bounds;
		this.rotation = rotation;
	}

	public override void Execute(GrassController c, ref GrassLayerInstance layer, CommandBuffer cmd)
	{
		if (!layer.hasBlades)
		{
			Debug.Log("No blades???");
			//No blades - nothing to try to destroy
			return;
		}

		// Debug.Log("Removing");

		layer.SetDispatchSize(c.destroyGrassInBounds, cmd);

		//Send the data needed and destroy grass
		cmd.SetComputeVectorParam(c.destroyGrassInBounds, ID_boundsTransform,
			new Vector4(destructionBounds.center.x - c.bounds.center.x,
						destructionBounds.center.y - c.bounds.center.y,
						destructionBounds.center.z - c.bounds.center.z,
						rotation));

		cmd.SetComputeVectorParam(c.destroyGrassInBounds, ID_boundsExtents, destructionBounds.extents);



		//dispatch a compute shader that will take in buffer of all mesh data
		//And return an append buffer of mesh data remaining
		//Then use this buffer as the main buffer

		cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, ID_Grass, layer.grassBladesBuffer);
		cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, ID_IndirectArgs, layer.drawIndirectArgsBuffer);

		layer.DispatchComputeWithThreads(cmd, c.destroyGrassInBounds, 0);

	}
}
