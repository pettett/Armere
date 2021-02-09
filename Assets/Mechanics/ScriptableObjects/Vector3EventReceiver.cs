using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Vector3EventReceiver : MonoBehaviour
{
	public Vector3EventChannelSO eventChannel;
	public UnityEvent<Vector3> onEventRaised;
	private void OnEnable()
	{
		eventChannel.OnEventRaised += onEventRaised.Invoke;
	}
	private void OnDisable()
	{
		eventChannel.OnEventRaised -= onEventRaised.Invoke;
	}

}