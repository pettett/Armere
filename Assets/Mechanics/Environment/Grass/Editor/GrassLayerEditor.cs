using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrassLayer))]
public class GrassLayerEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUI.enabled = false;
		EditorGUILayout.FloatField("Loaded Cells:", ((GrassLayer)target).loadedCellsCount);
		EditorGUILayout.FloatField("Thread Groups:", ((GrassLayer)target).GetThreadGroups());
		EditorGUILayout.FloatField("Loaded Chunks:", ((GrassLayer)target).loadedChunks.Count);

		//Preseve rect
		Rect r = GUILayoutUtility.GetRect(20, ((GrassLayer)target).occupiedBufferCells.Length + 2);
		EditorGUI.DrawRect(r, Color.white);

		for (int i = 0; i < ((GrassLayer)target).occupiedBufferCells.Length; i++)
		{
			if (((GrassLayer)target).occupiedBufferCells[i] != 0)
			{
				//Draw color reprosenting chunk
				Random.InitState(((GrassLayer)target).occupiedBufferCells[i]);
				EditorGUI.DrawRect(new Rect(r.x + 5, r.y + i, r.width - 10, r.y + i + 1), Random.ColorHSV());
			}
		}
	}
}