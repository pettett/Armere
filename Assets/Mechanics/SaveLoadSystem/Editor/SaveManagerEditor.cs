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
        if (GUILayout.Button("Reset Save")){
            t.ResetSave();
        }
            //Render all of the saves
            foreach ((string dir, SaveManager.SaveInfo info) in saves)
            {

                Rect r = EditorGUILayout.BeginHorizontal("Button");
                if (GUI.Button(r, GUIContent.none))
                {
                    //Clicked on save
                }


                // var rect = GUILayoutUtility.GetAspectRect(1.6f);
                // GUI.DrawTexture(rect, info.thumbnail, ScaleMode.ScaleAndCrop);

                GUILayout.Label(info.thumbnail);

                EditorGUILayout.BeginVertical();
    
    
                GUILayout.Label(info.regionName);
                if (GUILayout.Button(dir.Split(Path.DirectorySeparatorChar).Last()))
                {
                    var rootDir = dir.Replace(@"/", @"\");
                    System.Diagnostics.Process.Start("explorer.exe", "/select," + rootDir);
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