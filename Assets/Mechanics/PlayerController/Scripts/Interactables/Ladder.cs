using System.Collections;
using System.Collections.Generic;
using PlayerController;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    public float rungDistance = 0.25f;
    public float ladderHeight = 12;

    public void Interact(Player_CharacterController c)
    {
        //Change to state ladder

        //Maybe change this to happen inside the player controller?
        c.ChangeToState<LadderClimb>(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * ladderHeight);
    }

}
