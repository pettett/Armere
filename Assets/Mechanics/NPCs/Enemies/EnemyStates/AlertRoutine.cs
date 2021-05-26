using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "Alert Routine Routine", menuName = "Game/NPCs/Alert Routine Routine", order = 0)]
public class AlertRoutine : AIFocusCharacterStateTemplate
{
	public AIFocusCharacterStateTemplate engagePlayer;
	public AIFocusCharacterStateTemplate findWeapons;

	public override AIState StartState(AIHumanoid c)
	{
		Assert.IsNotNull(engaging);
		var s = new Alert(c, this, engaging);
		engaging = null;
		return s;
	}

}
public class Alert : AIState<AlertRoutine>
{
	public override bool alertOnAttack => false;

	public override bool searchOnEvent => false;

	public override bool investigateOnSight => false;


	Coroutine r;
	readonly Character character;
	public Alert(AIHumanoid c, AlertRoutine t, Character character) : base(c, t)
	{
		this.character = character;
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}

	IEnumerator Routine()
	{

		c.lookingAtTarget = character.transform;

		c.OnPlayerDetected();


		c.animationController.TriggerTransition(c.transitionSet.surprised);

		yield return new WaitForSeconds(1);

		if (c.inventory.HasMeleeWeapon)
		{
			if (c.weaponGraphics.holdables.melee.sheathed)
				yield return c.DrawItem(ItemType.Melee);

			c.ChangeToState(t.engagePlayer.EngageWith(character));
		}
		else
		{
			//Attempt to get hold of a melee weapon
			c.ChangeToState(t.findWeapons.EngageWith(character));
		}
	}
}