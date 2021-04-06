using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(NPCManager))]
public class NPCManagerEditor : Editor
{
	NPCManager m;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		m = target as NPCManager;

		float tempFloat;
		bool tempBool;
		foreach (string npcName in m.data.Keys)
		{
			EditorGUI.indentLevel = 0;

			EditorGUILayout.BeginFoldoutHeaderGroup(true, npcName.ToString());

			EditorGUI.indentLevel = 1;

			EditorGUI.BeginChangeCheck();
			tempBool = EditorGUILayout.Toggle("Spoken to", m.data[npcName].spokenTo);

			if (EditorGUI.EndChangeCheck())
			{
				m.data[npcName].spokenTo = tempBool;
				EditorUtility.SetDirty(m);
			}

			foreach (KeyValuePair<string, Yarn.Value> variable in m.data[npcName].variables)
			{

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(variable.Key);
				switch (variable.Value.type)
				{
					case Yarn.Value.Type.Number:
						EditorGUI.BeginChangeCheck();
						tempFloat = EditorGUILayout.FloatField(variable.Value.AsNumber);
						if (EditorGUI.EndChangeCheck())
						{
							m.data[npcName].AddVariable(variable.Key, tempFloat);
							EditorUtility.SetDirty(m);
						}
						break;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

	}
}