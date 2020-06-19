using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
public class Console : MonoBehaviour
{
    public static Console singleton;
    private void Awake()
    {
        singleton = this;
    }
    public struct Command
    {
        public string func;
        public string[] values;

        public Command(string text)
        {
            string[] segs = text.Split(' ');
            this.func = segs[0];
            this.values = segs.Skip(1).ToArray();
        }
    }
    System.Action<Command> onCommand;

    public InputField input;

    public static void Enable(System.Action<Command> onCommand)
    {
        singleton.onCommand = onCommand;
        singleton.input.gameObject.SetActive(true);
        singleton.input.onEndEdit.AddListener(singleton.OnSubmitCommand);
        singleton.input.Select();
    }
    public static void Disable()
    {
        singleton.input.gameObject.SetActive(false);
        singleton.input.onEndEdit.RemoveListener(singleton.OnSubmitCommand);
    }
    void OnSubmitCommand(string text)
    {
        onCommand?.Invoke(new Command(text));

    }
}
