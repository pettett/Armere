using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerTrigger : TriggerBox
{
	public override void OnTrigger(Collider other)
	{
		if (other.TryGetComponent<Armere.PlayerController.PlayerController>(out var p)) OnPlayerTrigger(p);
	}
	public abstract void OnPlayerTrigger(Armere.PlayerController.PlayerController player);
}
