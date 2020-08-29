using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;


public interface IDialogue
{
    YarnProgram Dialogue { get; }
    string StartNode { get; }
    Transform transform { get; }

    void SetupCommands(DialogueRunner runner);
    void RemoveCommands(DialogueRunner runner);
}

public class InteractableDialogue : MonoBehaviour, IInteractable, IDialogue
{
    YarnProgram IDialogue.Dialogue => dialogue;
    string IDialogue.StartNode => startNode;


    public bool canInteract { get => enabled; set => enabled = value; }

    [Range(0, 360)]
    public float requiredLookAngle = 180;
    public float requiredLookDot => Mathf.Cos(requiredLookAngle);

    public YarnProgram dialogue;
    public string startNode = "Start";
    public void Interact(IInteractor interactor)
    {
    }


    public void OnStartHighlight()
    {
        //Show arrow
    }

    public void OnEndHighlight()
    {
        //remove arrow
    }

    public virtual void SetupCommands(DialogueRunner runner) { }

    public virtual void RemoveCommands(DialogueRunner runner) { }
}
