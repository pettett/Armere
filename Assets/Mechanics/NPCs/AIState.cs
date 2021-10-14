using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class AIState : State<AIState, AIMachine, AIStateTemplate>
{
	public AIHumanoid c => machine.c;

	protected AIState(AIMachine c) : base(c)
	{
	}
}
public abstract class AIState<TemplateT> : AIState where TemplateT : AIStateTemplate
{
	public readonly TemplateT t;

	public override string StateName => t.name;


	protected AIState(AIMachine machine, TemplateT t) : base(machine)
	{
		this.t = t;
	}
}
public abstract class AIStateTemplate : StateTemplate<AIState, AIMachine, AIStateTemplate>
{

}