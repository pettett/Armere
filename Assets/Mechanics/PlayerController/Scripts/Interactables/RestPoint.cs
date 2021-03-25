using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestPoint : MonoBehaviour, IInteractable
{
	public string restPromptText = "Sit";
	[TextArea]
	public string restTimeSelectText = "Rest by the fire until...";

	public YarnProgram dialogue;


	public void Interact(IInteractor interactor) { }
	public bool canInteract { get; set; } = true;
	public float requiredLookDot => 0;
	public string interactionDescription => restPromptText;

	public string interactionName => null;
}
