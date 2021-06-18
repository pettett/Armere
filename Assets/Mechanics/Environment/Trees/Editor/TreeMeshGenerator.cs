using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreeMeshGenerator : EditorWindow
{

	[MenuItem("Armere/Tree Mesh Generator")]
	private static void ShowWindow()
	{
		var window = GetWindow<TreeMeshGenerator>();
		window.titleContent = new GUIContent("Tree Mesh Generator");
		window.Show();
	}
	[System.Serializable]
	struct ViewableMesh
	{
		public Mesh mesh;
		MeshPreview m;
		public void OnUpdate()
		{
			if (m != null)
			{
				m.Dispose();
			}
			m = new MeshPreview(mesh);
		}
		public void Draw(Rect rect, GUIStyle bgColor)
		{
			if (mesh != null)
			{
				if (m == null) OnUpdate();
				m.OnPreviewGUI(rect, bgColor);
			}
		}
	}
	public CuttableTreeProfile treeMesh;
	ViewableMesh treeBranches;
	ViewableMesh treeLeaves;

	Texture2D previewBackgroundTexture;



	//Every part of the tree
	ViewableMesh fullTreeMesh;
	//Just main trunk below the cut
	ViewableMesh stumpMesh;
	//Just main trunk above the cut
	ViewableMesh logMesh;
	//Everything but the main trunk
	ViewableMesh canopyMesh;


	private void OnEnable()
	{
		previewBackgroundTexture = Texture2D.blackTexture;
	}
	private void OnGUI()
	{
		ObjectField("Tree Branches", ref treeBranches.mesh);
		ObjectField("Tree Leaves", ref treeLeaves.mesh);

		ObjectField("Tree Leaves", ref treeMesh);

		GUIStyle bgColor = new GUIStyle();
		bgColor.normal.background = previewBackgroundTexture;


		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical();
		GUILayout.Label("Branches");
		Rect r1 = GUILayoutUtility.GetRect(200, 200);
		GUILayout.EndVertical();

		GUILayout.Space(10);

		GUILayout.BeginVertical();
		GUILayout.Label("Leaves");
		Rect r2 = GUILayoutUtility.GetRect(200, 200);
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();



		treeBranches.Draw(r1, bgColor);
		treeLeaves.Draw(r2, bgColor);




		GUILayout.Space(10);

		if (GUILayout.Button("Generate"))
		{
			GenerateTree();
		}


		GUILayout.Space(10);


		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical();
		GUILayout.Label("Full Tree");
		r1 = GUILayoutUtility.GetRect(200, 200);
		GUILayout.EndVertical();

		GUILayout.Space(10);

		GUILayout.BeginVertical();
		GUILayout.Label("Stump Mesh");
		r2 = GUILayoutUtility.GetRect(200, 200);
		GUILayout.EndVertical();

		GUILayout.Space(10);


		GUILayout.BeginVertical();
		GUILayout.Label("Log Mesh");
		var r3 = GUILayoutUtility.GetRect(200, 200);
		GUILayout.EndVertical();

		GUILayout.Space(10);

		GUILayout.BeginVertical();
		GUILayout.Label("Canopy Mesh");

		var r4 = GUILayoutUtility.GetRect(200, 200);
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();


		fullTreeMesh.Draw(r1, bgColor);
		stumpMesh.Draw(r2, bgColor);
		logMesh.Draw(r3, bgColor);
		canopyMesh.Draw(r4, bgColor);

	}
	void RemoveVertex(int vert, List<Vector3> verts, List<Vector3> normals, List<Color> cols, List<Vector2> uv, List<int> tri)
	{
		verts.RemoveAt(vert);
		normals.RemoveAt(vert);
		cols?.RemoveAt(vert);
		uv.RemoveAt(vert);

		//Going backword means removeing items in list does not effect next items
		for (int i = tri.Count / 3 - 1; i >= 0; i--)
		{
			if (tri[i * 3] == vert || tri[i * 3 + 1] == vert || tri[i * 3 + 2] == vert)
			{
				tri.RemoveAt(i * 3 + 2);
				tri.RemoveAt(i * 3 + 1);
				tri.RemoveAt(i * 3);
			}
			else
			{

				if (tri[i * 3] > vert) tri[i * 3]--;
				if (tri[i * 3 + 1] > vert) tri[i * 3 + 1]--;
				if (tri[i * 3 + 2] > vert) tri[i * 3 + 2]--;
			}
		}
	}
	void RemoveUnreferencedVerts(List<Vector3> verts, List<Vector3> normals, List<Vector2> uv, List<int> tri)
	{
		SortedSet<int> remove = new SortedSet<int>();

		for (int i = 0; i < verts.Count; i++)
		{
			if (!tri.Contains(i))
				remove.Add(i);
		}
		foreach (var item in remove.Reverse())
		{

			verts.RemoveAt(item);
			normals.RemoveAt(item);
			uv.RemoveAt(item);
			for (int i = 0; i < tri.Count; i++)
			{
				if (item < tri[i]) tri[i]--;
			}
		}

	}

	public void GenerateTree()
	{
		//Full tree mesh
		fullTreeMesh.mesh = new Mesh();

		fullTreeMesh.mesh.vertices = Join(treeBranches.mesh.vertices, treeLeaves.mesh.vertices);
		fullTreeMesh.mesh.normals = Join(treeBranches.mesh.normals, treeLeaves.mesh.normals);

		fullTreeMesh.mesh.uv = Join(treeBranches.mesh.uv, treeLeaves.mesh.uv);
		fullTreeMesh.mesh.subMeshCount = 2;
		fullTreeMesh.mesh.SetTriangles(treeBranches.mesh.triangles, 0);
		//Shift tris for leaf layer
		int[] tris = treeLeaves.mesh.triangles;
		for (int i = 0; i < tris.Length; i++)
		{
			tris[i] += treeBranches.mesh.vertices.Length;
		}

		fullTreeMesh.mesh.SetTriangles(tris, 1);



		Debug.Log("Created Full mesh");

		//stump and log meshes
		List<Vector3> verts = new List<Vector3>();
		treeBranches.mesh.GetVertices(verts);
		List<Vector3> normals = new List<Vector3>();
		treeBranches.mesh.GetNormals(normals);
		List<Color> cols = new List<Color>();
		treeBranches.mesh.GetColors(cols);
		List<int> tri = new List<int>();
		treeBranches.mesh.GetTriangles(tri, 0);
		List<Vector2> uv = new List<Vector2>();
		treeBranches.mesh.GetUVs(0, uv);



		for (int i = verts.Count - 1; i >= 0; i--)
		{
			if (cols[i].r > 0.05f || cols[i].g > 0.05f || cols[i].b > 0.05f)
			{
				//Not main trunk, remove
				RemoveVertex(i, verts, normals, cols, uv, tri);
			}
		}

		var stumpVerts = new List<Vector3>(verts);
		var stumpNormals = new List<Vector3>(normals);
		var stumpUVs = new List<Vector2>(uv);
		var stumpTris = new List<int>(tri);



		//Remove verts in stump region on log

		for (int i = tri.Count / 3 - 1; i >= 0; i--)
		{
			int v1 = tri[i * 3 + 0], v2 = tri[i * 3 + 1], v3 = tri[i * 3 + 2];


			if (verts[v1].y < treeMesh.cutHeight &&
				verts[v2].y < treeMesh.cutHeight &&
				verts[v3].y < treeMesh.cutHeight)
			{
				//Remove triangle
				tri.RemoveAt(i * 3 + 2);
				tri.RemoveAt(i * 3 + 1);
				tri.RemoveAt(i * 3 + 0);
			}
		}


		RemoveUnreferencedVerts(verts, normals, uv, tri);
		logMesh.mesh = new Mesh();

		logMesh.mesh.SetVertices(verts);
		logMesh.mesh.SetNormals(normals);
		logMesh.mesh.SetUVs(0, uv);
		logMesh.mesh.SetTriangles(tri, 0);


		Debug.Log("Created log");

		for (int i = stumpTris.Count / 3 - 1; i >= 0; i--)
		{
			int v1 = stumpTris[i * 3 + 0], v2 = stumpTris[i * 3 + 1], v3 = stumpTris[i * 3 + 2];


			if (stumpVerts[v1].y > treeMesh.cutHeight &&
				stumpVerts[v2].y > treeMesh.cutHeight &&
				stumpVerts[v3].y > treeMesh.cutHeight)
			{
				//Remove triangle
				stumpTris.RemoveAt(i * 3 + 2);
				stumpTris.RemoveAt(i * 3 + 1);
				stumpTris.RemoveAt(i * 3 + 0);
			}
		}
		RemoveUnreferencedVerts(stumpVerts, stumpNormals, stumpUVs, stumpTris);

		stumpMesh.mesh = new Mesh();

		stumpMesh.mesh.SetVertices(stumpVerts);
		stumpMesh.mesh.SetNormals(stumpNormals);
		stumpMesh.mesh.SetUVs(0, stumpUVs);
		stumpMesh.mesh.SetTriangles(stumpTris, 0);



		Debug.Log("Created Stump");


		//Canopy mesh - inverse stump and log with extra leaves
		verts.Clear();
		treeBranches.mesh.GetVertices(verts);
		normals.Clear();
		treeBranches.mesh.GetNormals(normals);
		cols.Clear();
		treeBranches.mesh.GetColors(cols);
		tri.Clear();
		treeBranches.mesh.GetTriangles(tri, 0);
		uv.Clear();
		treeBranches.mesh.GetUVs(0, uv);




		for (int i = verts.Count - 1; i >= 0; i--)
		{
			if (cols[i].r < 0.05f && cols[i].g < 0.05f && cols[i].b < 0.05f)
			{
				//Not main trunk, remove
				RemoveVertex(i, verts, normals, cols, uv, tri);
			}
		}

		tris = treeLeaves.mesh.triangles;
		for (int i = 0; i < tris.Length; i++)
		{
			tris[i] += verts.Count;
		}

		canopyMesh.mesh = new Mesh();

		canopyMesh.mesh.vertices = Join(verts.ToArray(), treeLeaves.mesh.vertices);
		canopyMesh.mesh.normals = Join(normals.ToArray(), treeLeaves.mesh.normals);
		canopyMesh.mesh.uv = Join(uv.ToArray(), treeLeaves.mesh.uv);
		canopyMesh.mesh.subMeshCount = 2;
		canopyMesh.mesh.SetTriangles(tri, 0);
		canopyMesh.mesh.SetTriangles(tris, 1);




		Debug.Log("Created Canopy");

		fullTreeMesh.OnUpdate();
		stumpMesh.OnUpdate();
		logMesh.OnUpdate();
		canopyMesh.OnUpdate();

		fullTreeMesh.mesh.UploadMeshData(false);
		logMesh.mesh.UploadMeshData(false);
		stumpMesh.mesh.UploadMeshData(false);
		canopyMesh.mesh.UploadMeshData(false);

		fullTreeMesh.mesh.name = "Full Tree Mesh " + treeMesh.name;
		logMesh.mesh.name = "Log Mesh " + treeMesh.name;
		stumpMesh.mesh.name = "Stump Mesh " + treeMesh.name;
		canopyMesh.mesh.name = "Canopy Mesh " + treeMesh.name;

		var p = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(treeMesh));
		AssetDatabase.CreateFolder(p, treeMesh.name);

		p = System.IO.Path.Combine(p, treeMesh.name);

		AssetDatabase.CreateAsset(fullTreeMesh.mesh, System.IO.Path.Combine(p, fullTreeMesh.mesh.name + ".asset"));
		AssetDatabase.CreateAsset(stumpMesh.mesh, System.IO.Path.Combine(p, stumpMesh.mesh.name + ".asset"));
		AssetDatabase.CreateAsset(logMesh.mesh, System.IO.Path.Combine(p, logMesh.mesh.name + ".asset"));
		AssetDatabase.CreateAsset(canopyMesh.mesh, System.IO.Path.Combine(p, canopyMesh.mesh.name + ".asset"));

		AssetDatabase.SaveAssets();

		treeMesh.treeMesh.mesh = fullTreeMesh.mesh;
		treeMesh.stumpMesh.mesh = stumpMesh.mesh;
		treeMesh.logMesh.mesh = logMesh.mesh;
		treeMesh.canopyMesh = canopyMesh.mesh;

		treeMesh.BakeProfile();


	}


	static T[] Join<T>(T[] t1, T[] t2)
	{
		T[] t = new T[t1.Length + t2.Length];
		System.Array.Copy(t1, t, t1.Length);
		System.Array.Copy(t2, 0, t, t1.Length, t2.Length);
		return t;
	}


	void ObjectField<T>(string name, ref T obj) where T : UnityEngine.Object
	{
		obj = (T)EditorGUILayout.ObjectField(name, obj, typeof(T), false);
	}
}