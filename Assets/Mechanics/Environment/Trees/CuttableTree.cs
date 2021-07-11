using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Assertions;

[System.Serializable]
public struct Triangle
{
	public int a;
	public int b;
	public int c;
	public int index;
	public bool pointingUpwards;

	public Triangle(int a, int b, int c, int index, bool pointingUpwards)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.index = index;
		this.pointingUpwards = pointingUpwards;
	}
}
public enum TriangleCutMode : byte { Full, Top, Base }
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class CuttableTree : MonoBehaviour, IAttackable, IExplosionEffector
{

	[System.Serializable]
	public struct CutVector
	{
		[Range(0, 2 * Mathf.PI)]
		public float angle;
		public float intensity;

		public CutVector(float angle, float intensity)
		{
			this.angle = angle;
			this.intensity = intensity;
		}
	}




	[System.NonSerialized] public MeshFilter meshFilter;
	[System.NonSerialized] public MeshCollider meshCollider;
	[System.NonSerialized] public MeshRenderer meshRenderer;
	[Header("References")]
	public CuttableTreeProfile profile;

	public List<CutVector> activeCutVectors = new List<CutVector>();

	float totalDamage = 0;


	public Vector3 offset => Vector3.up * profile.cutHeight;

	public void UpdateComponents()
	{
		Assert.IsTrue(TryGetComponent(out meshFilter));
		Assert.IsTrue(TryGetComponent(out meshCollider));
		Assert.IsTrue(TryGetComponent(out meshRenderer));
	}

	private void Start()
	{
		activeCutVectors = new List<CutVector>();
		UpdateComponents();

		UpdateMeshFilter(TriangleCutMode.Full);
	}

	public AttackResult Attack(DamageType flags, float damage, GameObject controller, Vector3 hitPosition)
	{
		if (flags == DamageType.Blunt) return AttackResult.None;
		else return CutTree(hitPosition, controller.transform.position);
	}

	public void OnExplosion(Vector3 source, float radius, float force)
	{
		if (enabled)
		{
			float intensity = (1 - (Vector3.SqrMagnitude(source - transform.position) / (radius * radius))) * 2 - 0.5f;

			//Debug.Log($"Tree hit by explosion from {source} with {intensity} force");
			if (intensity > 0.1f)
				CutTree(source, source, intensity);
		}
	}


	private void OnEnable()
	{
		TypeGroup<IAttackable>.allObjects.Add(this);
	}
	private void OnDisable()
	{
		TypeGroup<IAttackable>.allObjects.Remove(this);
	}

	public AttackResult CutTree(Vector3 hitPoint, Vector3 hitterPosition, float intensity = 0.2f)
	{
		Profiler.BeginSample("Cut Tree");
		if (totalDamage >= profile.damageToCut)
		{
			//Tree already destroyed, this should never happen
			//Debug.LogWarning("Hit tree that has already been destroyed");
			return AttackResult.None;
		}


		Vector3 direction = (hitPoint - transform.position);
		direction.y = 0;
		direction.Normalize();

		activeCutVectors.Add(new CutVector(Vector3.SignedAngle(transform.forward, direction, Vector3.up) * Mathf.Deg2Rad, intensity));
		totalDamage += intensity;

		if (profile.cutClips?.Valid() ?? false)
		{
			profile.soundProfile.position = transform.position;
			profile.audioEventChannelSO.RaiseEvent(profile.cutClips, profile.soundProfile);
		}
		//Cut or split the tree
		if (totalDamage < profile.damageToCut)
		{
			UpdateMeshFilter(TriangleCutMode.Full);
			Profiler.EndSample();
			return AttackResult.Damaged;
		}
		else
		{
			SplitTree(hitterPosition);
			Profiler.EndSample();
			return AttackResult.Damaged | AttackResult.Killed;
		}
		//Debug.Break();
	}

	public void UpdateMeshRendererMaterials(MeshRenderer renderer)
	{
		renderer.materials = profile.GetCutTreeMaterials();
	}

	public void SplitTree(Vector3 hitterPosition)
	{
		if (meshCollider == null) throw new System.Exception("Mesh collider required");

		Mesh stump = CreateCutMesh(TriangleCutMode.Base);
		meshFilter.sharedMesh = stump;
		meshCollider.sharedMesh = stump;

		UpdateMeshRendererMaterials(meshRenderer);

		Mesh trunkMesh = CreateCutMesh(TriangleCutMode.Top);
		GameObject log = Instantiate(profile.emptyLogPrefab);
		log.transform.SetPositionAndRotation(transform.position + Vector3.up * profile.cutSize * 0.5f, transform.rotation);

		MeshCollider logCollider = log.GetComponent<MeshCollider>();
		Rigidbody logRB = log.GetComponent<Rigidbody>();
		MeshFilter logFilter = log.GetComponent<MeshFilter>();
		MeshRenderer logRenderer = log.GetComponent<MeshRenderer>();
		CutLog cutLog = log.GetComponent<CutLog>();

		cutLog.originatingStump = gameObject;
		GameObject canopy = new GameObject("Canopy", typeof(MeshFilter), typeof(MeshRenderer));
		canopy.GetComponent<MeshFilter>().sharedMesh = profile.canopyMesh;
		canopy.GetComponent<MeshRenderer>().materials = profile.logMaterials;
		canopy.transform.SetParent(log.transform, false);
		cutLog.canopy = canopy;
		cutLog.lengthRegion = new Vector2(profile.cutHeight, profile.cutHeight + profile.logEstimateHeight);

		logCollider.convex = true;
		logCollider.sharedMesh = trunkMesh;
		logFilter.sharedMesh = trunkMesh;
		logRenderer.materials = profile.GetCutTreeMaterials();

		//Calculate the direction the log should fall in
		Vector3 playerDirection = transform.position - hitterPosition;
		playerDirection.y = 0;
		playerDirection.Normalize();

		logRB.mass = profile.LogMass;

		logRB.AddForceAtPosition(playerDirection * profile.logKnockingForce,
								logRB.centerOfMass + Vector3.up * profile.logEstimateHeight * 0.5f - playerDirection * profile.logEstimateRadius);

		//Disable the script
		enabled = false;
	}



	public int FindFullSubdivisions(float intensity)
	{
		//Base of 2 - intensity of 0 should have no subdivisions
		return intensity == 0 ? 1 : Mathf.Clamp(Mathf.FloorToInt(profile.subdivisionScalar * intensity) + profile.minSubdivisions, profile.minSubdivisions, profile.maxSubdivisions);
	}

	public int FindSubdivisions(TriangleCutMode cutMode, float intensity) => cutMode == TriangleCutMode.Full ?
													FindFullSubdivisions(intensity) * 2 :
													FindFullSubdivisions(intensity);

	public int LinePointCount(TriangleCutMode cutMode, float intensity) => FindSubdivisions(cutMode, intensity) * 2 + 2;



	public void UpdateMeshFilter(TriangleCutMode cutMode)
	{
		Profiler.BeginSample("Update Mesh Filter");
		if (meshFilter == null) throw new System.Exception("No mesh filter set to update");
		if (activeCutVectors.Count > 0) //Cut into the mesh
		{
			meshFilter.sharedMesh = CreateCutMesh(cutMode);
			UpdateMeshRendererMaterials(meshRenderer);
		}
		else //Do not cut into the mesh
		{
			meshFilter.sharedMesh = profile.treeMesh.mesh;
			meshRenderer.materials = profile.logMaterials;
		}
		meshCollider.sharedMesh = profile.treeMesh.mesh;
		Profiler.EndSample();
	}

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
	struct FindCutVectorsJob : IJob
	{
		[ReadOnly]
		NativeArray<CutVector> activeCutVectors;

		[WriteOnly]
		NativeArray<float3> cutVectors;

		public FindCutVectorsJob(NativeArray<CutVector> activeCutVectors, NativeArray<float3> cutVectors)
		{
			this.activeCutVectors = activeCutVectors;
			this.cutVectors = cutVectors;
		}

		public void Execute()
		{
			for (int i = 0; i < activeCutVectors.Length; i++)
			{
				cutVectors[i] = new float3(
					math.sin(activeCutVectors[i].angle), 0,
					math.cos(activeCutVectors[i].angle)) * activeCutVectors[i].intensity;
			}
		}
	}

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
	struct ProcessCutVectorsJob : IJob
	{
		[ReadOnly]
		NativeArray<float3> cutVectors;
		[ReadOnly]
		NativeArray<Triangle> cutCylinder;
		[ReadOnly]
		NativeArray<float3> vertices;
		[ReadOnly]
		NativeArray<float3> normals;

		NativeArray<float> cutIntensities;


		TriangleCutMode cutMode;
		float intensityCutoff, cutHeight;

		public ProcessCutVectorsJob(NativeArray<float3> cutVectors, NativeArray<Triangle> cutCylinder, NativeArray<float3> vertices, NativeArray<float3> normals, NativeArray<float> cutIntensities, TriangleCutMode cutMode, float intensityCutoff, float cutHeight)
		{
			this.cutVectors = cutVectors;
			this.cutCylinder = cutCylinder;
			this.vertices = vertices;
			this.normals = normals;
			this.cutIntensities = cutIntensities;
			this.cutMode = cutMode;
			this.intensityCutoff = intensityCutoff;
			this.cutHeight = cutHeight;
		}

		public void Execute()
		{
			float3 rightNormal;


			for (int i = 0; i < cutCylinder.Length; i++)
			{
				int other = cutCylinder[i].pointingUpwards ? cutCylinder[i].b : cutCylinder[i].c;
				rightNormal = math.normalize(math.lerp(normals[cutCylinder[i].a], normals[other],
							 math.unlerp(vertices[cutCylinder[i].a].y, vertices[other].y, cutHeight)));

				for (int j = 0; j < cutVectors.Length; j++)
				{
					if (cutMode == TriangleCutMode.Full)
						cutIntensities[i] += math.clamp(math.dot(rightNormal, cutVectors[j]), 0, 1);
					else
						cutIntensities[i] += math.dot(rightNormal, cutVectors[j]);
				}
				//Add a bevel effect to the stump

				if (cutMode != TriangleCutMode.Full)
				{
					//add the splinter effect
					if (cutIntensities[i] <= 0)
					{
						cutIntensities[i] *= 0.5f - (i % 2);
					}

					//cutIntensities[i] = Mathf.Clamp(cutIntensities[i], minSeveredIntensity, float.MaxValue);
				}
				else if (cutIntensities[i] < intensityCutoff) cutIntensities[i] = 0;
			}
		}
	}


	public Mesh CreateCutMesh(TriangleCutMode cutMode, System.Action<Vector3, string> label = null, System.Action<Vector3, Vector3> line = null)
	{
		Profiler.BeginSample("Create Cut Mesh");

		ref CuttableTreeProfile.CuttableCylinderMesh m = ref profile.GetMeshForCut(cutMode);


		Profiler.BeginSample("Load Mesh");
		//Get data from deep inside the unity c++ core. spooky
		NativeArray<float3> verts = new NativeArray<Vector3>(m.mesh.vertices, Allocator.TempJob).Reinterpret<float3>();
		NativeArray<float3> normals = new NativeArray<Vector3>(m.mesh.normals, Allocator.TempJob).Reinterpret<float3>();
		NativeArray<float2> uv = new NativeArray<Vector2>(m.mesh.uv, Allocator.TempJob).Reinterpret<float2>();

		Profiler.EndSample();


		Profiler.BeginSample("Create weights");

		NativeArray<CutVector> activeCutVectorsArray = new NativeArray<CutVector>(activeCutVectors.Count, Allocator.TempJob);
		for (int i = 0; i < activeCutVectors.Count; i++)
			activeCutVectorsArray[i] = activeCutVectors[i]; //Copy list - save garbage

		NativeArray<float3> cutVectors = new NativeArray<float3>(activeCutVectors.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

		//Find cut vectors
		new FindCutVectorsJob(activeCutVectorsArray, cutVectors).Run();

		activeCutVectorsArray.Dispose();

		NativeArray<float> cutIntensities = new NativeArray<float>(m.cutCylinder.Length, Allocator.TempJob, NativeArrayOptions.ClearMemory);
		NativeArray<Triangle> cutCylinder = new NativeArray<Triangle>(m.cutCylinder, Allocator.TempJob);

		//Process cut vectors, after 
		new ProcessCutVectorsJob(cutVectors, cutCylinder, verts, normals, cutIntensities, cutMode, profile.intensityCutoff, profile.cutHeight).Run();

		cutVectors.Dispose();

		Profiler.BeginSample("Calculate required vertices");
		int additionalVertices = 0;
		int triangleCount = 0;

		bool chainToLeft = false;

		NativeArray<int> meshTriangleOffsets = new NativeArray<int>(m.cutCylinder.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

		int[] triangleIndices = new int[m.cutCylinder.Length];
		for (int i = 0; i < m.cutCylinder.Length; i++)
			triangleIndices[i] = m.cutCylinder[i].index;

		//Get the triangles
		List<int> meshTriangles = new List<int>();
		m.mesh.GetTriangles(meshTriangles, 0);

		//And calculate the number of additional vertices that will be required
		for (int i = 0; i < m.cutCylinder.Length; i++)
		{
			int leftTriangle = i - 1;
			if (leftTriangle == -1) leftTriangle = m.cutCylinder.Length - 1;
			if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0) || cutMode != TriangleCutMode.Full)
			{
				additionalVertices += LinePointCount(cutMode, cutIntensities[i]) + (chainToLeft ? 0 : LinePointCount(cutMode, cutIntensities[leftTriangle]));

				//Calculate the number of triangles this cut will use
				bool up = m.cutCylinder[i].pointingUpwards;
				meshTriangleOffsets[i] = triangleCount;

				if (up && cutMode != TriangleCutMode.Base || !up && cutMode != TriangleCutMode.Top)
				{
					triangleCount += 3;
				}
				if (!up && cutMode != TriangleCutMode.Base || up && cutMode != TriangleCutMode.Top)
				{
					triangleCount += 2 * 3;
				}


				//Remove this triangle from the mesh
				meshTriangles.RemoveRange(triangleIndices[i], 3);
				for (int j = 0; j < triangleIndices.Length; j++)
				{
					if (triangleIndices[j] > triangleIndices[i]) triangleIndices[j] -= 3;
				}

				chainToLeft = profile.mergeFaces;
			}
			else
			{
				chainToLeft = false;
			}
		}



		for (int i = 0; i < m.cutCylinder.Length; i++)
			meshTriangleOffsets[i] += meshTriangles.Count;



		Profiler.EndSample();
		//DEBUG - draw sorted indexes
		// for (int i = 0; i < cylinderTriangles.Length; i++)
		// {
		//     Vector3 avg = (verts[cylinderTriangles[i].a] + verts[cylinderTriangles[i].b] + verts[cylinderTriangles[i].c]) / 3f + transform.position;
		//     label?.Invoke(avg, string.Format("t:{0} up:{1}", i, cylinderTriangles[i].pointingUpwards));
		// }




		int totalVertices = verts.Length + additionalVertices;

		Profiler.EndSample();
		Profiler.BeginSample("Create Vertex array");
		//Add all these triangles to the new mesh

		//Vector3[] newVertices = new Vector3[totalVertices];
		//Vector3[] newNormals = new Vector3[totalVertices];
		//Vector2[] newUVs = new Vector2[totalVertices];


		//verts.CopyTo(newVertices);
		// normals.CopyTo(newNormals);
		// uv.CopyTo(newUVs);

		//Each array needs to be sliced to the correct size

		//New array has greater size, so needs to copy after
		NativeArray<float3> newVertices = new NativeArray<float3>(totalVertices, Allocator.TempJob);

		newVertices.Slice(0, verts.Length).CopyFrom(verts);

		//New array has greater size, so needs to copy after
		NativeArray<float3> newNormals = new NativeArray<float3>(totalVertices, Allocator.TempJob);
		newNormals.Slice(0, normals.Length).CopyFrom(normals);
		normals.Dispose();

		//New array has greater size, so needs to copy after
		NativeArray<float2> newUVs = new NativeArray<float2>(totalVertices, Allocator.TempJob);

		newUVs.Slice(0, uv.Length).CopyFrom(uv);
		uv.Dispose();


		Profiler.EndSample();

		//Cut the edges
		Profiler.BeginSample("Remove unwanted triangles");
		chainToLeft = false;
		float halfCutSize = profile.cutSize / 2;




		List<int> cutTriangles = new List<int>();
		// if (m.mesh.subMeshCount > 1)//Some parts of the mesh will be pre - cut
		// 	m.mesh.GetTriangles(cutTriangles, 1);



		triangleCount += meshTriangles.Count;
		//Update the capacities of the lists for less allocations.


		NativeArray<int> newMeshTriangles = new NativeArray<int>(triangleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

		newMeshTriangles.Slice(0, meshTriangles.Count).CopyFrom(meshTriangles.ToArray());

		//meshTriangles.Capacity = triangleCount;
		//meshTriangles.AddRange(Enumerable.Repeat(0, triangleCount - meshTriangles.Count));



		Profiler.EndSample();

		//Time to actually cut after all this pre processing

		Profiler.BeginSample($"Cut {m.cutCylinder.Length} Triangles");


		//Run the burst function for creating the mesh
		//Runs without threading, but very much faster
		new CreateCutMeshJob(
			verts.Length, cutIntensities, cutCylinder,
			newVertices, newNormals, newUVs, newMeshTriangles,
			meshTriangleOffsets, cutMode, m.centerPoint, profile).Run();


		int vertexOffset = verts.Length;



		//Use this data to finally create the cuts
		for (int i = 0; i < m.cutCylinder.Length; i++)
		{
			//Blend between triangles on the left (-1) and the right (+1)
			int leftTriangle = i - 1;
			if (leftTriangle == -1) leftTriangle = m.cutCylinder.Length - 1;

			float leftIntensity = cutIntensities[leftTriangle];
			float rightIntensity = cutIntensities[i];

			if (!(leftIntensity == 0 && rightIntensity == 0) || cutMode != TriangleCutMode.Full)
			{
				//TODO: This should all be added into the burst job
				AddCutTriangles(cutMode, cutTriangles, vertexOffset, newVertices.Length, leftIntensity, rightIntensity, chainToLeft);




				//Track number of total verts (again)
				vertexOffset += LinePointCount(cutMode, rightIntensity) + (chainToLeft ? 0 : LinePointCount(cutMode, leftIntensity));

				//This triangle will be the first in a chain sharing vertices
				chainToLeft = profile.mergeFaces;
			}
			else
			{
				chainToLeft = false;
			}
			//rightIntensity = leftIntensity;
		}


		//Dispose now unneeded arrays
		cutIntensities.Dispose();
		cutCylinder.Dispose();
		meshTriangleOffsets.Dispose();
		verts.Dispose();


		Profiler.EndSample();

		//Copy added data to the array of all the data

		Mesh cutMesh = new Mesh();
		//Before locking in the vert counts for the cutting, remove all vertices not encompassed by the cut mode
		if (cutMode != TriangleCutMode.Full)
		{
			CutTopTriangles(cutMode, newVertices.Reinterpret<Vector3>(), newNormals.Reinterpret<Vector3>(), newUVs.Reinterpret<Vector2>(), cutTriangles, m, newMeshTriangles, cutMesh);
		}
		else
		{
			Profiler.BeginSample("Create Mesh");
			cutMesh.SetVertices(newVertices);
			cutMesh.SetNormals(newNormals);
			cutMesh.SetUVs(0, newUVs);
			cutMesh.SetUVs(1, newUVs);
		}

		newVertices.Dispose();
		newNormals.Dispose();
		newUVs.Dispose();

		//  print($"{meshTriangles.Count} : {triangleCount}");

		cutMesh.subMeshCount = profile.CrosssectionMaterialIndex + 1;

		cutMesh.SetIndices(newMeshTriangles, MeshTopology.Triangles, 0);
		for (int i = 1; i < m.mesh.subMeshCount; i++)
			cutMesh.SetTriangles(m.mesh.GetTriangles(i), i);

		cutMesh.SetTriangles(cutTriangles, profile.CrosssectionMaterialIndex);

		newMeshTriangles.Dispose();

		cutMesh.UploadMeshData(true);
		Profiler.EndSample();


		Profiler.EndSample();

		return cutMesh;
	}


	public void AddCutTriangles(TriangleCutMode cutMode, List<int> cutTriangles, int vertexOffset, int meshVertsLength, float leftIntensity, float rightIntensity, bool connectLeft)
	{
		//Cut the mesh
		int leftPointCount = LinePointCount(cutMode, leftIntensity);
		int leftSubdivisions = FindSubdivisions(cutMode, leftIntensity);
		int rightPointCount = LinePointCount(cutMode, rightIntensity);
		int rightSubdivisions = FindSubdivisions(cutMode, rightIntensity);


		const int left = 0;
		int right = leftPointCount; //Right side is one line over

		if (connectLeft) //Make right start from 0
			vertexOffset -= right;

		if (cutMode == TriangleCutMode.Base)
		{
			//Add triangle to cap piece
			cutTriangles.Add(vertexOffset + right + rightPointCount - 1);
			cutTriangles.Add(vertexOffset + left + leftPointCount - 1);
			cutTriangles.Add(meshVertsLength);
		}
		else if (cutMode == TriangleCutMode.Top)
		{
			//Add triangle to cap piece
			cutTriangles.Add(vertexOffset + left);
			cutTriangles.Add(vertexOffset + right);
			cutTriangles.Add(meshVertsLength);
		}


		bool reverseTriangles = leftSubdivisions < rightSubdivisions;

		int addRight = reverseTriangles ? 2 : 1;
		int addLeft = reverseTriangles ? 1 : 2;

		//Add all the triangles for the subdivisions
		for (int i = 0; i < Mathf.Min(leftSubdivisions, rightSubdivisions); i++)
		{

			//Triangle 1
			cutTriangles.Add(vertexOffset + left + 1 + i * 2);
			cutTriangles.Add(vertexOffset + left + 2 + i * 2);
			cutTriangles.Add(vertexOffset + right + addRight + i * 2);
			//Triangle 2
			cutTriangles.Add(vertexOffset + left + addLeft + i * 2);
			cutTriangles.Add(vertexOffset + right + 2 + i * 2);
			cutTriangles.Add(vertexOffset + right + 1 + i * 2);
		}
		if (rightSubdivisions > leftSubdivisions)
		{
			int requiredTriangles = rightSubdivisions - leftSubdivisions;
			//Add final additional triangles
			for (int i = 0; i < requiredTriangles; i++)
			{
				cutTriangles.Add(vertexOffset + left + leftPointCount - 2);
				cutTriangles.Add(vertexOffset + right + rightPointCount - 2 - i * 2);
				cutTriangles.Add(vertexOffset + right + rightPointCount - 3 - i * 2);
			}
		}

		else if (leftSubdivisions > rightSubdivisions)
		{
			//Add final additional triangles

			int requiredTriangles = leftSubdivisions - rightSubdivisions;
			for (int i = 0; i < requiredTriangles; i++)
			{

				cutTriangles.Add(vertexOffset + left + leftPointCount - 3 - i * 2);
				cutTriangles.Add(vertexOffset + left + leftPointCount - 2 - i * 2);
				cutTriangles.Add(vertexOffset + right + rightPointCount - 2);
			}
		}

	}

	//Moved removing top or bottom part of method to separate method to 
	//reduce JIT time (likely to cut full then top + bottom so this separates the two things)
	public void CutTopTriangles(
		TriangleCutMode cutMode, NativeArray<Vector3> newVertices, NativeArray<Vector3> newNormals,
		NativeArray<Vector2> newUVs, List<int> cutTriangles, CuttableTreeProfile.CuttableCylinderMesh m, NativeArray<int> newMeshTriangles, Mesh cutMesh)
	{
		Profiler.BeginSample("Copy to new lists");
		List<Vector3> cutVerts = newVertices.ToList();
		List<Vector3> cutNormals = newNormals.ToList();
		List<Vector2> cutUVs = newUVs.ToList();
		Profiler.EndSample();
		//Add the vert for the cap / base part

		cutVerts.Add(m.centerPoint);
		cutNormals.Add(cutMode == TriangleCutMode.Base ? Vector3.up : Vector3.down); //Point normal in correct direction
																					 //This should be in the center
		cutUVs.Add(Vector2.one * 0.5f);


		Profiler.BeginSample("Remove tall vertices");
		void Remove(int index)
		{
			cutVerts.RemoveAt(index);
			cutNormals.RemoveAt(index);
			cutUVs.RemoveAt(index);
			for (int t = 0; t < cutTriangles.Count; t += 3)
			{
				if (cutTriangles[t] > index) cutTriangles[t]--;
				if (cutTriangles[t + 1] > index) cutTriangles[t + 1]--;
				if (cutTriangles[t + 2] > index) cutTriangles[t + 2]--;
			}
			for (int t = 0; t < newMeshTriangles.Length; t += 3)
			{
				if (newMeshTriangles[t] > index) newMeshTriangles[t]--;
				if (newMeshTriangles[t + 1] > index) newMeshTriangles[t + 1]--;
				if (newMeshTriangles[t + 2] > index) newMeshTriangles[t + 2]--;
			}
		}
		//Use a hashset to hold every *unique* vertex being removed
		SortedSet<int> toBeRemoved = new SortedSet<int>();

		//If at top, remove cylinder ring at base
		for (int i = 0; i < m.cutCylinder.Length; i++)
		{

			if (m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Base || !m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Top)
			{
				toBeRemoved.Add(m.cutCylinder[i].a);
			}
			if (!m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Base || m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Top)
			{
				toBeRemoved.Add(m.cutCylinder[i].b);
				toBeRemoved.Add(m.cutCylinder[i].c);
			}
		}
		//Then removed all of them backwards so no offset errors from list size chaning
		foreach (int index in toBeRemoved.Reverse())
		{
			Remove(index);
		}

		//If at base, remove cylinder ring at top

		Profiler.EndSample();

		Profiler.BeginSample("Create Mesh");
		cutMesh.SetVertices(cutVerts);
		cutMesh.SetNormals(cutNormals);
		cutMesh.SetUVs(0, cutUVs);
		cutMesh.SetUVs(1, cutUVs);
	}




}
