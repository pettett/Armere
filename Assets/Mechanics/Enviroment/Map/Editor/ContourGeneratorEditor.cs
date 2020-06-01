using UnityEngine;
using UnityEditor;


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

        base.OnInspectorGUI();
    }
}