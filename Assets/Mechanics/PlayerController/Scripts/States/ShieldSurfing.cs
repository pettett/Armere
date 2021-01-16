using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{


	[Serializable]
	public class Shieldsurfing : MovementState
	{

		public override void Start()
		{
			originalMaterial = c.collider.material;
			c.collider.material = c.surfPhysicMat;


			c.inputReader.jumpEvent += OnJump;
			c.inputReader.sprintEvent += OnSprint;
			c.inputReader.movementEvent += OnInputHorizontal;
		}

		public override void End()
		{
			c.collider.material = originalMaterial;

			c.inputReader.jumpEvent -= OnJump;
			c.inputReader.sprintEvent -= OnSprint;
			c.inputReader.movementEvent -= OnInputHorizontal;
		}

		public override string StateName => "Shield Surfing";
		public override char StateSymbol => 's';
		float turning;

		PhysicMaterial originalMaterial;
		float currentSpeed;

		public override void FixedUpdate()
		{
			currentSpeed = c.rb.velocity.magnitude;
			if (currentSpeed <= c.minSurfingSpeed)
			{
				c.ChangeToState<Walking>();
			}

			turning = inputHorizontal.x * Time.fixedDeltaTime * c.turningTorqueForce;

			c.rb.velocity = Quaternion.Euler(0, turning, 0) * c.rb.velocity;

			//set player orientation
			transform.forward = c.rb.velocity;
			transform.rotation *= Quaternion.Euler(0, 0, inputHorizontal.x * -c.turningAngle);
		}
		public override void Animate(AnimatorVariables vars)
		{
			animator.SetBool("IsSurfing", true);
			animator.SetBool("IsGrounded", c.onGround);
			animator.SetFloat("InputHorizontal", GameCameras.s.TransformInput(inputHorizontal).x);
			animator.SetFloat("InputVertical", GameCameras.s.TransformInput(Vector2.up * c.rb.velocity.z).z);//set it to forward velocity
			animator.SetFloat("VerticalVelocity", c.rb.velocity.y);

		}
		public void OnJump(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				c.rb.AddForce(Vector3.up * c.jumpForce, ForceMode.Acceleration);
			}
		}

		public void OnSprint(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				c.ChangeToState<Walking>();
			}
		}
		Vector2 inputHorizontal;

		public void OnInputHorizontal(Vector2 input)
		{
			inputHorizontal = input;
		}


	}

}