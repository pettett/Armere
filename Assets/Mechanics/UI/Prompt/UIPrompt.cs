using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPrompt : MonoBehaviour
{
    public static UIPrompt singleton;

    public TMPro.TextMeshProUGUI promptText;

    public TMPro.TextMeshProUGUI keybindPromptText1;

    public TMPro.TextMeshProUGUI keybindPromptText2;

    public static void ApplyPrompt(string prompt, float retension = 0)
    {
        singleton.promptText.enabled = true;

        singleton.promptText.text = prompt;
        if (retension != 0)
        {
            singleton.Invoke("ResetPrompt", retension);
        }
    }

    public static void ApplyPrompt(string prompt, string keybind, float retension = 0)
    {
        singleton.promptText.enabled = true;
        singleton.keybindPromptText1.enabled = true;
        singleton.keybindPromptText2.enabled = true;
        singleton.promptText.text = prompt;
        singleton.keybindPromptText1.text = keybind;
        singleton.keybindPromptText2.text = keybind;

        if (retension != 0)
        {
            singleton.Invoke("ResetPrompt", retension);
        }
    }

    public static void ResetPrompt()
    {
        singleton.promptText.enabled = false;
        singleton.keybindPromptText1.enabled = false;
        singleton.keybindPromptText2.enabled = false;
    }
    private void Start()
    {
        singleton = this;
    }
}
