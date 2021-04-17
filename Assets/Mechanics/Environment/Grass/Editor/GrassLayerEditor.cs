using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrassLayer))]
public class GrassLayerEditor : Editor
{
	GrassLayer t => (GrassLayer)target;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUI.enabled = false;
		EditorGUILayout.FloatField("Loaded Cells:", t.loadedCellsCount);
		EditorGUILayout.FloatField("Max Loaded Cells:", t.maxLoadedCells);
		EditorGUILayout.FloatField("Max Loaded Blades:", t.maxLoadedBlades);
		EditorGUILayout.FloatField("Max Rendered Blades:", t.maxRenderedBlades);

		EditorGUILayout.FloatField("Thread Groups:", t.GetThreadGroups());
		EditorGUILayout.FloatField("Loaded Chunks:", t.loadedChunks.Count);

		int count = t.occupiedBufferCells.Length / t.cellsInSmallestChunk;

		//Preseve rect
		Rect r = GUILayoutUtility.GetRect(20, count + 2);
		EditorGUI.DrawRect(r, Color.white);

		for (int i = 0; i < count; i++)
		{
			int index = i * t.cellsInSmallestChunk;
			if (((GrassLayer)target).occupiedBufferCells[index] != 0)
			{
				//Draw color reprosenting chunk
				Random.InitState(((GrassLayer)target).occupiedBufferCells[index]);

				float height = 1;

				EditorGUI.DrawRect(new Rect(r.x + 5, r.y + i, r.width - 10, height), Random.ColorHSV());
			}
		}

		Repaint();
	}
}