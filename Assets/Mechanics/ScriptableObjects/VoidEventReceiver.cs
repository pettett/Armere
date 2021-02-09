using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class VoidEventReceiver : MonoBehaviour
{
	public VoidEventChannelSO eventChannel;
	public UnityEvent onEventRaised;
	private void OnEnable()
	{
		eventChannel.OnEventRaised += onEventRaised.Invoke;
	}
	private void OnDisable()
	{
		eventChannel.OnEventRaised -= onEventRaised.Invoke;
	}

}
