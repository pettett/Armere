using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public ItemName type;
    public event System.Action<InteractableItem> onItemDestroy;
    public void Interact(PlayerController.Player_CharacterController c)
    {
        InventoryController.AddItem(type);

        onItemDestroy?.Invoke(this);
    }
}
