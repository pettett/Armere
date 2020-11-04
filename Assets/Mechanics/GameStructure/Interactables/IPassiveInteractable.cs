using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPassiveInteractable
{
    void Interact(IInteractor interactor);
    bool canInteract { get; set; }
}
