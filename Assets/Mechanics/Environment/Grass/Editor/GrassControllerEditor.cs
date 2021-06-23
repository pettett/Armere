using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrassController))]
public class GrassControllerEditor : Editor
{
	GrassController t => (GrassController)target;
	bool showBuffers = false;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		showBuffers = EditorGUILayout.Toggle("Show Buffers", showBuffers);

		if (t.instances != null)
			for (int i = 0; i < t.instances.Length; i++)
			{

				GUI.enabled = false;
				ref GrassLayerInstance inst = ref t.instances[i];
				GUILayout.Label(inst.profile.name, EditorStyles.boldLabel);
				EditorGUILayout.FloatField("Loaded Cells:", inst.loadedCellsCount);
				EditorGUILayout.FloatField("Last Active Cell:", inst.lastActiveCellIndex);
				EditorGUILayout.FloatField("Max Loaded Cells:", inst.maxLoadedCells);
				EditorGUILayout.FloatField("Blades per cell:", inst.bladesInCell);
				EditorGUILayout.FloatField("Max Loaded Blades:", inst.maxLoadedBlades);
				EditorGUILayout.FloatField("Max Rendered Blades:", inst.maxRenderedBlades);
				EditorGUILayout.FloatField("Cell area:", inst.cellArea);

				EditorGUILayout.FloatField("Thread Groups:", inst.GetThreadGroups());
				EditorGUILayout.FloatField("Loaded Chunks:", inst.loadedChunks.Count);
				if (showBuffers)
				{
					EditorGUILayout.LabelField($"Exact blades: { inst.drawIndirectArgsBuffer.BeginWrite<uint>(1, 1)[0]}");
					inst.drawIndirectArgsBuffer.EndWrite<uint>(0);
				}
				// //Preseve rectct
				const int pad = 2;
				Rect r = GUILayoutUtility.GetRect(20, inst.occupiedBufferCells.Length + pad * 2);
				EditorGUI.DrawRect(r, Color.white);

				int chunkStart = 0;
				int chunkID = inst.occupiedBufferCells[0];
				void DrawChunk(int end)
				{
					if (chunkID != 0)
					{
						Random.InitState(chunkID);
						var rect = new Rect(r.x + 5, r.y + chunkStart + pad, r.width - 10, end - chunkStart);
						var col = Random.ColorHSV();
						var invCol = new Color(1 - col.r, 1 - col.g, 1 - col.b);
						EditorGUI.DrawRect(rect, col);
						GUI.Label(rect, $"Chunk {chunkID}", new GUIStyle() { normal = new GUIStyleState() { textColor = invCol } });
					}
				}

				for (int ii = 1; ii < inst.occupiedBufferCells.Length; ii++)
				{
					if (inst.occupiedBufferCells[ii] != chunkID)
					{
						//Draw color reprosenting chunk
						DrawChunk(ii);
						chunkID = inst.occupiedBufferCells[ii];
						chunkStart = ii;
					}
				}
				//Draw last chunk
				DrawChunk(inst.occupiedBufferCells.Length);

			}
		//Repaint();
	}
}