using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoidEventChannelSO))]
public class VoidEventChannelEditor : Editor
{
	public VoidEventChannelSO t => (VoidEventChannelSO)target;
	void OnEnable()
	{
		t.OnEventRaised += OnEvent;
	}
	void OnDisable()
	{
		t.OnEventRaised -= OnEvent;
	}
	void OnEvent()
	{
		eventTimes.Add(Time.time);
		Repaint();
	}

	public List<float> eventTimes = new List<float>();

	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Raise event"))
		{
			t.RaiseEvent();
		}
		if (eventTimes.Count > 0 && eventTimes[0] < Time.time - 5)
		{
			eventTimes.RemoveAt(0);
		}

		for (int i = 0; i < eventTimes.Count; i++)
		{
			EditorGUILayout.HelpBox($"Event at: {eventTimes[i]}", MessageType.None);




		}

	}


}