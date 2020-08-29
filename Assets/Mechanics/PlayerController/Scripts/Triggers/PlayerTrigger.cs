using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : TriggerBox
{
    public override void OnTrigger(Collider other)
    {
        if (other.TryGetComponent<PlayerController.Player_CharacterController>(out var p)) OnPlayerTrigger(p);
    }
    public virtual void OnPlayerTrigger(PlayerController.Player_CharacterController player) { }
}
