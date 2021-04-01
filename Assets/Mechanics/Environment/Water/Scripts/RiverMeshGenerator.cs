using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class RiverMeshGenerator : MonoBehaviour
{
	public WaterController controller;
	public Mesh mesh;

	[MyBox.PositiveValueOnly]
	public float meshWidthMultiplier = 2;

	[MyBox.PositiveValueOnly]
	public int lengthwiseCuts = 0;


	public static float RemapToNorm(float value)
	{
		const float from1 = -1, to1 = 1, from2 = 0, to2 = 1;

		return (value + from1) / (to1 - from1) * (to2 - from2) + from2;
	}


	[MyBox.ButtonMethod]
	public void GenMesh()
	{
		int length = controller.path.Length;


		int lines = lengthwiseCuts + 2;


		NativeArray<Vector3> verts = new NativeArray<Vector3>(length * lines, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector3> normals = new NativeArray<Vector3>(length * lines, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector2> uv = new NativeArray<Vector2>(length * lines, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

		NativeArray<Vector4> flowColors = new NativeArray<Vector4>(length * lines, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

		int triStep = 6 * (lines - 1);

		NativeArray<int> tris = new NativeArray<int>((length * 6 - 6) * (lines - 1), Allocator.Temp, NativeArrayOptions.UninitializedMemory);

		Vector3 min = controller.path[0].transform.position;
		Vector3 max = controller.path[0].transform.position;

		Vector3 maxFlow = Vector3.zero;

		for (int i = 0; i < length; i++)
		{


			float baseFlow = controller.flowRate / controller.path[i].waterWidth;
			Vector3 flowDir = -controller.GetTangent(i);

			for (int j = 0; j < lines; j++)
			{
				float t = (float)j * 2 / (lines - 1) - 1f;

				float flow = Mathf.Lerp(Mathf.Clamp01((1 - Mathf.Abs(t) * meshWidthMultiplier)), 1, controller.path[i].edgeFlowStrength) * baseFlow;


				verts[i + length * j] = controller.Extrude(i, t * controller.path[i].waterWidth * meshWidthMultiplier) - transform.position;

				normals[i + length * j] = Vector3.up;

				min = Vector3.Min(min, verts[i + length * j]);

				max = Vector3.Max(max, verts[i + length * j]);

				flowColors[i + length * j] = flowDir * flow;

				if (i < length - 1 && j < lines - 1)
				{
					tris[i * triStep + 0 + j * 6] = i + 1 + length * j;
					tris[i * triStep + 1 + j * 6] = i + length * j;
					tris[i * triStep + 2 + j * 6] = i + length + length * j;
					tris[i * triStep + 3 + j * 6] = i + 1 + length * j;
					tris[i * triStep + 4 + j * 6] = i + length + length * j;
					tris[i * triStep + 5 + j * 6] = i + length + 1 + length * j;
				}

			}

			//verts[i] = controller.Extrude(i, controller.path[i].waterWidth * meshWidthMultiplier);
			//verts[i + length] = controller.Extrude(i, -controller.path[i].waterWidth * meshWidthMultiplier);

			//maxFlow = Vector3.Max(maxFlow, new Vector3(Math.Abs(flowColors[i].x), 0, Math.Abs(flowColors[i].z)));


		}

		for (int i = 0; i < verts.Length; i++)
		{
			uv[i] = new Vector2((verts[i].x - min.x) / (max.x - min.x), (verts[i].z - min.z) / (max.z - min.z));
		}


		mesh = new Mesh();
		mesh.SetVertices(verts);
		mesh.SetUVs(0, uv);
		mesh.SetNormals(normals);

		mesh.SetColors(flowColors);

		mesh.SetIndices(tris, MeshTopology.Triangles, 0);
		mesh.UploadMeshData(true);
	}
	[MyBox.ButtonMethod]
	public void ApplyMesh()
	{
		MeshFilter filter = GetComponent<MeshFilter>();
		if (filter == null)
		{
			gameObject.AddComponent<MeshFilter>();
		}
		filter.sharedMesh = mesh;


		// MaterialPropertyBlock block = new MaterialPropertyBlock();
		// block.SetVector("Constant_Flow", Vector3.zero);
		// GetComponent<MeshRenderer>().SetPropertyBlock(block);
	}

	// private void OnDrawGizmosSelected()
	// {
	// 	//Gizmos.matrix = transform.localToWorldMatrix;
	// 	Gizmos.DrawMesh(mesh);
	// }
}
