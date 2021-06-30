using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Armere.UI;
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


	public static void LoadLevel(string levelName, System.Action afterSceneLoad = null)
	{
		singleton.StartCoroutine(singleton.Load(levelName, afterSceneLoad));
	}

	IEnumerator Load(string levelName, System.Action afterSceneLoad)
	{
		loadingDisplayRoot.SetActive(true);
		UIController.singleton.FadeOut(0, levelName);
		UIController.singleton.onSceneChangeBegin.Invoke();

		Time.timeScale = 0;

		Scene currentScene = SceneManager.GetActiveScene();



		yield return SceneManager.UnloadSceneAsync(currentScene);

		yield return new WaitForSecondsRealtime(1f);

		yield return SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);

		var loadedScene = SceneManager.GetSceneByName(levelName);

		if (!SceneManager.SetActiveScene(loadedScene))
		{
			Debug.LogWarning("Unable to set active scene");
		}


		currentLevel = levelName;

		System.GC.Collect();
		Time.timeScale = 1;



		yield return null;
		afterSceneLoad?.Invoke();


		loadingDisplayRoot.SetActive(false);


		yield return new WaitForSeconds(0.1f);
		UIController.singleton.FadeIn(1, true);
	}


}
