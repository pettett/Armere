using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ContourGenerator))]
public class ContourGeneratorEditor : Editor{
    public override void OnInspectorGUI(){
        if(GUILayout.Button("Generate Contours")){
            (target as ContourGenerator).GenerateContours();
        }
        base.OnInspectorGUI();
    }
}