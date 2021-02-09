using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnableBodyTrigger : TriggerBox
{
	public UnityEvent<SpawnableBody> onBodyEnter;
	public override void OnTrigger(Collider other)
	{
		if (other.TryGetComponent<SpawnableBody>(out var b))
		{
			onBodyEnter.Invoke(b);
		}
	}
}
