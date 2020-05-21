using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable
{
    ItemName type;
    public void Interact(PlayerController.Player_CharacterController c)
    {
        InventoryController.AddItem(type);
    }
}
