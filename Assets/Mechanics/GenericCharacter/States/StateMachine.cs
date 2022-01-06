using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class StateMachine<StateT, MachineT, TemplateT> : ArmereBehaviour
	where MachineT : StateMachine<StateT, MachineT, TemplateT>
	where StateT : State<StateT, MachineT, TemplateT>
	where TemplateT : StateTemplate<StateT, MachineT, TemplateT>
{
	public bool autoRunDefault = true;
	public TemplateT defaultState;
	public StateT mainState;
	public readonly List<StateT> currentStates = new List<StateT>();
	private void Start()
	{
		if (autoRunDefault && defaultState != null)
			ChangeToState(defaultState);
	}


	public void Init(TemplateT defaultState)
	{
		Assert.IsNotNull(defaultState);
		this.defaultState = defaultState;
		ChangeToState(defaultState);
	}

	public event System.Action onStateChanged;
	protected void StateChanged() => onStateChanged?.Invoke();

	void EndAllStatesExcept(TemplateT[] keep)
	{
		//go through and end all the parallel states not used by this state
		for (int i = 0; i < currentStates.Count; i++)
		{
			for (int j = 0; j < keep.Length; j++)
				if (currentStates[i].GetType() == keep[j].StateType())
					goto DoNotEndState;

			//Goto saves time here - ending state will only be skipped if it is required
			currentStates[i].End();
		DoNotEndState:
			continue;
		}
	}
	void EndAllStatesExcept(Type keep)
	{
		//go through and end all the parallel states not used by this state
		for (int i = 0; i < currentStates.Count; i++)
			if (currentStates[i].GetType() != keep)
				currentStates[i].End();
	}

	public virtual void ChangeToState(StateT state)
	{
		EndAllStatesExcept(state.GetType());
		currentStates.Clear();
		currentStates.Add(state);
		mainState = state;
	}
	public virtual StateT ChangeToState(TemplateT template)
	{
		TemplateT t = template ?? defaultState;

		mainState = t.StartState((MachineT)this);

		EndAllStatesExcept(t.parallelStates);

		StateT[] newStates = new StateT[t.parallelStates.Length + 1];

		for (int i = 0; i < t.parallelStates.Length; i++)
		{
			//test if the desired parallel state is currently active
			newStates[i] = GetState(t.parallelStates[i].StateType());
			if (newStates[i] == null)
			{
				newStates[i] = t.parallelStates[i].StartState((MachineT)this);
			}
		}
		newStates[t.parallelStates.Length] = mainState;

		currentStates.Clear();
		currentStates.AddRange(newStates);

		for (int i = 0; i < currentStates.Count; i++)
		{
			currentStates[i].Start();
		}
		StateChanged();
		return mainState;
	}


	protected virtual void Update()
	{
		for (int i = 0; i < currentStates.Count; i++)
			currentStates[i].Update();
	}
	protected virtual void FixedUpdate()
	{
		for (int i = 0; i < currentStates.Count; i++)
			currentStates[i].FixedUpdate();
	}




	public StateT GetState(Type t)
	{
		for (int i = 0; i < currentStates.Count; i++)
		{
			if (currentStates[i].GetType() == t)
			{
				return currentStates[i];
			}
		}
		return null;
	}
	public T GetState<T>() where T : StateT
	{
		return (T)GetState(typeof(T));
	}

	public bool TryGetState<T>(out T state) where T : StateT
	{
		for (int i = 0; i < currentStates.Count; i++)
		{
			if (currentStates[i].GetType() == typeof(T))
			{
				state = currentStates[i] as T;
				return true;
			}
		}
		state = default;
		return false;
	}


}