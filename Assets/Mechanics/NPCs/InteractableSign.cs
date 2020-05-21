using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerController;
using Yarn.Unity;
public class InteractableSign : MonoBehaviour, IInteractable
{
    DialogueRunner runner;
    private void Start()
    {
        runner = GetComponent<DialogueRunner>();
        runner.AddCommandHandler("test", Test);
    }
    Player_CharacterController c;
    public void Interact(Player_CharacterController c)
    {
        this.c = c;
        c.ChangeToState<Conversation>();
        runner.StartDialogue("Start");
        DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
    }

    void OnDialogueComplete()
    {
        c.ChangeToState<PlayerController.Player_CharacterController.Walking>();
        DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
    }


    void Test(string[] arg)
    {
        print(arg[0]);
    }
}
