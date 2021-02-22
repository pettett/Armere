using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum LevelName
{
	test = 0,
	stealthTest = 1,
	Headlands = 2
}

public class LevelManager : MonoBehaviour
{
	public static LevelManager singleton;
	public Scene mainScene;
	public string currentLevel;

	public GameObject loadingDisplayRoot;


	private void Awake()
	{
		singleton = this;
		mainScene = SceneManager.GetSceneByName("MainScene");
	}


	public static void LoadLevel(string levelName, bool keepPlayerLoaded, System.Action afterSceneLoad = null)
	{
		singleton.StartCoroutine(singleton.Load(levelName, keepPlayerLoaded, afterSceneLoad));
	}

	IEnumerator Load(string levelName, bool keepPlayerLoaded, System.Action afterSceneLoad)
	{
		loadingDisplayRoot.SetActive(true);
		UIController.singleton.FadeOut(0, levelName);
		UIController.singleton.onSceneChangeBegin.Invoke();

		Time.timeScale = 0;

		int sceneToLoadID = SceneManager.GetSceneByName(levelName).buildIndex;
		Scene currentScene = SceneManager.GetActiveScene();

		GameObject player = LevelInfo.currentLevelInfo.player;

		if (keepPlayerLoaded)
		{

			SceneManager.MoveGameObjectToScene(player, mainScene);
		}

		yield return SceneManager.UnloadSceneAsync(currentScene);

		yield return SceneManager.LoadSceneAsync(sceneToLoadID, LoadSceneMode.Additive);

		var sceneToLoad = SceneManager.GetSceneByBuildIndex(sceneToLoadID);
		SceneManager.SetActiveScene(sceneToLoad);

		if (keepPlayerLoaded)
		{
			SceneManager.MoveGameObjectToScene(player, sceneToLoad);
		}

		currentLevel = levelName;

		if (keepPlayerLoaded)
		{
			//Unload the player that came with the scene and set the persisten one
			//TODO: Make player created as prefab only when not persistent
			Destroy(LevelInfo.currentLevelInfo.player);
			LevelInfo.currentLevelInfo.player = player;
		}

		System.GC.Collect();
		Time.timeScale = 1;



		yield return null;
		afterSceneLoad?.Invoke();


		loadingDisplayRoot.SetActive(false);


		yield return new WaitForSeconds(0.1f);
		UIController.singleton.FadeIn(1, true);
	}


}
