using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, WorldObjectComponent(typeof(PassiveInteractableWorldObjectAddon), "Passive Interactable")]
public class PassiveInteractableWorldObjectAddonSettings : WorldObjectDataComponentSettings
{

}

public class PassiveInteractableWorldObjectAddon : WorldObjectComponent<PassiveInteractableWorldObjectAddonSettings>, IPassiveInteractable
{
    public bool canInteract { get; set; } = true;
    public void Interact(IInteractor interactor)
    {
        //Give items and destroy
        worldObject.AddContentsToInventory();
        WorldObjectSpawner.DestroyWorldObject(worldObject);
    }
}
