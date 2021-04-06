using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameUI : MonoBehaviour
{
	public OnMinigameEventChannel onMinigameStarted;

	private void Start()
	{
		onMinigameStarted.OnEventRaised += OnMinigameStarted;
	}
	public void OnMinigameStarted(Minigame minigame)
	{
		minigame.goal.CreateUI(this);
		for (int i = 0; i < minigame.constraints.Length; i++)
			minigame.constraints[i].CreateUI(this);

	}
}
