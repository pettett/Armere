using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPositionEvent : MonoBehaviour
{
	public Vector3EventChannelSO positionEventChannel;

	public void Trigger()
	{
		positionEventChannel.RaiseEvent(transform.position);
	}
}
