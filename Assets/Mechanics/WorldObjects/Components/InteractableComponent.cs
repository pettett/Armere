using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableComponent : WorldObjectComponent<InteractableComponentSettings>, IInteractable
{

    public bool canInteract { get; set; } = true;

    public float requiredLookDot => 0;

    public string interactionDescription = "Pickup {0}";
    string IInteractable.interactionDescription => string.Format(interactionDescription, worldObject.instanceData.containsItem);

    public void Interact(IInteractor interactor)
    {
        //Open the chest or pickup the object
        InventoryController.AddItem(
            worldObject.instanceData.containsItem,
            worldObject.instanceData.containsItemCount);

        canInteract = false;

        WorldObjectSpawner.DestroyWorldObject(worldObject);
    }

    public void OnEndHighlight()
    {
        UIController.singleton.itemIndicator.EndIndication();
    }

    public void OnStartHighlight()
    {
        UIController.singleton.itemIndicator.StartIndication(
            transform, worldObject.instanceData.containsItem.ToString());
    }
}
