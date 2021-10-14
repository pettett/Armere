using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Investigate Routine", menuName = "Game/NPCs/Investigate Routine", order = 0)]
public class InvestigateRoutine : AIContextStateTemplate<Vector3>
{

	public override AIState StartState(AIMachine c)
	{
		return new Investigate(c, this, context);
	}


}

public class Investigate : AIState<InvestigateRoutine>
{


	readonly Vector3 investigating;

	public Vision vision;

	public Investigate(AIMachine machine, InvestigateRoutine t, Vector3 investigating) : base(machine, t)
	{
		this.investigating = investigating;


		c.GoToPosition(investigating);
	}
	float investProgress = 0;


}