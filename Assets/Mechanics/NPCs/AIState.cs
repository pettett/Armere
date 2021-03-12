using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class AIState : State<AIHumanoid>
{
	public abstract bool alertOnAttack { get; }
	public abstract bool searchOnEvent { get; }
	public abstract bool investigateOnSight { get; }

	protected AIState(AIHumanoid c) : base(c)
	{
	}
}
public abstract class AIState<TemplateT> : AIState where TemplateT : AIStateTemplate
{
	public readonly TemplateT t;

	public override bool alertOnAttack => t.alertOnAttack;
	public override bool searchOnEvent => t.searchOnEvent;
	public override bool investigateOnSight => t.investigateOnSight;

	protected AIState(AIHumanoid c, TemplateT t) : base(c)
	{
		this.t = t;
	}
}
public abstract class AIStateTemplate : StateTemplate<AIStateTemplate, AIHumanoid, AIState>
{
	public bool alertOnAttack;
	public bool searchOnEvent;
	public bool investigateOnSight;

}