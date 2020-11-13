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

        EditorGUILayout.LabelField($"Width: {IntPower(2, t.resolutionPower)}");

        if (GUILayout.Button("Bake texture"))
        {
            int width = IntPower(2, t.resolutionPower);
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



    public static int IntPower(short x, short power)
    {
        if (power == 0) return 1;
        if (power == 1) return x;
        // ----------------------
        int n = 15;
        while ((power <<= 1) >= 0) n--;

        int tmp = x;
        while (--n > 0)
            tmp = tmp * tmp *
                 (((power <<= 1) < 0) ? x : 1);
        return tmp;
    }

}