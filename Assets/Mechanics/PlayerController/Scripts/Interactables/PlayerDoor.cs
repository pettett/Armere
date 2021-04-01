using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.PlayerController;

public class PlayerDoor : MonoBehaviour, IInteractable
{
	public bool canInteract { get; set; } = true;

	public float walkingDistance = 1;
	public float doorStepDistance = 0.1f;
	public Transform leftDoorPivot;
	public Transform rightDoorPivot;
	public float doorAngularSpeed = 45;
	public float doorWidth = 1;
	[Range(0, 360)]
	public float requiredLookAngle = 180;
	public float requiredLookDot => Mathf.Cos(requiredLookAngle);

	public string interactionDescription => "Open";

	public string interactionName => null;

	public Vector3 worldOffset => default;

	public Room forwardRoom;
	public Room backwardRoom;

	public void Interact(IInteractor interactor)
	{
		if (interactor is PlayerController player)
		{
			StartCoroutine(DoorUseageRoutine(player));
		}
	}

	IEnumerator ChangeDoorRotation(Transform doorPivot, Quaternion to)
	{
		float diff;
		do
		{
			doorPivot.localRotation = Quaternion.RotateTowards(doorPivot.localRotation, to, doorAngularSpeed * Time.deltaTime);
			diff = Quaternion.Angle(doorPivot.localRotation, to);
			yield return null;
		} while (diff > 0.5f);

		doorPivot.localRotation = to;
	}

	IEnumerator DoorUseageRoutine(PlayerController player)
	{
		//Work out what side of the door the player is on
		//Unlikely to equal 0 
		int doorSide = Vector3.Dot(transform.forward, transform.position - player.transform.position) > 0 ? -1 : 1;

		if (doorSide == 1)
		{
			player.EnterRoom(backwardRoom);
		}
		else
		{
			player.EnterRoom(forwardRoom);
		}


		bool leftDoor = Vector3.Dot(transform.right, transform.position - player.transform.position) > 0;
		Transform doorPivot = leftDoor ? leftDoorPivot : rightDoorPivot;
		if (doorPivot == null)
		{
			//Use the other door pivot
			leftDoor = !leftDoor;
			doorPivot = leftDoor ? leftDoorPivot : rightDoorPivot;
		}

		var autowalk = (AutoWalking)player.ChangeToState(player.autoWalk);

		//Needs to go to the left if using the right door pivot
		Vector3 doorCenter = transform.TransformPoint(doorPivot.localPosition + Vector3.right * doorWidth * 0.5f * (leftDoor ? 1 : -1));

		//Walk to the start
		autowalk.WalkTo(doorCenter + doorSide * transform.forward * doorStepDistance);

		yield return autowalk.WaitForAgent(0.5f);
		//Start opening the door - door has to be open and agent at doorstep to continue
		yield return ChangeDoorRotation(doorPivot, Quaternion.Euler(0, 90 * doorSide * (leftDoor ? 1 : -1), 0));

		yield return autowalk.WaitForAgent();

		Vector3 forward = -transform.forward * doorSide;
		forward.y = 0;
		player.transform.forward = forward;

		//Walk to the end
		autowalk.WalkTo(doorCenter - doorSide * transform.forward * walkingDistance);


		yield return autowalk.WaitForAgent();

		//Close the door
		yield return ChangeDoorRotation(doorPivot, Quaternion.identity);

		//Return control
		player.ChangeToState(player.defaultState);
	}


	private void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		if (leftDoorPivot != null)
		{
			var c = leftDoorPivot.localPosition + Vector3.right * doorWidth * 0.5f;

			Gizmos.DrawLine(c + Vector3.forward * walkingDistance, c + Vector3.forward * -walkingDistance);
		}
		if (rightDoorPivot != null)
		{
			var c = rightDoorPivot.localPosition + Vector3.left * doorWidth * 0.5f;

			Gizmos.DrawLine(c + Vector3.forward * walkingDistance, c + Vector3.forward * -walkingDistance);
		}
	}
}
