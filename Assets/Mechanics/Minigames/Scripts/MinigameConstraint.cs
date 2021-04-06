using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinigameConstraint : ScriptableObject
{
	protected bool endOnGoal;
	protected Minigame minigame;
	protected System.Action endMinigame;

	public abstract object result { get; }
	public void InitConstraint(bool endOnGoal, Minigame minigame, System.Action endMinigame)
	{
		this.minigame = minigame;
		this.endMinigame = endMinigame;
		this.endOnGoal = endOnGoal;
	}
	public virtual void StartConstraint()
	{

	}
	public virtual void UpdateConstraint()
	{

	}
	public virtual void ResetConstraint()
	{

	}
	public virtual void CreateUI(MinigameUI ui)
	{

	}
}
