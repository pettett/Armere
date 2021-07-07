using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class AIState : State<AIState, AIMachine, AIStateTemplate>
{
	public AIHumanoid c => machine.c;
	public abstract bool alertOnAttack { get; }
	public abstract bool searchOnEvent { get; }
	public abstract bool investigateOnSight { get; }

	protected AIState(AIMachine c) : base(c)
	{
	}
}
public abstract class AIState<TemplateT> : AIState where TemplateT : AIStateTemplate
{
	public readonly TemplateT t;

	public override bool alertOnAttack => t.alertOnAttack;
	public override bool searchOnEvent => t.searchOnEvent;
	public override bool investigateOnSight => t.investigateOnSight;

	protected AIState(AIMachine c, TemplateT t) : base(c)
	{
		this.t = t;
	}
}
public abstract class AIStateTemplate : StateTemplate<AIState, AIMachine, AIStateTemplate>
{
	public bool alertOnAttack;
	public bool searchOnEvent;
	public bool investigateOnSight;

}