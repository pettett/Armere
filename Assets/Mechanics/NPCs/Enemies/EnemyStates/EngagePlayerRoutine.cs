using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "Engage Player Routine", menuName = "Game/NPCs/Engage Player Routine", order = 0)]
public class EngagePlayerRoutine : AIFocusCharacterStateTemplate
{
	public override AIState StartState(AIHumanoid c)
	{
		Assert.IsNotNull(engaging);
		var s = new EngagePlayer(c, this, engaging);
		engaging = null;
		return s;
	}

}

public class EngagePlayer : AIState<EngagePlayerRoutine>
{
	public override bool alertOnAttack => false;

	public override bool searchOnEvent => false;

	public override bool investigateOnSight => false;
	public float approachDistance = 1;
	float sqrApproachDistance => approachDistance * approachDistance;
	public bool approachPlayer = true;
	Coroutine r;
	Character target;
	public EngagePlayer(AIHumanoid c, EngagePlayerRoutine t, Character target) : base(c, t)
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
		Health playerHealth = target.GetComponent<Health>();
		bool movingToCatchPlayer = false;
		c.lookingAtTarget = target.transform;

		if (c.weaponGraphics.holdables.melee.sheathed)
			yield return c.DrawItem(ItemType.Melee);


		//Stop attacking the player after it has died
		while (!playerHealth.dead)
		{
			directionToPlayer = target.transform.position - c.transform.position;
			if (approachPlayer && directionToPlayer.sqrMagnitude > sqrApproachDistance)
			{
				if (!movingToCatchPlayer)
				{
					movingToCatchPlayer = true;
					yield return new WaitForSeconds(0.1f);
				}

				c.agent.Move(directionToPlayer.normalized * Time.deltaTime * c.agent.speed);
			}
			else if (movingToCatchPlayer)
			{
				movingToCatchPlayer = false;
				//Small delay to adjust to stopped movement
				yield return new WaitForSeconds(0.1f);
			}
			else
			{
				//Within sword range of player
				//Swing sword
				yield return c.SwingSword();
			}

			directionToPlayer.y = 0;
			c.transform.forward = directionToPlayer;

			//TODO: Test to see if the player is still in view


			yield return null;
		}
		//Once the player has died, return to normal routine to stop end looking janky
		c.ChangeToState(c.defaultState);
	}

}
