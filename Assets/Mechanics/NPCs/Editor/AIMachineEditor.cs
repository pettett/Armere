using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AIMachine))]
public class PlayerMachineEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		AIMachine t = (AIMachine)target;

		for (int i = 0; i < t.currentStates.Count; i++)
		{
			GUILayout.Label(t.currentStates[i].StateName);
		}

	}
}
