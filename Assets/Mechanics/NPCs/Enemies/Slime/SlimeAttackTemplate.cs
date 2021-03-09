using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAttackTemplate : AIStateTemplate
{
	public override AIState StartState(AIHumanoid c)
	{
		return new SlimeAttack(c, this);
	}
}
public class SlimeAttack : AIState<SlimeAttackTemplate>
{
	public SlimeAttack(AIHumanoid c, SlimeAttackTemplate t) : base(c, t)
	{
	}
}