using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{

	List<(string dir, SaveManager.SaveInfo info)> saves;


	SaveManager t => (SaveManager)target;

	public void OnEnable()
	{
		DiscoverSaves();
		t.onSavingFinish.onEventRaised += DiscoverSaves;
	}

	private void OnDisable()
	{
		t.onSavingFinish.onEventRaised -= DiscoverSaves;
	}

	public void DiscoverSaves()
	{
		saves = new List<(string dir, SaveManager.SaveInfo info)>();
		foreach (string dir in SaveManager.SaveFileSaveDirectories())
		{
			saves.Add((dir, SaveManager.LoadSaveInfo(dir)));
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (saves == null) return;
		if (GUILayout.Button("Reset all Saves"))
		{
			t.ResetSave();
			DiscoverSaves();
		}
		if (Application.isPlaying && GUILayout.Button("Save"))
		{
			t.SaveGameState();
			DiscoverSaves();
		}
		if (Application.isPlaying && GUILayout.Button("Load Blank Save"))
		{
			//Hard reload game
			t.LoadBlankSave(true);
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

			if (Application.isPlaying && GUILayout.Button("Load Save"))
			{
				Debug.Log("Loading selected save slot…");
				t.LoadSave(dir, true);
			}

			if (GUILayout.Button("Delete Save"))
			{
				Debug.Log("Deleting selected save slot…");
				t.DeleteSave(dir);
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