using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/NPCs/Flight Flee")]
public class FlightFleeTemplate : AnimalStateContextTemplate<Vector3>
{
	public AnimatorVariable isFlying;
	public float reactionTime = 0.2f;
	public float fleeTime = 1f;
	public float fleeSpeed = 1f;
	public bool dissapearAfterFlee = true;
	public float upwardsBias = 1f;
	public override AnimalState StartContext(AnimalMachine machine, Vector3 from) => new FlightFlee(machine, this, from);
	public override AnimalState StartState(AnimalMachine machine) => null;
}

public class FlightFlee : AnimalState<FlightFleeTemplate>
{
	readonly float startTime;
	readonly Vector3 fleeFrom;

	public FlightFlee(AnimalMachine machine, FlightFleeTemplate t, Vector3 fleePos) : base(machine, t)
	{
		startTime = Time.time;
		fleeFrom = fleePos;
		c.animator.SetBool(t.isFlying, true);
	}

	public override void Update()
	{
		float time = Time.time - startTime;
		if (time > t.reactionTime)
		{

			Vector3 dir = (Vector3.Scale(c.transform.position - fleeFrom, new Vector3(1, 0, 1))).normalized;
			dir.y = t.upwardsBias;
			dir.Normalize();


			Vector3 vel = dir * t.fleeSpeed;

			c.transform.forward = dir;
			c.transform.position += vel * Time.deltaTime;

		}
		if (time > t.fleeTime)
		{
			if (t.dissapearAfterFlee)
			{
				c.Disappear();
			}

		}


	}

}
