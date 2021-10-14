using System.Collections;
using UnityEngine;
public class SearchForEventRoutine : AIState
{

	public override string StateName => "SearchForEventRoutine";

	readonly Vector3 eventPos;
	Coroutine r;



	public SearchForEventRoutine(AIMachine c, Vector3 eventPos) : base(c)
	{
		this.eventPos = eventPos;
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}
	IEnumerator Routine()
	{

		/*
		Investigate routine:
			Go to close enough distance to event
			Rotate to event
			Wait there, looking around a bit
			go back to what we were doing before
		*/

		c.debugText.SetText("Searching");
		yield return c.RotateTo(Quaternion.LookRotation(eventPos - c.transform.position), c.agent.angularSpeed);
		c.debugText.SetText("Searching - looking");
		yield return new WaitForSeconds(3);
	}
}
