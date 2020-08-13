using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(Climbable))]
public class ClimbableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Climbable c = target as Climbable;
        if (GUILayout.Button("Sync Mesh"))
        {
            c.SyncMesh(c.mesh);
        }
        base.OnInspectorGUI();

    }

    private void OnSceneGUI()
    {
        Climbable c = target as Climbable;
        c.localTestPos = Handles.PositionHandle(c.localTestPos, Quaternion.identity);
    }


}