using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class EditorLevelManager
{
	[MenuItem("Levels/stealthTest")] public static void LoadStealthTest() => LoadEditorLevel("stealthTest");
	[MenuItem("Levels/Headlands")] public static void LoadHeadlands() => LoadEditorLevel("Headlands");
	[MenuItem("Levels/Mobility Dungeon")] public static void LoadMobility() => LoadEditorLevel("MobilityDungeon");
	[MenuItem("Levels/Enemy Dungeon")] public static void LoadEnemyDungeon() => LoadEditorLevel("EnemyDungeon");
	[MenuItem("Levels/Test Town")] public static void LoadTestTown() => LoadEditorLevel("TestTown");
	[MenuItem("Levels/Combat Test")] public static void LoadCombatTest() => LoadEditorLevel("combatTest");



	public static void LoadEditorLevel(string level)
	{
		if (!Application.isPlaying)
		{
			if (EditorSceneManager.SaveOpenScenes())
			{
				EditorSceneManager.OpenScene($"Assets/Levels/{level}/{level}.unity", OpenSceneMode.Single);
				EditorSceneManager.OpenScene("Assets/Levels/MainScene.unity", OpenSceneMode.Additive);
			}
			else
			{
				Debug.LogWarning("Scenes not saved, cannot change level");
			}
		}
	}
}
