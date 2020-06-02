using System.Collections;
using System.Collections.Generic;
using PlayerController;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    public float rungDistance = 0.25f;
    public float rungOffset = 0.1f;
    public float ladderHeight = 12;


    public void Interact(Player_CharacterController c)
    {
        //Change to state ladder

        //Maybe change this to happen inside the player controller?
        c.ChangeToState<LadderClimb>(this);
    }
    public Vector3 LadderPosAtHeight(float height, float radius)
    {
        return transform.position - transform.forward * radius + Vector3.up * height;
    }

    public Vector3 LadderPosByRung(float rung, float right)
    {
        return transform.position + Vector3.up * (rung * rungDistance + rungOffset) + transform.right * right;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * ladderHeight);
        //Draw all the rungs
        int rungCount = Mathf.FloorToInt(ladderHeight / rungDistance);
        for (int i = 0; i < rungCount; i++)
        {
            Vector3 h = Vector3.up * (rungDistance * i + rungOffset);
            Gizmos.DrawLine(transform.position + transform.right * 0.2f + h, transform.position - transform.right * 0.2f + h);
        }
    }

}
