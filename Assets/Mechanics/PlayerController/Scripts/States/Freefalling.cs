using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{

	public class Freefalling : MovementState<FreefallingTemplate>
	{
		public override string StateName => "Falling";

		Vector3 desiredVelocity;

		float currentHeight;

		public Freefalling(PlayerController c, FreefallingTemplate t) : base(c, t)
		{

		}



		public override void FixedUpdate()
		{
			//desiredVelocity = GameCameras.s.TransformInput(c.input.horizontal);


			if (Physics.Raycast(transform.position, c.WorldDown, out RaycastHit hit, float.PositiveInfinity, c.m_groundLayerMask, QueryTriggerInteraction.Ignore))
			{
				currentHeight = hit.distance;
			}

			Vector3 fallingVel = Vector3.Scale(c.rb.velocity, c.WorldUp);

			Vector3 u = new Vector3(Mathf.Abs(c.WorldDown.x), Mathf.Abs(c.WorldDown.y), Mathf.Abs(c.WorldDown.z));

			float d = Vector3.Dot(fallingVel, u);

			if (c.allCPs.Count > 0 && d < 0)
			{
				c.ChangeToState(c.defaultState);
			}

			//only change back when the body is actually touching the ground

		}

		public override void Update()
		{
			//animator.SetFloat(vars.vertical.id, c.input.horizontal.magnitude);
			animator.SetBool(c.transitionSet.isGrounded.id, false);
			animator.SetFloat(c.transitionSet.verticalVelocity.id, c.rb.velocity.y);
			animator.SetFloat(c.transitionSet.groundDistance.id, currentHeight);
		}


		public void OnInteract(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)//shield surfing combo - shield, jump, interact
			{
				c.ChangeToState(t.airInteract);
			}
		}

		// public override void OnCollideCliff(RaycastHit hit)
		// {
		//     if (c.input.inputWalk.sqrMagnitude > 0.5f)
		//     {
		//         ChangeToState<Climbing>();
		//     }
		// }


		public override void Start()
		{
			c.StartCoroutine(c.UnEquipAll());
			c.animationController.enableFeetIK = false;
			c.allCPs.Clear();
			c.inputReader.actionEvent += OnInteract;
		}
		public override void End()
		{
			base.End();

			c.inputReader.actionEvent -= OnInteract;
		}

	}
}