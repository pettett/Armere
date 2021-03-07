using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(ContourGenerator))]
public class ContourGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Generate Contours"))
		{
			double startTime = EditorApplication.timeSinceStartup;
			(target as ContourGenerator).GenerateContours();
			Debug.LogFormat("Look {0} ms to calculate contours", (EditorApplication.timeSinceStartup - startTime) * 1000f);
		}

		if (GUILayout.Button("Generate Heightmap"))
		{
			double startTime = EditorApplication.timeSinceStartup;
			(target as ContourGenerator).GenerateTerrainHeightmap();
			Debug.LogFormat("Look {0} ms to calculate heightmap", (EditorApplication.timeSinceStartup - startTime) * 1000f);
		}


		if (GUILayout.Button("Render Map"))
		{
			double startTime = EditorApplication.timeSinceStartup;
			RenderMap();
			Debug.LogFormat("Look {0} ms to render map", (EditorApplication.timeSinceStartup - startTime) * 1000f);
		}


		base.OnInspectorGUI();
	}


	public void RenderMap()
	{
		CommandBuffer cmd = new CommandBuffer();
		var old = RenderTexture.active;
		RenderTexture map = new RenderTexture(512, 512, 16, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
		RenderTexture.active = map;



		cmd.SetViewMatrix(Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.up)));
		cmd.SetProjectionMatrix(Matrix4x4.Ortho(-500, 500, -500, 500, 400, 0));



		cmd.SetRenderTarget(map);




		foreach (var t in FindObjectsOfType<RenderOnMap>())
		{
			//Render target onto map
			if (t.TryGetComponent<MeshFilter>(out var r))
			{
				Debug.Log("Drawing mesh");
				cmd.DrawMesh(r.sharedMesh, t.transform.localToWorldMatrix * Matrix4x4.Scale(t.scale), t.renderMat, 0, 0);
			}
			else if (t.TryGetComponent<Terrain>(out var terrain))
			{
				t.renderMat.SetTexture("Heightmap", terrain.terrainData.heightmapTexture);
				cmd.DrawMesh(t.terrainMesh, Matrix4x4.Scale(new Vector3(-1, 2, 1)), t.renderMat, 0, 0);
			}
		}

		Graphics.ExecuteCommandBuffer(cmd);

		//Turn into png image
		Texture2D tex = new Texture2D(map.width, map.height);
		tex.ReadPixels(new Rect(0, 0, map.width, map.height), 0, 0);
		tex.Apply();


		string imagePath = AssetDatabase.GetAssetPath(target);
		imagePath = imagePath.Substring(0, imagePath.Length - 5) + "png";
		System.IO.File.WriteAllBytes(imagePath, tex.EncodeToPNG());
		AssetDatabase.Refresh();


		RenderTexture.active = old;

	}


}