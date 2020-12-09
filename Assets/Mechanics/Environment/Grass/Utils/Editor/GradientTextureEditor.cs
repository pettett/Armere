using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(GradientTexture))]
public class GradientTextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GradientTexture t = (GradientTexture)target;

        EditorGUILayout.LabelField($"Width: {1 << t.resolutionPower}");

        if (GUILayout.Button("Bake texture"))
        {
            int width = 1 << t.resolutionPower;
            Texture2D gradient = new Texture2D(width, 1);
            Color[] cols = new Color[width];
            for (int i = 0; i < width; i++)
            {
                cols[i] = t.gradient.Evaluate(i / ((float)width - 1));
            }
            gradient.SetPixels(cols);
            gradient.Apply();
            string imagePath = AssetDatabase.GetAssetPath(t);
            imagePath = imagePath.Substring(0, imagePath.Length - 5) + "png";
            System.IO.File.WriteAllBytes(imagePath, gradient.EncodeToPNG());
            AssetDatabase.Refresh();
        }
    }





}