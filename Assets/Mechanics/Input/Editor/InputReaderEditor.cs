using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InputReader))]
public class InputReaderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (Application.isPlaying)
		{
			GUILayout.Label("Subscriptions");

			InputReader t = (InputReader)target;

			GUILayout.Label("Attack event");
			Subscriptions(t.attackEvent);

			GUILayout.Label("Move event");
			Subscriptions(t.movementEvent);

			GUILayout.Label("Camera move event");
			Subscriptions(t.cameraMoveEvent);

		}
	}

	public void Subscriptions(System.MulticastDelegate del)
	{
		if (del != null)
			foreach (var d in del.GetInvocationList())
			{
				if (d.Target is Object obj)
				{
					GUI.enabled = false;
					EditorGUILayout.ObjectField(d.Method.Name, obj, d.Target.GetType(), false);
					GUI.enabled = true;
				}
				else
				{
					GUILayout.Label($"{d.Method.Name}: {d.Target.ToString()}");
				}
			}
	}
}