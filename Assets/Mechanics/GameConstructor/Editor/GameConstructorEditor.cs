using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
[CustomEditor(typeof(GameConstructor))]
public class GameConstructorEditor : Editor
{

	List<(string dir, SaveManager.SaveInfo info)> saves;


	GameConstructor t => (GameConstructor)target;

	public void OnEnable()
	{
		DiscoverSaves();
		t.onSavingFinish.OnEventRaised += DiscoverSaves;
	}

	private void OnDisable()
	{
		t.onSavingFinish.OnEventRaised -= DiscoverSaves;
	}

	public void DiscoverSaves()
	{
		saves = new List<(string dir, SaveManager.SaveInfo info)>();
		foreach (string dir in SaveManager.SaveFileSaveDirectories())
		{
			Debug.Log($"Loading {dir}");
			saves.Add((dir, SaveManager.LoadSaveInfo(dir)));
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (saves == null) return;


		if (GUILayout.Button("Open Saves Folder"))
		{
			var rootDir = SaveManager.SaveDirName().Replace(@"/", @"\");
			System.Diagnostics.Process.Start("explorer.exe", "/select," + rootDir);
		}

		if (GUILayout.Button("Discover all Saves"))
		{
			DiscoverSaves();
		}

		if (GUILayout.Button("Reset all Saves"))
		{
			SaveManager.ResetSave();
			DiscoverSaves();
		}
		if (Application.isPlaying && GUILayout.Button("Save"))
		{
			t.triggerSave.RaiseEvent();
			DiscoverSaves();
		}
		if (Application.isPlaying && GUILayout.Button("Load Blank Save"))
		{
			//Hard reload game
			t.LoadNewGame();
		}

		//Render all of the saves
		foreach ((string dir, SaveManager.SaveInfo info) in saves)
		{

			Rect r = EditorGUILayout.BeginHorizontal("Button");

			// var rect = GUILayoutUtility.GetAspectRect(1.6f);
			// GUI.DrawTexture(rect, info.thumbnail, ScaleMode.ScaleAndCrop);

			GUILayout.Label(info.thumbnail);

			EditorGUILayout.BeginVertical();


			GUILayout.Label(info.regionName);
			GUILayout.Label(info.AdaptiveTime());
			if (GUILayout.Button("Open Folder"))
			{
				var rootDir = dir.Replace(@"/", @"\");
				System.Diagnostics.Process.Start("explorer.exe", "/select," + rootDir);
			}
			if (Application.isPlaying)
			{
				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button("Hard Load"))
				{
					Debug.Log("Loading selected save slot…");
					t.LoadSave(dir);
				}
				if (GUILayout.Button("Soft Load"))
				{
					Debug.Log("Loading selected save slot…");
					t.LoadSave(dir);
				}

				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Delete Save"))
			{
				Debug.Log("Deleting selected save slot…");
				SaveManager.DeleteSave(dir);
				DiscoverSaves();
			}



			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();



		}

	}

	public void OpenSaveFolder()
	{
		string rootDir = Application.persistentDataPath + "/saves/save1";
		rootDir = rootDir.Replace(@"/", @"\");   // explorer doesn't like front slashes
		System.Diagnostics.Process.Start("explorer.exe", "/select," + rootDir);
	}
}