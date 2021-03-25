using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPrompt : MonoBehaviour
{
	public TMPro.TextMeshProUGUI promptText;

	public TMPro.TextMeshProUGUI keybindPromptText1;

	//public Interacta



	public void ApplyPrompt(string prompt, float retension = 0)
	{
		promptText.enabled = true;

		promptText.text = prompt;
		if (retension != 0)
		{
			Invoke("ResetPrompt", retension);
		}
	}

	public void ApplyPrompt(string prompt, string keybind, float retension = 0)
	{
		promptText.enabled = true;
		keybindPromptText1.enabled = true;

		promptText.text = prompt;
		keybindPromptText1.text = keybind;


		if (retension != 0)
		{
			Invoke("ResetPrompt", retension);
		}
	}

	public void ResetPrompt()
	{
		promptText.enabled = false;
		keybindPromptText1.enabled = false;

	}
}
