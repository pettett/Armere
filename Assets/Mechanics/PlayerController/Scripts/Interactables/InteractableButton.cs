using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Collider))]
public class InteractableButton : MonoBehaviour, IInteractable
{
    public bool isOn;
    public bool onlyTurnOn;
    public bool canInteract { get => enabled; set => enabled = value; }

    public UnityEvent activateEvent;
    public UnityEvent deactivateEvent;

    [Range(0, 360)]
    public float requiredLookAngle = 180;
    public float requiredLookDot => Mathf.Cos(requiredLookAngle);

    public string interactionDescription => "Press Button";

    public void Interact(IInteractor c)
    {
        isOn = !isOn;
        if (isOn && onlyTurnOn)
        {
            enabled = false;
            GetComponent<Collider>().enabled = false;
        }
        if (isOn)
            activateEvent.Invoke();
        else
            deactivateEvent.Invoke();
    }

    public void OnStartHighlight()
    {

    }

    public void OnEndHighlight()
    {

    }
}
