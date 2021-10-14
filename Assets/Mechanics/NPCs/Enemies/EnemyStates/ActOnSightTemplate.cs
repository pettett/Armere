using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/NPCs/Enemies/Act on Sight")]
public class ActOnSightTemplate : AIStateTemplate
{
	public AIContextStateTemplate<Character> onSighted;

	public override AIState StartState(AIMachine machine)
	{
		return new ActOnSight(machine, this);
	}
}
public class ActOnSight : AIState<ActOnSightTemplate>
{
	public VirtualVision vision;
	public ActOnSight(AIMachine machine, ActOnSightTemplate t) : base(machine, t)
	{
		c.AssertComponent(out vision);
	}
	public override void Update()
	{
		if (vision.TryFindTarget(out var target))
		{
			machine.ChangeToState(t.onSighted.Target(target));
		}
	}
}