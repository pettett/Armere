using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerController.Player_CharacterController c);
    bool canInteract { get; set; }
    void OnStartHighlight();
    void OnEndHighlight();
    GameObject gameObject { get; }
}
