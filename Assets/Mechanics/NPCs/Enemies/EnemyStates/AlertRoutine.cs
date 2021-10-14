using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "Alert Routine Routine", menuName = "Game/NPCs/Alert Routine Routine", order = 0)]
public class AlertRoutine : AIContextStateTemplate<Character>
{
	public AIContextStateTemplate<Character> engagePlayer;
	public AIContextStateTemplate<Character> findWeapons;
	public bool useWeapons = true;
	public override AIState StartState(AIMachine c)
	{
		Assert.IsNotNull(context);
		var s = new Alert(c, this, context);
		context = null;
		return s;
	}

}
public class Alert : AIState<AlertRoutine>
{



	Coroutine r;
	readonly Character character;
	public Alert(AIMachine c, AlertRoutine t, Character character) : base(c, t)
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
		if (t.useWeapons)
		{
			if (c.inventory.HasMeleeWeapon)
			{
				if (c.weaponGraphics.holdables.melee.sheathed)
					yield return c.DrawItem(ItemType.Melee);

				machine.ChangeToState(t.engagePlayer.Target(character));
			}
			else
			{
				//Attempt to get hold of a melee weapon
				machine.ChangeToState(t.findWeapons.Target(character));
			}
		}
		else
		{

			machine.ChangeToState(t.engagePlayer.Target(character));
		}
	}
}