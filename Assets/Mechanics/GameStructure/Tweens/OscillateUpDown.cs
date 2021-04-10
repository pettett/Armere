using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateUpDown : MonoBehaviour
{
	public float maxY = 20;
	public float time = 0.5f;
	[MyBox.SearchableEnum] public LeanTweenType type;
	// Start is called before the first frame update
	void Start()
	{
		LeanTween.moveLocalY(gameObject, maxY, time).setLoopPingPong().setEase(type);
	}

	// Update is called once per frame
}
