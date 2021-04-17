using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Yarn.Unity;



public class InteractableDialogue : MonoBehaviour, IInteractable, IDialogue
{
	string IDialogue.StartNode => startNode;


	public bool canInteract { get => enabled; set => enabled = value; }

	[Range(0, 360)]
	public float requiredLookAngle = 180;
	public float requiredLookDot => Mathf.Cos(requiredLookAngle);

	public string interactionDescription => "Read";

	public string interactionName => null;

	public Vector3 worldOffset => default;

	public AssetReferenceT<YarnProgram> Dialogue => dialogue;

	public AssetReferenceT<YarnProgram> dialogue;
	public string startNode = "Start";
	public void Interact(IInteractor interactor)
	{
	}


	public virtual void SetupCommands(DialogueRunner runner) { }

	public virtual void RemoveCommands(DialogueRunner runner) { }
}
