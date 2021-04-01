using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IInteractable
{
	void Interact(IInteractor interactor);
	bool canInteract { get; set; }
	GameObject gameObject { get; }
	float requiredLookDot { get; }
	string interactionDescription { get; }
	string interactionName { get; }
	Vector3 worldOffset { get; }

}

