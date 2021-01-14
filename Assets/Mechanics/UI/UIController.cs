using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIController : MonoBehaviour
{

	public UIMenu tabMenu;
	public GameObject buyMenu;
	public WorldIndicator itemIndicator;
	public WorldIndicator npcIndicator;
	public static UIController singleton;

	public Image fadeoutImage;
	public TextMeshProUGUI fadeoutText;
	public GameObject scrollingSelector;

	public GameObject deathScreen;

	public UIBossBar bossBar;


	private void Awake()
	{
		singleton = this;
	}
	private void Start()
	{
		FadeIn(0.1f, false);
	}

	public IEnumerator FullFade(float fadeTime, float time, string text = null)
	{
		fadeTime = Mathf.Clamp(fadeTime, 0, time / 2);
		bool useText = text != null;

		// fadeoutImage.color = Color.clear; //Black with full transparency
		fadeoutImage.gameObject.SetActive(true);

		if (useText)
		{
			fadeoutText.text = text;
			fadeoutText.gameObject.SetActive(true);
		}

		yield return Fade(0, 1, fadeTime, useText);


		float fullyBlackTime = time - fadeTime * 2;

		yield return new WaitForSeconds(fullyBlackTime);

		yield return Fade(1, 0, fadeTime, useText);

		DisableFadeout();
	}

	public void FadeOut(float fadeTime, string text = null)
	{
		bool useText = text != null;

		fadeoutImage.gameObject.SetActive(true);

		if (useText)
		{
			fadeoutText.text = text;
			fadeoutText.gameObject.SetActive(true);
		}

		StartFade(0, 1, fadeTime, useText);
	}
	public void FadeIn(float fadeTime, bool useText)
	{
		fadeoutImage.gameObject.SetActive(true);
		if (useText)
		{
			fadeoutText.gameObject.SetActive(true);
		}
		StartCoroutine(Fade(1, 0, fadeTime, useText, DisableFadeout));
	}

	//Change alpha
	public IEnumerator Fade(float startAlpha, float endAlpha, float fadeTime, bool useText, System.Action onComplete = null)
	{
		StartFade(startAlpha, endAlpha, fadeTime, useText);
		yield return new WaitForSeconds(fadeTime);
		onComplete?.Invoke();
	}
	public void StartFade(float startAlpha, float endAlpha, float fadeTime, bool useText)
	{
		fadeoutImage.CrossFadeAlpha(startAlpha, 0, true);
		fadeoutImage.CrossFadeAlpha(endAlpha, fadeTime, true);
		//do the same for text
		if (useText)
		{
			fadeoutText.CrossFadeAlpha(startAlpha, 0, true);
			fadeoutText.CrossFadeAlpha(endAlpha, fadeTime, true);
		}
	}


	void DisableFadeout()
	{
		fadeoutImage.gameObject.SetActive(false);
		fadeoutText.gameObject.SetActive(false);
	}



	public static void SetTabMenu(bool active)
	{
		if (active)
			singleton.tabMenu.OpenMenu();
		else
			singleton.tabMenu.CloseMenu();
	}




}
