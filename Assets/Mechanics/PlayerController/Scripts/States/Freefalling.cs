using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{
	[Serializable]
	public class Freefalling : MovementState
	{

		public override char StateSymbol => 'F';

		Vector3 desiredVelocity;

		int airJumps;


		public override void FixedUpdate()
		{
			//desiredVelocity = GameCameras.s.TransformInput(c.input.horizontal);

			//c.rb.AddForce(desiredVelocity);

			//only change back when the body is actually touching the ground

		}

		public override void Animate(AnimatorVariables vars)
		{
			animator.SetBool(vars.surfing.id, false);
			//animator.SetFloat(vars.vertical.id, c.input.horizontal.magnitude);
			animator.SetBool(vars.isGrounded.id, c.onGround);
			animator.SetFloat(vars.verticalVelocity.id, c.rb.velocity.y);
			animator.SetFloat(vars.groundDistance.id, c.currentHeight);
		}

		public void OnInteract(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)//shield surfing combo - shield, jump, interact
			{
				ChangeToState<Shieldsurfing>();
			}
		}

		public void OnJump(InputActionPhase phase)
		{
			if (airJumps > 0 && phase == InputActionPhase.Started)
			{
				airJumps--;
				c.rb.AddForce(Vector3.up * (10 - c.rb.velocity.y), ForceMode.VelocityChange);
			}
		}
		// public override void OnCollideCliff(RaycastHit hit)
		// {
		//     if (c.input.inputWalk.sqrMagnitude > 0.5f)
		//     {
		//         ChangeToState<Climbing>();
		//     }
		// }
		public override string StateName => "Falling";

		public override void Start()
		{
			airJumps = 0;
			c.animationController.enableFeetIK = false;

			c.inputReader.jumpEvent += OnJump;
			c.inputReader.actionEvent += OnInteract;
		}
		public override void End()
		{
			base.End();

			c.inputReader.jumpEvent -= OnJump;
			c.inputReader.actionEvent -= OnInteract;
		}

	}
}