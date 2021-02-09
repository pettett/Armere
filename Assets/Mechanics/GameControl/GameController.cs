using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : SceneSaveData
{
	public float timeBeforeDeathScreen = 4;
	public float deathScreenTime = 5;

	public override string SaveTooltip => LevelInfo.currentLevelInfo.SaveTooltip;

	public VoidEventChannelSO onPlayerDeathChannel;

	//Track when the player dies. When they do, show the UI and load the last save
	private void Start()
	{
		SetOnDeathEvent();
		LevelController.onLevelLoaded += UpdatePlayerHealth;
	}
	public void UpdatePlayerHealth(Scene s, LoadSceneMode l)
	{
		if (l == LoadSceneMode.Single)
		{
			//New player transform. Only called after first death as first load is not a save load (yet)
			//TODO: This will break eventually when loading is more thorough
			SetOnDeathEvent();

		}
	}

	public void SetOnDeathEvent()
	{
		onPlayerDeathChannel.OnEventRaised += OnPlayerDeath;
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
	}
}
