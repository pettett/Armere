using System.Collections;
using UnityEngine;


public class DieRoutine : AIState
{
	public override string StateName => "Died";

	Coroutine r;
	public DieRoutine(AIMachine c) : base(c)
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
