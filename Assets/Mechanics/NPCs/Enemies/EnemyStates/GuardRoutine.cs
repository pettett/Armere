using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guard Routine", menuName = "Game/NPCs/Guard Routine", order = 0)]
public class GuardRoutine : AIStateTemplate
{
	public override AIState StartState(AIMachine c)
	{
		return new Guard(c, this);
	}
}
public class Guard : AIState<GuardRoutine>
{
	readonly Coroutine r;
	public Guard(AIMachine c, GuardRoutine t) : base(c, t)
	{
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		if (r != null)
			c.StopCoroutine(r);
	}

	public IEnumerator Routine()
	{
		c.debugText?.SetText("Guarding");


		int waypoint = c.GetClosestWaypoint();

		if (waypoint != -1)
			yield return c.GoToWaypoint(waypoint);
	}
}