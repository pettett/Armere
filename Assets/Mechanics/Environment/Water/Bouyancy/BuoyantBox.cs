using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

using Unity.Collections.LowLevel.Unsafe;

[BurstCompile(CompileSynchronously = true)]
public struct GenerateVoxels : IJobParallelFor
{
	[WriteOnly] public NativeArray<float4> voxels;
	public float3 volumeBoundsCentre, volumeBoundsExtents, voxelSize, size;
	public float4x4 localToWorld;
	public int3 voxelCount;
	public float voxelSpan;
	public float VoxelSubmersion(float3 voxel, float3 boundsCentre, float3 boundsExtents)
	{
		return math.saturate((boundsCentre.y + boundsExtents.y - voxel.y) / voxelSpan + 0.5f);
	}
	public void To3D(int index, out int x, out int y, out int z)
	{
		z = index / (voxelCount.x * voxelCount.y);
		index -= (z * voxelCount.x * voxelCount.y);
		y = index / voxelCount.x;
		x = index % voxelCount.x;
	}

	public void Execute(int index)
	{
		To3D(index, out int x, out int y, out int z);
		float4 v = default;
		v.xyz = math.mul(localToWorld, new float4(new float3(voxelSize.x * x, voxelSize.y * y, voxelSize.z * z) + voxelSize * 0.5f - size * 0.5f, 1)).xyz;

		v.w = VoxelSubmersion(v.xyz, volumeBoundsCentre, volumeBoundsExtents);

		voxels[index] = v;
	}
}

[BurstCompile(CompileSynchronously = true)]
public struct VoxelVelocity : IJobParallelFor
{
	//Disable safety so multiple jobs can read from the array at once
	[NativeDisableContainerSafetyRestriction, ReadOnly] public NativeArray<float4> voxels;

	[WriteOnly] public NativeArray<float3> velocities;

	[ReadOnly] public float3 worldCenter, angularVel, bodyVelocity;

	public void Execute(int index)
	{
		//Angular velocity = position vector x velocity vector

		//velocity vector =  position vector x Angular velocity
		//https://en.wikipedia.org/wiki/Angular_velocity

		float3 positionVector = voxels[index].xyz - worldCenter;

		//Split into two terms -> velocity of reference fixed point (center of mass) and velocity of and orbital angular velocity around that point

		velocities[index] = bodyVelocity + math.cross(angularVel, positionVector);
	}
}


[BurstCompile(CompileSynchronously = true)]
public struct VoxelBuoyancyForce : IJobParallelFor
{
	//Disable safety so multiple jobs can read from the array at once
	[NativeDisableContainerSafetyRestriction, ReadOnly] public NativeArray<float4> voxels;


	[WriteOnly] public NativeArray<float3> forces;
	public float3 buoyantForce;

