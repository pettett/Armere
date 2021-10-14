using System.Collections;
using UnityEngine;
public class KnockoutRoutine : AIState
{

	public override string StateName => "Knockout";

	readonly float knockoutTime;


	Coroutine r;
	public KnockoutRoutine(AIMachine c, float knockoutTime) : base(c)
	{
		this.knockoutTime = knockoutTime;
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}

	IEnumerator Routine()
	{
		c.ragdoller.RagdollEnabled = true;
		c.GetComponent<Focusable>().enabled = false;
		yield return new WaitForSeconds(knockoutTime);
		c.GetComponent<Focusable>().enabled = true;
		c.ragdoller.RagdollEnabled = false;
	}
}
