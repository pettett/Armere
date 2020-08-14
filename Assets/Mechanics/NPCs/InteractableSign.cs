﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
public class InteractableSign : MonoBehaviour, IInteractable
{
    public bool canInteract { get => enabled; set => enabled = value; }

    [Range(0, 360)]
    public float requiredLookAngle = 180;
    public float requiredLookDot => Mathf.Cos(requiredLookAngle);
    DialogueRunner runner;
    private void Start()
    {
        runner = GetComponent<DialogueRunner>();
        runner.AddCommandHandler("test", Test);
    }
    public void Interact(IInteractor interactor)
    {
        //c.ChangeToState<Conversation>();
        runner.StartDialogue("Start");
        DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
    }

    void OnDialogueComplete()
    {
        //c.ChangeToState<Walking>();
        DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
    }


    void Test(string[] arg)
    {
        print(arg[0]);
    }

    public void OnStartHighlight()
    {
        //Show arrow
    }

    public void OnEndHighlight()
    {
        //remove arrow
    }
}
