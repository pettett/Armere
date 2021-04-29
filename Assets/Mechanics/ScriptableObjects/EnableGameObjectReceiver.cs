using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableComponentReceiver : MonoBehaviour
{
	public MonoBehaviour component;

	public BoolEventChannelSO enabler;

	private void OnEnable()
	{
		enabler.OnEventRaised += Enable;
	}
	private void OnDisable()
	{

		enabler.OnEventRaised -= Enable;
	}
	public void Enable(bool enabled)
	{
		component.enabled = enabled;
	}
}
