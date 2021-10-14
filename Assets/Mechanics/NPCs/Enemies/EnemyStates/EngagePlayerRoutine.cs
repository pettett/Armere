using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "Engage Player Routine", menuName = "Game/NPCs/Engage Player Routine", order = 0)]
public class EngagePlayerRoutine : AIContextStateTemplate<Character>
{
	public bool useSword;
	public bool useBow;
	public float meleeDistance = 1;
	public float bowDistance = 5f;

	public override AIState StartState(AIMachine c)
	{
		Assert.IsNotNull(context);
		var s = new EngagePlayer(c, this, context);
		context = null;
		return s;
	}

}

public class EngagePlayer : AIState<EngagePlayerRoutine>
{


	float sqrMeleeDistance => t.meleeDistance * t.meleeDistance;

	Coroutine r;
	Character target;
	public EngagePlayer(AIMachine c, EngagePlayerRoutine t, Character target) : base(c, t)
	{
		Assert.IsNotNull(target);

		this.target = target;
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}

	IEnumerator Routine()
	{
		//Once they player has attacked or been seen, do not stop engageing until circumstances change
		c.agent.isStopped = true;

		Vector3 directionToPlayer;
		Health targetHealth = target.GetComponent<Health>();
		bool movingToCatchPlayer = false;
		c.lookingAtTarget = target.transform;

		if (c.weaponGraphics.holdables.melee.sheathed)
			yield return c.DrawItem(ItemType.Melee);


		//Stop attacking the player after it has died
		while (!targetHealth.dead)
		{
			directionToPlayer = target.transform.position - c.transform.position;

			float targetDistSqr = sqrMeleeDistance;
			//Melee if sword is on or neither are on
			//Should be one or the other
			bool sword = (t.useSword && c.hasInventory && c.inventory.HasMeleeWeapon);
			bool bow = (t.useBow && c.hasInventory && c.inventory.HasBowWeapon);

			bool melee = sword || !bow; //Melee with sword if we can, else use bow, else use fists

			if (!melee) targetDistSqr = t.bowDistance * t.bowDistance;


			bool tooFarToAttack = directionToPlayer.sqrMagnitude > targetDistSqr;



			switch (melee, tooFarToAttack, movingToCatchPlayer)
			{

				case (_, true, false):
					movingToCatchPlayer = true;
					yield return new WaitForSeconds(0.1f);
					break;
				case (_, true, true): //Wants to melee, too far to melee and already moving
					c.agent.Move(directionToPlayer.normalized * Time.deltaTime * c.agent.speed);
					break;
				case (_, false, true):
					movingToCatchPlayer = false;
					//Small delay to adjust to stopped movement
					yield return new WaitForSeconds(0.1f);
					break;

				case (true, false, false):
					//Within melee range of player

					if (t.useSword)
						//Swing sword
						yield return c.SwingSword();
					else
					{
						//Melee attack
						Debug.Log("Doing melee attack");

					}
					break;
				case (false, false, false):
					//use bow, can only use bow
					Debug.Log("Doing bow attack");
					break;

			}

			directionToPlayer.y = 0;
			c.transform.forward = directionToPlayer;

			//TODO: Test to see if the player is still in view


			yield return null;
		}
		//Once the player has died, return to normal routine to stop end looking janky
		machine.ChangeToState(c.defaultState);
	}

}
