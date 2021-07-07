using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimalState : State<AnimalState, AnimalMachine, AnimalStateTemplate>
{
	public Animal c => machine.c;
	protected AnimalState(AnimalMachine machine) : base(machine)
	{
	}
}
public abstract class AnimalState<TemplateT> : AnimalState where TemplateT : AnimalStateTemplate
{
	public readonly TemplateT t;

	protected AnimalState(AnimalMachine machine, TemplateT t) : base(machine)
	{
		this.t = t;
	}
}