using System.Collections;
using System.Collections.Generic;
using Armere.Inventory;
using UnityEngine;
[CreateAssetMenu(fileName = "Patrol Routine", menuName = "Game/NPCs/Patrol Routine", order = 0)]
public class PatrolRoutine : AIStateTemplate
{
	public float patrolSpeed = 3.5f;
	public float waitTime = 2;

	public bool holdWeaponDrawn = false;
	public bool overrideHeldWeapon = false;
	public HoldableItemData holdingWeapon;

	//Used to light torches
	public bool lightFlammableBody = true;

	public override AIState StartState(AIMachine c)
	{
		return new Patrol(c, this);
	}
}
public class Patrol : AIState<PatrolRoutine>
{
	readonly Coroutine r;
	public Patrol(AIMachine c, PatrolRoutine t) : base(c, t)
	{
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}
	public IEnumerator SetupWeapon()
	{
		if (t.overrideHeldWeapon)
		{
			var handle = c.SetHeldMelee(t.holdingWeapon);
			while (!handle.IsDone) // May be done immediately
				yield return null;
		}
		if (t.lightFlammableBody && c.weaponGraphics.holdables.melee.gameObject.TryGetComponent<FlammableBody>(out var f))
		{
			f.Light();
		}
	}
	public IEnumerator Routine()
	{
		yield return SetupWeapon();

		c.animationController.TriggerTransition(c.transitionSet.swordWalking);
		c.weaponGraphics.holdables.melee.sheathed = !t.holdWeaponDrawn;

		//Pick the closest waypoint by non -pathed distance
		int waypoint = c.GetClosestWaypoint();

		//If the player is seen, switch out of this routine
		c.agent.speed = t.patrolSpeed;
		while (true)
		{
			c.debugText.SetText($"Patrolling to {waypoint}");
			yield return c.GoToWaypoint(waypoint);

			yield return new WaitForSeconds(t.waitTime);
			waypoint++;
			if (waypoint == c.waypointGroup.Length) waypoint = 0;
		}
	}
}