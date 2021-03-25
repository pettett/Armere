using System.Collections;
using System.Collections.Generic;
using Armere.Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "Auto Bow Routine", menuName = "Game/NPCs/Auto Bow Routine", order = 0)]
public class AutoBowRoutine : AIStateTemplate
{

	public BowItemData bowItem;
	public AmmoItemData ammoItem;
	public float bowFireRate = 1;
	public float arrowNotchTime = 0.1f;
	public float arrowSpeed = 100;


	public override AIState StartState(AIHumanoid c)
	{
		return new AutoBow(c, this);
	}
}

public class AutoBow : AIState<AutoBowRoutine>
{
	readonly Coroutine r;
	public AutoBow(AIHumanoid c, AutoBowRoutine t) : base(c, t)
	{
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}

	public IEnumerator Routine()
	{
		var arrowNotch = new WaitForSeconds(t.arrowNotchTime);
		//Shoot the bow forever
		var handle = c.SetHeldBow(t.bowItem);
		while (!handle.IsDone)
			yield return null;

		c.weaponGraphics.holdables.bow.sheathed = false;

		float arrowChargeTime = 1 / t.bowFireRate - t.arrowNotchTime;
		var bowAC = c.weaponGraphics.holdables.bow.gameObject.GetComponent<Animator>();

		var bow = c.weaponGraphics.holdables.bow.gameObject.GetComponent<Bow>();
		bow.InitBow(t.bowItem);

		c.animationController.weights.weight = 1;
		c.animationController.weights.headWeight = 1;
		c.animationController.weights.eyesWeight = 1;
		c.animationController.weights.bodyWeight = 1;
		c.animationController.weights.clampWeight = 0.5f; //180 degrees
		c.animationController.TriggerTransition(c.transitionSet.holdBow);

		while (true)
		{
			float t = 0;


			bow.NotchNextArrow(this.t.ammoItem);

			while (t < 1)
			{
				t += Time.deltaTime;
				bowAC.SetFloat("Charge", t);

				bow.transform.forward = c.transform.forward;
				c.animationController.lookAtPosition = bow.arrowSpawnPosition.position + c.transform.forward * this.t.arrowSpeed;

				yield return null;
			}

			bow.ReleaseArrow(c.transform.forward * this.t.arrowSpeed);

			yield return arrowNotch;
		}
	}
}