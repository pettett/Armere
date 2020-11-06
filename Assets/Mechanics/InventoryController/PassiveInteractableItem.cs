using UnityEngine;

public class PassiveInteractableItem : ItemSpawnable, IPassiveInteractable
{
    public bool canInteract { get; set; } = true;

    public void Interact(IInteractor interactor)
    {
        AddItemsToInventory();
        Destroy();
    }
}