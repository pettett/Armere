using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWaypointGroup : MonoBehaviour
{
    //Contains utilities for moving between waypoints in group
    public Transform RandomWaypoint() => transform.GetChild(Random.Range(0, Length));
    public Transform this[int index] => transform.GetChild(index);
    public int Length => transform.childCount;


    private void OnDrawGizmos()
    {
        if (Length >= 2)
        {
            for (int i = 0; i < Length - 1; i++)
            {
                Gizmos.DrawLine(this[i].position, this[i + 1].position);
            }
            Gizmos.DrawLine(this[0].position, this[Length - 1].position);
        }
    }

}
