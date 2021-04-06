using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Minigames/Timer Constraint")]
public class TimerConstraint : MinigameConstraint
{
	public GameObject uiPrefab;
	GameObject uiInstance;
	TMPro.TextMeshProUGUI text;
	public float avaliableSeconds;

	[System.NonSerialized] public float startTime, endTime;

	public float remaining
	{
		get
		{
			if (endOnGoal)
				return endTime - Time.time;
			else
				return Time.time - startTime;
		}
	}
	public override object result => remaining;

	public override void StartConstraint()
	{
		startTime = Time.time;
		endTime = startTime + avaliableSeconds;
	}
	public override void UpdateConstraint()
	{
		float remaining = this.remaining;


		int minutes = Mathf.FloorToInt(remaining / 60);
		int seconds = Mathf.FloorToInt(remaining % 60);

		int centSeconds = Mathf.FloorToInt((remaining % 1) * 100);

		text.SetText($"{minutes:00}:{seconds:00}.{centSeconds:00}");
		if (Time.time > endTime && endOnGoal)
		{
			endMinigame();
		}
	}
	public override void ResetConstraint()
	{
		Destroy(uiInstance);
	}
	public override void CreateUI(MinigameUI ui)
	{
		uiInstance = Instantiate(uiPrefab, ui.transform);
		text = uiInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
	}
}