	public void Execute(int index)
	{
		forces[index] = voxels[index].w * buoyantForce;
	}
}
[BurstCompile(CompileSynchronously = true)]
public struct VoxelFlowDragForce : IJobParallelFor
{
	public NativeArray<float3> forces;
	[ReadOnly] public NativeArray<float3> velocities;
	[ReadOnly] public NativeArray<float4> voxels;
	[NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<WaterController.PathNode> riverPath;

	public float drag, voxelMass, flowRate;
	public static float InverseLerp(float3 a, float3 b, float3 value)
	{
		float3 AB = b - a;
		float3 AV = value - a;
		return math.dot(AV, AB) / math.dot(AB, AB);
	}


	public float3 GetFlowAtPoint(float3 point)
	{
		float3 flow = Vector3.forward;
		int closestPathIndex = 0;
		float closestPath = Mathf.Infinity;
		float bestLerp = 0;
		for (int i = 0; i < riverPath.Length - 1; i++)
		{
			float lerp = math.saturate(InverseLerp(riverPath[i].position, riverPath[i + 1].position, point));
			float distance = math.distancesq(point, math.lerp(riverPath[i].position, riverPath[i + 1].position, lerp));

			if (distance < closestPath)
			{
				bestLerp = lerp;
				closestPath = distance;
				closestPathIndex = i;
			}
		}

		float waterWidth = math.lerp(riverPath[closestPathIndex].waterWidth, riverPath[closestPathIndex + 1].waterWidth, bestLerp);

		float centerness = math.lerp(1, riverPath[closestPathIndex].edgeFlowStrength, math.saturate(math.sqrt(closestPath) / waterWidth));

		return flowRate * math.lerp(
			WaterController.GetTangent(closestPathIndex, riverPath),
			WaterController.GetTangent(closestPathIndex + 1, riverPath),
			 bestLerp) * centerness;
	}


	public void Execute(int index)
	{
		float submersion = voxels[index].w;
		if (submersion > 0)
		{
			float3 flow = GetFlowAtPoint(voxels[index].xyz);

			float3 relativeVelocity = (flow - velocities[index]);
			//Normal damping damps to 0, but we damp to the flow which is desired velocity
			float3 dragForce = relativeVelocity * drag * voxelMass * submersion;

			forces[index] += dragForce;
		}

	}
}
[BurstCompile(CompileSynchronously = true)]
public struct VoxelDragForce : IJobParallelFor
{
	public NativeArray<float3> forces;
	[ReadOnly] public NativeArray<float3> velocities;
	[ReadOnly] public NativeArray<float4> voxels;

	public float drag, voxelMass;

	public void Execute(int index)
	{
		float submersion = voxels[index].w;

		//Normal damping damps to 0, but we damp to the flow which is desired velocity
		float3 localDamping = -velocities[index].xyz * drag * voxelMass * submersion;

		forces[index] += localDamping;

	}
}


[BurstCompile(CompileSynchronously = true)]
public struct SumPointForces : IJob
{
	[ReadOnly] public NativeArray<float3> forces;
	[ReadOnly] public NativeArray<float4> points;
	[ReadOnly] public float3 rbCentre;


	[WriteOnly] public NativeArray<float3> output;
	public void Execute()
	{
		float3 totalForce = default;
		float3 totalTorque = default;


		for (int i = 0; i < points.Length; i++)
		{
			totalForce += forces[i];
			totalTorque += math.cross(points[i].xyz - rbCentre, forces[i]);
		}

		//Turn this torque and force back into a force and a position
		output[1] = -math.cross(totalTorque, totalForce) / math.dot(totalForce, totalForce);
		output[0] = totalForce;

	}
}




[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]

public class BuoyantBox : BuoyantBody
{

	float3 voxelSize;
	public int3 voxelCount = new int3(2, 2, 2);
	new BoxCollider collider;

	float voxelSpan;

	NativeArray<float4> voxels;
	NativeArray<float3> forces;
	NativeArray<float3> velocities;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		collider = GetComponent<BoxCollider>();

		rb.mass = collider.size.x * collider.size.y * collider.size.z * density;


		voxelSize = new float3(collider.size) / voxelCount;

