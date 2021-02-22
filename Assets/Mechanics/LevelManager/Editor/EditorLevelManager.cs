using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class EditorLevelManager
{
	[MenuItem("Levels/stealthTest")]
	public static void LoadStealthTest()
	{
		LoadEditorLevel("stealthTest");
	}
	[MenuItem("Levels/Headlands")]
	public static void LoadHeadlands()
	{
		LoadEditorLevel("Headlands");
	}


	public static void LoadEditorLevel(string level)
	{
		if (!Application.isPlaying)
		{
			EditorSceneManager.OpenScene($"Assets/Levels/{level}/{level}.unity", OpenSceneMode.Single);
			EditorSceneManager.OpenScene("Assets/Levels/MainScene.unity", OpenSceneMode.Additive);
		}
	}
}
