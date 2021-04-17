using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.PlayerController;

[RequireComponent(typeof(Rigidbody))]
public class HoldableBody : MonoBehaviour, IInteractable
{
	public enum HoldableShape { Cylinder, Sphere }
	public HoldableShape shape;
	public Rigidbody rb;
	new public Collider collider;
	public float heightOffset = 0.26f;
	public bool interactable = true;
	public bool canInteract { get => interactable; set { interactable = value; } }

	public float requiredLookDot => 0;
	public string holdableTriggerTag = "Default";
	public string interactionDescription => "Pickup";

	public string interactionName => null;

	public Vector3 worldOffset => default;

	public FixedJoint joint;
	float oldMass;
	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		collider = GetComponent<Collider>();
	}
	public void Interact(IInteractor interactor)
	{
		oldMass = rb.mass;
		rb.mass = 0;
		joint = gameObject.AddComponent<FixedJoint>();

		(((PlayerController)interactor).currentState as Walking)?.HoldHoldable(this);
	}
	public void OnDropped()
	{
		rb.mass = oldMass;
		rb.AddForce(Vector3.up * 0.001f);
		Destroy(joint);
	}


	private void OnDestroy()
	{
		interactable = false;
	}
}
