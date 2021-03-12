using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn;

using TMPro;
using System.Linq;
using Armere.Inventory;
[RequireComponent(typeof(AIHumanoid))]
public class AIDialogue : MonoBehaviour, IDialogue, IInteractable
{

	[Range(0, 360)]
	public float requiredLookAngle = 180;
	public float requiredLookDot => Mathf.Cos(requiredLookAngle);
	bool IInteractable.canInteract { get => enabled; set => enabled = value; }

	public string interactionDescription => "Talk";
	public Transform headPosition;
	public NPCName npcName;


	public IDialogue target;
	AIHumanoid ai;
	[System.NonSerialized] public Transform[] conversationGroupOverride, walkingPoints, focusPoints;
	public System.Action<IInteractor> onInteract;

	public BuyMenuItem[] buyInventory;
	private void Start()
	{
		ai = GetComponent<AIHumanoid>();
	}
	public void GoToWalkingPoint(string name, System.Action onComplete = null)
	{
		ai.GoToPosition(GetTransform(walkingPoints, name).position, onComplete);
	}
	public void SetupCommands(DialogueRunner runner)
	{
		target.SetupCommands(runner);
	}
	public Transform GetTransform(Transform[] transforms, string name) => transforms.First(t => t.name == name);
	public Transform GetFocusPoint(string name) => GetTransform(focusPoints, name);
	public void RemoveCommands(DialogueRunner runner)
	{
		target.RemoveCommands(runner);
	}

	public void Interact(IInteractor interactor)
	{
		onInteract?.Invoke(interactor);
	}


	public void OnStartHighlight()
	{
		//show arrow
		UIController.singleton.npcIndicator.StartIndication(
			transform,
			NPCManager.singleton.data[npcName].spokenTo ? npcName.ToString() : "", //Do not show the npc name if the player has never spoken to them
			Vector3.up * 2);
	}

	public void OnEndHighlight()
	{
		//remove arrow
		UIController.singleton.npcIndicator.EndIndication();
	}

	public YarnProgram Dialogue => target.Dialogue;
	public string StartNode => target.StartNode;

	[System.NonSerialized] public IVariableAddon dialogueAddon;

}