		voxelSpan = Mathf.Max(voxelSize.x, voxelSize.y, voxelSize.z);


	}
	private void OnDestroy()
	{
		if (voxels.IsCreated)
		{
			voxels.Dispose();
			velocities.Dispose();
			forces.Dispose();
		}
	}
	JobHandle CalculateVoxels()
	{
		return new GenerateVoxels()
		{
			voxels = voxels,
			voxelSize = voxelSize,
			localToWorld = transform.localToWorldMatrix,
			voxelCount = voxelCount,
			volumeBoundsCentre = volume.bounds.center,
			volumeBoundsExtents = volume.bounds.extents,
			voxelSpan = voxelSpan,
			size = collider.size
		}.Schedule(voxels.Length, voxelCount.x);
	}
	JobHandle CalculateVoxelVelocities(JobHandle dependsOnVoxels = default(JobHandle))
	{
		return new VoxelVelocity()
		{
			voxels = voxels,
			velocities = velocities,
			worldCenter = rb.worldCenterOfMass,
			angularVel = rb.angularVelocity,
			bodyVelocity = rb.velocity
		}.Schedule(voxels.Length, voxelCount.x, dependsOnVoxels);
	}
	JobHandle CalculateForces(JobHandle dependsOnVoxels = default(JobHandle))
	{

		float voxelVolume = voxelSize.x * voxelSize.y * voxelSize.z;
		//Apply drag
		float voxelMass = voxelVolume * density;

		float3 buoyantForce = -Physics.gravity * voxelVolume * volume.density;

		return new VoxelBuoyancyForce()
		{
			voxels = voxels,
			forces = forces,
			buoyantForce = buoyantForce,

		}.Schedule(voxels.Length, voxelCount.x, dependsOnVoxels);

	}

	JobHandle CalculateDrag(JobHandle dependsOnVelocityFlowVoxels = default(JobHandle))
	{
		float voxelVolume = voxelSize.x * voxelSize.y * voxelSize.z;
		//Apply drag
		float voxelMass = voxelVolume * density;

		if (volume.path.Length > 1)
		{
			//Volume has a valid path
			//TODO: Allow this to be done multithreaded
			return new VoxelFlowDragForce()
			{
				voxels = voxels,
				forces = forces,
				velocities = velocities,
				voxelMass = voxelMass,
				drag = objectDrag * volume.template.viscosity,
				riverPath = volume.pathPositions,
				flowRate = volume.flowRate

			}.Schedule(voxels.Length, 1, dependsOnVelocityFlowVoxels);


		}
		else
		{
			//Volume has no path
			return new VoxelDragForce()
			{
				voxels = voxels,
				forces = forces,
				velocities = velocities,
				voxelMass = voxelMass,
				drag = objectDrag * volume.template.viscosity,

			}.Schedule(voxels.Length, voxelCount.x, dependsOnVelocityFlowVoxels);
		}

	}


	private void FixedUpdate()
	{
		if (volume != null)
		{

			var calculateVoxels = CalculateVoxels();

			var calculateVelocities = CalculateVoxelVelocities(calculateVoxels);

			var calculateForces = CalculateForces(calculateVoxels);

			var calculateDrag = CalculateDrag(JobHandle.CombineDependencies(calculateForces, calculateVelocities));

			var output = new NativeArray<float3>(2, Allocator.TempJob);

			var f = new SumPointForces()
			{
				points = voxels,
				forces = forces,
				rbCentre = rb.centerOfMass,
				output = output
			};



			f.Schedule(calculateDrag).Complete();
			if (math.all(math.isfinite(output[1])))
				rb.AddForceAtPosition(output[0], output[1]);

			output.Dispose();
		}
	}

	public override void OnWaterEnter(WaterController waterController)
	{
		base.OnWaterEnter(waterController);

		voxels = new NativeArray<float4>(voxelCount.x * voxelCount.y * voxelCount.z, Allocator.Persistent);
		velocities = new NativeArray<float3>(voxels.Length, Allocator.Persistent);
		forces = new NativeArray<float3>(voxels.Length, Allocator.Persistent);
	}
	public override void OnWaterExit(WaterController waterController)
	{
		base.OnWaterExit(waterController);
		voxels.Dispose();
		velocities.Dispose();
		forces.Dispose();
	}

	private void OnDrawGizmosSelected()
	{

		if (volume != null)
		{

			for (int i = 0; i < voxels.Length; i++)
			{
				Gizmos.color = Color.Lerp(Color.white, Color.red, voxels[i].w);
				Gizmos.DrawWireSphere(voxels[i].xyz, 0.05f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(voxels[i].xyz, voxels[i].xyz + velocities[i]);
			}

		}
	}
}
