
using System;
using UnityEngine;
public abstract class StateTemplate<TemplateT, CharacterT, StateT> : ScriptableObject
	where StateT : State<CharacterT>
	where CharacterT : Character
	where TemplateT : StateTemplate<TemplateT, CharacterT, StateT>
{
	public char stateSymbol;
	public Type StateType() => typeof(StateT);
	public abstract StateT StartState(CharacterT c);

	public TemplateT[] parallelStates;
}