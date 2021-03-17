using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(ContourGenerator))]
public class ContourGeneratorEditor : Editor
{
	ContourGenerator t => (ContourGenerator)target;
	Texture2D tex;
	private void OnEnable()
	{
		tex = new Texture2D(512, 512);
	}
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Generate Contours"))
		{
			double startTime = EditorApplication.timeSinceStartup;
			t.GenerateContours();
			Debug.LogFormat("Look {0} ms to calculate contours", (EditorApplication.timeSinceStartup - startTime) * 1000f);
		}

		if (GUILayout.Button("Generate Heightmap"))
		{
			double startTime = EditorApplication.timeSinceStartup;
			t.GenerateTerrainHeightmap();
			Debug.LogFormat("Look {0} ms to calculate heightmap", (EditorApplication.timeSinceStartup - startTime) * 1000f);
		}


		if (GUILayout.Button("Render Map"))
		{
			double startTime = EditorApplication.timeSinceStartup;



			string imagePath = AssetDatabase.GetAssetPath(target);
			imagePath = imagePath.Substring(0, imagePath.Length - 5) + "png";
			System.IO.File.WriteAllBytes(imagePath, tex.EncodeToPNG());
			AssetDatabase.Refresh();


			Debug.LogFormat("Look {0} ms to render map", (EditorApplication.timeSinceStartup - startTime) * 1000f);
		}

		RenderMap();
		GUILayout.Label(tex);

		base.OnInspectorGUI();
	}


	public void RenderMap()
	{
		CommandBuffer cmd = new CommandBuffer();

		RenderTexture map = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.Default);
		RenderTexture mapDepth = RenderTexture.GetTemporary(512, 512, 24, RenderTextureFormat.Depth);




		const float zNear = -50;
		const float zFar = 200;


		cmd.SetViewMatrix(Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.up)));
		cmd.SetProjectionMatrix(Matrix4x4.Ortho(t.mapCenter.x - t.mapExtents.x, t.mapCenter.x + t.mapExtents.x, t.mapCenter.y - t.mapExtents.y, t.mapCenter.y + t.mapExtents.y, zFar, zNear));



		cmd.SetRenderTarget(map.colorBuffer, mapDepth.depthBuffer);
		cmd.ClearRenderTarget(true, true, Color.black);

		MaterialPropertyBlock standard = new MaterialPropertyBlock();
		standard.SetVector("_Clipping", new Vector4(zFar, zNear));
		//standard.SetTexture("_Depth", t.test);


		foreach (var t in FindObjectsOfType<RenderOnMap>())
		{
			//Render target onto map
			if (t.TryGetComponent<MeshFilter>(out var r))
			{
				//Debug.Log("Drawing mesh");
				Vector3 scale = t.scale;

				//scale.y = -scale.y;
				cmd.DrawMesh(r.sharedMesh, t.transform.localToWorldMatrix * Matrix4x4.Scale(scale), t.renderMat, 0, 0, standard);
			}
			else if (this.t.useTerrain && t.TryGetComponent<Terrain>(out var terrain))
			{
				MaterialPropertyBlock block = new MaterialPropertyBlock();
				block.SetTexture("Heightmap", terrain.terrainData.heightmapTexture);

				block.SetFloat("_MaxTerrainHeight", terrain.terrainData.size.y * t.scale.y);
				block.SetVector("_Clipping", new Vector4(zFar, zNear));
				Vector3 pos = t.transform.position;
				pos.x += terrain.terrainData.size.x * 0.5f;
				pos.z += terrain.terrainData.size.z * 0.5f;
				Vector3 scale = new Vector3(-terrain.terrainData.size.x / 1000, 1, terrain.terrainData.size.z / 1000);
				cmd.DrawMesh(t.terrainMesh, Matrix4x4.TRS(pos, t.transform.rotation, scale), t.renderMat, 0, 0, block);


			}
		}

		var old = RenderTexture.active;

		RenderTexture.active = map;

		Graphics.ExecuteCommandBuffer(cmd);

		//Turn into png image
		tex.ReadPixels(new Rect(0, 0, map.width, map.height), 0, 0);
		tex.Apply();

		RenderTexture.active = old;
		RenderTexture.ReleaseTemporary(map);
		RenderTexture.ReleaseTemporary(mapDepth);



	}


}