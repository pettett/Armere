using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIAmbientThought : MonoBehaviour
{
	public Transform ambientThought;
	public TextMeshPro ambientThoughtText;
	private void Update()
	{
		ambientThought.rotation = Camera.main.transform.rotation;
	}
}
