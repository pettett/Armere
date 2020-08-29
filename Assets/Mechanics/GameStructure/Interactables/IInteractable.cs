using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(IInteractor interactor);
    bool canInteract { get; set; }
    void OnStartHighlight();
    void OnEndHighlight();
    GameObject gameObject { get; }
    float requiredLookDot { get; }
}

