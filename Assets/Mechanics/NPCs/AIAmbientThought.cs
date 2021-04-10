using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIAmbientThought : MonoBehaviour
{
	public Transform ambientThought;
	public TextMeshPro ambientThoughtText;
	public Transform thoughtBackground;
	public string startingText = "No State";

	public float oscillationIntensity = 0.05f;
	public float oscillationTime = 0.05f;
	[MyBox.SearchableEnum]
	public LeanTweenType type = LeanTweenType.easeInOutCirc;
	public void SetText(string text)
	{
		ambientThoughtText.SetText(text);
	}
	private void Start()
	{
		ambientThoughtText.gameObject.transform.localPosition = -Vector3.up * oscillationIntensity;
		LeanTween.moveLocalY(ambientThoughtText.gameObject, oscillationIntensity, oscillationTime).setLoopPingPong().setEase(type);
	}
	private void Update()
	{
		ambientThought.rotation = Camera.main.transform.rotation;
		var s = (ambientThoughtText.transform as RectTransform).sizeDelta;
		thoughtBackground.localScale = new Vector3(s.x + 0.1f, s.y, 1);
		thoughtBackground.localPosition = new Vector3(s.x * 0.5f, 0, 0.01f);
	}
}
