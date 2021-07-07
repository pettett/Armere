using UnityEngine;
using UnityEditor;
using Armere.PlayerController;

[CustomEditor(typeof(PlayerMachine))]
public class PlayerMachineEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		PlayerMachine t = (PlayerMachine)target;

		for (int i = 0; i < t.currentStates.Count; i++)
		{
			GUILayout.Label(t.currentStates[i].StateName);
		}

	}
}
