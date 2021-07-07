
using System;
using UnityEngine;
public abstract class StateTemplate<StateT, MachineT, TemplateT> : ScriptableObject
	where MachineT : StateMachine<StateT, MachineT, TemplateT>
	where StateT : State<StateT, MachineT, TemplateT>
	where TemplateT : StateTemplate<StateT, MachineT, TemplateT>
{
	public char stateSymbol;
	public Type StateType() => typeof(StateT);
	public abstract StateT StartState(MachineT machine);

	public TemplateT[] parallelStates;
}