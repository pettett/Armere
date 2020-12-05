using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
public abstract class ConsoleReceiver : MonoBehaviour
{
    public abstract void OnCommand(Command command);

    public abstract List<string> GetSuggestionsForSlice(int slice, string[] segments);

}


public class Console : MonoBehaviour
{
    public InputAction suggestionUp = new InputAction("suggestionUp", InputActionType.Button, "<Keyboard>/upArrow");
    public InputAction suggestionDown = new InputAction("suggestionDown", InputActionType.Button, "<Keyboard>/downArrow");
    public InputAction suggestionSelect = new InputAction("suggestionSelect", InputActionType.Button, "<Keyboard>/tab");

    public static Console singleton;
    public ConsoleReceiver receiver;
    public RectTransform suggestions;
    public GameObject suggestionOptionPrefab;
    public TMPro.TMP_InputField input;
    System.Action onComplete;

    int selectedSuggestion = 0;
    int editingSlice = 0;
    List<string> viableCommands = new List<string>();

    private void Awake()
    {
        singleton = this;
    }

    private void Start()
    {
        suggestionUp.performed += SuggestionUp;
        suggestionDown.performed += SuggestionDown;
        suggestionSelect.performed += SuggestionSelect;
    }
    public void SuggestionUp(InputAction.CallbackContext c)
    {
        SetSelectedSuggestion(selectedSuggestion - 1);
    }
    public void SuggestionDown(InputAction.CallbackContext c)
    {
        SetSelectedSuggestion(selectedSuggestion + 1);
    }
    public void SuggestionSelect(InputAction.CallbackContext c)
    {
        //Apply the suggestion to the current block of text being operated on

        //assume first slice for now
        string[] segments = input.text.Split(' ');
        segments[editingSlice] = viableCommands[selectedSuggestion];
        //Update suggestions when typed
        string newText = string.Join(" ", segments) + " ";
        //set text without notify to give oppertunity to place caret
        input.SetTextWithoutNotify(newText);

        input.caretPosition = newText.Length;

        //Caret needs to be in the correct place before change to autofill correctly
        OnValueChanged(newText);
    }

    void SetSelectedSuggestion(int newSelect)
    {
        // in range, do not set
        if (newSelect >= 0 && newSelect < viableCommands.Count)
        {
            suggestions.GetChild(selectedSuggestion).GetComponent<Image>().color = Color.white;
            suggestions.GetChild(selectedSuggestion).GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            selectedSuggestion = newSelect;
            suggestions.GetChild(selectedSuggestion).GetComponent<Image>().color = Color.black;
            suggestions.GetChild(selectedSuggestion).GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }
    }

    public static void Enable(System.Action onComplete)
    {
        singleton.onComplete = onComplete;
        singleton.suggestions.gameObject.SetActive(true);
        singleton.input.gameObject.SetActive(true);
        singleton.input.onEndEdit.AddListener(singleton.OnSubmitCommand);
        singleton.input.text = "";
        singleton.input.onValueChanged.AddListener(singleton.OnValueChanged);
        singleton.input.Select();
        singleton.OnValueChanged("");

        singleton.suggestionUp.Enable();
        singleton.suggestionDown.Enable();
        singleton.suggestionSelect.Enable();
    }

    public static void Disable()
    {
        singleton.suggestions.gameObject.SetActive(false);
        singleton.input.gameObject.SetActive(false);
        singleton.input.onEndEdit.RemoveListener(singleton.OnSubmitCommand);
        singleton.input.onValueChanged.RemoveListener(singleton.OnValueChanged);

        singleton.suggestionUp.Disable();
        singleton.suggestionDown.Disable();
        singleton.suggestionSelect.Disable();
    }



    public static int GetCaretSlicePosition(int caret, string[] slices)
    {
        int previousLength = 0;
        int editingSlice = 0;
        for (int i = 0; i < slices.Length; i++)
        {
            if (previousLength <= caret)
            {
                //more length required
                previousLength += slices[i].Length + 1;
                editingSlice++;
            }
        }
        editingSlice--;
        return editingSlice;
    }

    public void OnValueChanged(string input)
    {
        //test if the user is still typing the name of the command
        string[] segments = input.Split(' ');

        editingSlice = GetCaretSlicePosition(this.input.caretPosition, segments);

        viableCommands = receiver.GetSuggestionsForSlice(editingSlice, segments);


        //Debug.LogFormat("Editing slice {0}", editingSlice);

        //update the highlight list
        //   ┌───────────┐
        //   │suggestions│
        //   └───────────┘
        // ti

        int dif = viableCommands.Count - suggestions.childCount;
        if (dif > 0) // need more text
            for (int i = 0; i < dif; i++)
            {
                Instantiate(suggestionOptionPrefab, suggestions);
            }
        else if (dif < 0) // need less text
            for (int i = 0; i < -dif; i++)
            {
                //remove from top down
                Destroy(suggestions.GetChild(suggestions.childCount - i - 1).gameObject);
            }

        // Debug.AssertFormat(viableCommands.Count == suggestions.childCount + dif, "Commands not equal to suggestions {0}", dif);

        for (int i = 0; i < viableCommands.Count; i++)
        {
            suggestions.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = viableCommands[i];
        }

        if (selectedSuggestion > viableCommands.Count - 1)
            SetSelectedSuggestion(viableCommands.Count - 1);
        else
            SetSelectedSuggestion(selectedSuggestion);


    }




    void OnSubmitCommand(string text)
    {
        receiver.OnCommand(new Command(text));
        onComplete?.Invoke();
    }
}
