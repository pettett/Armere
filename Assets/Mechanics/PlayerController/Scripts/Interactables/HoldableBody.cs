using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.PlayerController;

[RequireComponent(typeof(Rigidbody))]
public class HoldableBody : MonoBehaviour, IInteractable
{
    public Rigidbody rb;
    public float heightOffset = 0.26f;
    public bool canInteract { get => (!(PlayerController.activePlayerController.currentState as Walking)?.holdingBody) ?? false; set { } }

    public float requiredLookDot => 0;
    public string holdableTriggerTag = "Default";
    public string interactionDescription => "Pickup";
    public FixedJoint joint;
    float oldMass;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Interact(IInteractor interactor)
    {
        oldMass = rb.mass;
        rb.mass = 0;
        joint = gameObject.AddComponent<FixedJoint>();

        (PlayerController.activePlayerController.currentState as Walking)?.HoldHoldable(this);
    }
    public void OnDropped()
    {
        rb.mass = oldMass;
        Destroy(joint);
    }

    public void OnEndHighlight()
    {

    }

    public void OnStartHighlight()
    {

    }
}
