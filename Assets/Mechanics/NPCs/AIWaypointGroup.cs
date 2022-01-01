using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWaypointGroup : MonoBehaviour
{
	public struct Waypoint
	{
		public Transform transform;
		public int index;

		public Waypoint(Transform transform, int index)
		{
			this.transform = transform;
			this.index = index;
		}
	}

	//Contains utilities for moving between waypoints in group
	public Waypoint RandomWaypoint()
	{
		int i = Random.Range(0, Length);
		return new(this[i], i);
	}

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
