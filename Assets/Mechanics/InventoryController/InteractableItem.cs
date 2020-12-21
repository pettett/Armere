using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
public class InteractableItem : ItemSpawnable, IInteractable
{

    public bool canInteract { get; set; } = true;

    public float requiredLookDot => -1;

    public string interactionDescription => $"Pickup {InventoryController.singleton.db[item].displayName}";



    public void Interact(IInteractor c)
    {

        Destroy();

        AddItemsToInventory();

    }

    public void OnStartHighlight()
    {
        //Show an arrow for this item with name on ui
        UIController.singleton.itemIndicator.StartIndication(transform, item.ToString());
    }

    public void OnEndHighlight()
    {
        //remove arrow
        UIController.singleton.itemIndicator.EndIndication();
    }



}
