using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Armere.UI;
public class GameController : SceneSaveData
{
	public float timeBeforeDeathScreen = 4;
	public float deathScreenTime = 5;

	public override string SaveTooltip => LevelInfo.currentLevelInfo.currentRegionName;

	public VoidEventChannelSO onPlayerDeathChannel;

	//Track when the player dies. When they do, show the UI and load the last save
	private void Start()
	{
		onPlayerDeathChannel.OnEventRaised += OnPlayerDeath;
	}
	private void OnDestroy()
	{
		onPlayerDeathChannel.OnEventRaised -= OnPlayerDeath;
	}



	public void OnPlayerDeath()
	{
		StartCoroutine(DeathRoutine());
	}
	public IEnumerator DeathRoutine()
	{
		yield return new WaitForSecondsRealtime(timeBeforeDeathScreen);
		//Tween in the death message
		UIController.singleton.deathScreen.SetActive(true);
		foreach (var g in UIController.singleton.deathScreen.GetComponentsInChildren<UnityEngine.UI.Graphic>())
		{
			//Set the alpha to 0
			g.CrossFadeAlpha(0, 0, true);
			//then twerp to 1
			g.CrossFadeAlpha(1, 1, true);
		}

		yield return new WaitForSecondsRealtime(deathScreenTime);

		SaveManager.singleton.LoadMostRecentSave(true);

		UIController.singleton.deathScreen.SetActive(false);
	}
}
