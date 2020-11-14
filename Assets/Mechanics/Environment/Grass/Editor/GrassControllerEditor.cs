// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// [CustomEditor(typeof(GrassController))]
// public class GrassControllerEditor : Editor
// {
//     float drawStrength = 0.5f;

//     Texture2D density;

//     private void OnEnable()
//     {
//         GrassController c = (GrassController)target;
//         density = new Texture2D(c.texSize, c.texSize, TextureFormat.R8, 0, true);
//         density.wrapMode = TextureWrapMode.Clamp;
//         if (c.grassDensity != null)
//             density.SetPixels(c.grassDensity.GetPixels());
//     }
//     private void OnDisable()
//     {
//         AssetDatabase.Refresh();
//     }
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

//         GrassController c = (GrassController)target;

//         drawStrength = EditorGUILayout.Slider(drawStrength, 0, 1);
//         Rect rect = GUILayoutUtility.GetAspectRect(1);
//         GUI.DrawTexture(rect, density);

//         GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

//         if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && rect.Contains(Event.current.mousePosition))
//         {

//             Vector2 localPos = (Event.current.mousePosition - rect.min) * c.texSize / rect.size;
//             int xCoord = Mathf.FloorToInt(localPos.x);
//             int yCoord = c.texSize - Mathf.FloorToInt(localPos.y) - 1;


//             Color[] pix = density.GetPixels();
//             pix[xCoord + yCoord * c.texSize] = Color.red * drawStrength;
//             c.cells1D = new bool[c.texSize * c.texSize];
//             for (int i = 0; i < pix.Length; i++)
//             {
//                 c.cells1D[i] = pix[i].r > 0.1f;
//             }

//             c.UpdateChunkTree();


//             density.SetPixels(pix);
//             density.Apply();

//             byte[] b = density.EncodeToPNG();
//             Debug.Log(AssetDatabase.GetAssetPath(c.grassDensity));
//             System.IO.File.WriteAllBytes(AssetDatabase.GetAssetPath(c.grassDensity), b);

//             Repaint();
//         }
//     }
// }