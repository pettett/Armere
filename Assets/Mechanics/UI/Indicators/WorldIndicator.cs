using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldIndicator : IndicatorUI
{

	public TMPro.TextMeshProUGUI titleText;
	public Vector2 titleTextHeightRange = new Vector2(50, 70);
	public void StartIndication(Transform target, string title, Vector3 worldOffset = default)
	{
		Init(target, worldOffset);

		titleText.SetText(title);
		gameObject.SetActive(true);
		titleText.transform.localPosition = Vector3.up * titleTextHeightRange.x;

		LeanTween.moveLocalY(
			titleText.gameObject,
			titleTextHeightRange.y,
			 0.8f).setLoopPingPong().setEaseInOutSine();
	}
	public void EndIndication()
	{
		if (this != null) //Test for destruction
		{
			target = null;
			gameObject?.SetActive(false);
		}
	}




}
