using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerController.Player_CharacterController c);
    bool enabled { get; }
    GameObject gameObject { get; }
}
