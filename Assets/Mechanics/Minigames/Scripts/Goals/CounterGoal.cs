using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Minigames/Counter Goal")]
public class CounterGoal : MinigameConstraint
{
	public bool isConstraint;
	public uint targetCounters;
	[System.NonSerialized] uint count;
	public override object result => (float)count;
	public VoidEventChannelSO onCounterIncrement;
	public GameObject uiPrefab;
	[System.NonSerialized] GameObject uiInstance;
	[System.NonSerialized] TMPro.TextMeshProUGUI counter;
	public override void StartConstraint()
	{
		onCounterIncrement.OnEventRaised += Increment;
	}

	public void Increment()
	{
		count++;
		counter.SetText(count.ToString());
		if (endOnGoal && count >= targetCounters)
		{
			endMinigame();
		}
	}
	public override void CreateUI(MinigameUI ui)
	{
		uiInstance = Instantiate(uiPrefab, ui.transform);
		counter = uiInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
	}
	public override void ResetConstraint()
	{
		onCounterIncrement.OnEventRaised -= Increment;
		counter = null;
		Destroy(uiInstance);
	}


}
