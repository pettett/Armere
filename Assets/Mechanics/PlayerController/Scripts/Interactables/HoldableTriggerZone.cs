using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableTriggerZone : MonoBehaviour
{
	[TagSelector] public string allowTag;
	public uint bodiesInZone;
	public IntEventChannelSO onCountChangedEventChannel;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger && other.TryGetComponent<HoldableBody>(out var b) && b.holdableTriggerTag == allowTag)
		{
			bodiesInZone++;
			//Update anything that cares about this trigger
			onCountChangedEventChannel?.RaiseEvent((int)bodiesInZone);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger && other.TryGetComponent<HoldableBody>(out var b) && b.holdableTriggerTag == allowTag)
		{
			bodiesInZone--;
			//Update anything that cares about this trigger
			onCountChangedEventChannel?.RaiseEvent((int)bodiesInZone);
		}
	}

}
