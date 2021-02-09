using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class Vector3VFXAttributeEventReceiver : MonoBehaviour
{
	public string attributeName;
	int nameID;
	public VisualEffect effect;
	public Vector3EventChannelSO eventChannel;
	public float multiplier = 1f;

	private void OnEnable()
	{
		nameID = Shader.PropertyToID(attributeName);
		eventChannel.OnEventRaised += OnEvent;
	}
	private void OnDisable()
	{
		eventChannel.OnEventRaised -= OnEvent;
	}
	public void OnEvent(Vector3 val)
	{
		effect.SetVector3(nameID, val * multiplier);
	}
}
