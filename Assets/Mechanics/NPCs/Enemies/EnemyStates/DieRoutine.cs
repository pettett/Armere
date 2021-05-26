using System.Collections;
using UnityEngine;


public class DieRoutine : AIState
{
	public override bool alertOnAttack => false;

	public override bool searchOnEvent => false;

	public override bool investigateOnSight => false;
	Coroutine r;
	public DieRoutine(AIHumanoid c) : base(c)
	{

		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}
	IEnumerator Routine()
	{

		foreach (var x in c.weaponGraphics.holdables)
			x.RemoveHeld();

		c.ragdoller.RagdollEnabled = true;
		yield return new WaitForSeconds(4);
		MonoBehaviour.Destroy(c.gameObject);
	}
}
