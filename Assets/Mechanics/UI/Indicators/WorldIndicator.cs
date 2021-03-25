using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldIndicator : IndicatorUI
{
	public UnityEvent<string> onIndicate;
	public UnityEvent onEndIndicate;
	public void StartIndication(Transform target, string title, Vector3 worldOffset = default)
	{
		Init(target, worldOffset);

		onIndicate?.Invoke(title);
		gameObject.SetActive(true);

		LeanTween.value(gameObject, -5, 5, 0.8f).setOnUpdate(x => localOffset.y = x).setLoopPingPong().setEaseInOutSine();
	}
	public void EndIndication()
	{
		if (this != null) //Test for destruction
		{
			target = null;
			onEndIndicate?.Invoke();
			gameObject?.SetActive(false);
		}
	}




}
