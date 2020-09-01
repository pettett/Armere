using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CuttableTree))]
public class CuttableTreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (target as CuttableTree);
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            t.UpdateMeshFilter();
        }
        GUI.enabled = false;
        EditorGUILayout.IntField("Additional Vertices:", t.meshFilter.sharedMesh.vertexCount - t.originalMesh.vertexCount);

    }
}

