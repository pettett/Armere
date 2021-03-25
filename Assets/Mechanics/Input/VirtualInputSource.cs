using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualInputSource : MonoBehaviour
{
	public InputReader input;
	public bool doCameraMove;
	public bool mimicMouse = true;
	public Vector2 cameraMove;
	public bool doMovement;
	public Vector2 movement;

	private void Update()
	{
		if (doCameraMove)
			input.VirtualCameraMove(cameraMove * Time.deltaTime, mimicMouse);
		if (doMovement)
			input.VirtualMovement(movement);
	}

}
