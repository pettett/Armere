using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PassiveInteractableWorldObjectAddon : WorldObjectComponent<PassiveInteractableComponentSettings>, IPassiveInteractable
{
    public bool canInteract { get; set; } = true;
    public void Interact(IInteractor interactor)
    {
        //Give items and destroy
        worldObject.AddContentsToInventory();
        WorldObjectSpawner.DestroyWorldObject(worldObject);
    }
}
