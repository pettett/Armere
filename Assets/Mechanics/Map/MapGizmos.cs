using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class MapGizmos : MonoBehaviour
{
	const float zNear = 200;
	const float zFar = -50;
	private void OnDrawGizmos()
	{
		Matrix4x4 mat = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.down)) * Matrix4x4.Ortho(-128, 128, -128, 128, zNear, zFar) * Matrix4x4.Translate(new Vector3(0, 1000, 0));

		Gizmos.matrix = mat;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(256, 256, 400));
		Gizmos.DrawLine(new Vector3(0, 0, zNear), new Vector3(0, 0, zFar));

	}

	private void Update()
	{
		Matrix4x4 mat = Matrix4x4.Ortho(-128, 128, -128, 128, zNear, zFar) * Matrix4x4.Translate(new Vector3(0, 1000, 0));
		Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(-90, 0, 0)));
		foreach (var t in FindObjectsOfType<RenderOnMap>())
		{


			//Render target onto map
			if (t.TryGetComponent<MeshFilter>(out var r))
			{
				//Debug.Log("Drawing mesh");
				//Gizmos.matrix = mat * t.transform.localToWorldMatrix * Matrix4x4.Scale(t.scale);
				//Gizmos.DrawMesh(r.sharedMesh);
				Graphics.DrawMesh(r.sharedMesh, mat * rot * t.transform.localToWorldMatrix * Matrix4x4.Scale(t.scale), t.renderMat, 0);

				//cmd.DrawMesh(r.sharedMesh,), t.renderMat, 0, 0);
			}
			else if (t.TryGetComponent<Terrain>(out var terrain))
			{
				MaterialPropertyBlock block = new MaterialPropertyBlock();
				block.SetTexture("Heightmap", terrain.terrainData.heightmapTexture);
				block.SetFloat("_MaxTerrainHeight", terrain.terrainData.size.y * t.scale.y * 2);

				Vector3 pos = t.transform.position;
				pos.x += terrain.terrainData.size.x * 0.5f;
				pos.z += terrain.terrainData.size.z * 0.5f;

				Vector3 scale = new Vector3(-terrain.terrainData.size.x / 1000, 1, terrain.terrainData.size.z / 1000);

				Graphics.DrawMesh(t.terrainMesh, mat * rot * Matrix4x4.TRS(pos, t.transform.rotation, scale), t.renderMat, 0, null, 0, block);

				//Gizmos.matrix = mat * Matrix4x4.TRS(pos, t.transform.rotation, scale);
				//cmd.DrawMesh(t.terrainMesh, Matrix4x4.TRS(pos, t.transform.rotation, scale), t.renderMat, 0, 0, block);
				//	Gizmos.DrawMesh(t.terrainMesh);
			}
		}

		//Gizmos.matrix = mat;
		//Gizmos.DrawWireCube(Vector3.zero, new Vector3(256, 256, 400));
		//Gizmos.DrawLine(new Vector3(0, 0, zNear), new Vector3(0, 0, zFar));
	}
}
