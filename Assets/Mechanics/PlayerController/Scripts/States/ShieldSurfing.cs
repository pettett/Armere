using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{



	public class ShieldSurfing : MovementState<ShieldSurfingTemplate>
	{
		SlideAnchor slide;
		public ShieldSurfing(PlayerController c, ShieldSurfingTemplate t) : base(c, t)
		{

			originalMaterial = c.collider.material;

			c.collider.material = new PhysicMaterial()
			{
				frictionCombine = PhysicMaterialCombine.Minimum,
				dynamicFriction = t.friction
			};

			c.animationController.TriggerTransition(c.transitionSet.shieldSurf);

			c.inputReader.jumpEvent += OnJump;
			c.inputReader.sprintEvent += OnSprint;

			slide = MonoBehaviour.Instantiate(t.slideAnchor, c.transform);
			slide.ac = c.animationController;
			slide.enabled = true;
		}

		public override void End()
		{
			c.collider.material = originalMaterial;

			c.animationController.TriggerTransition(c.transitionSet.freeMovement);

			c.inputReader.jumpEvent -= OnJump;
			c.inputReader.sprintEvent -= OnSprint;

			MonoBehaviour.Destroy(slide.gameObject);
		}

		public override string StateName => "Shield Surfing";
		float turning;

		PhysicMaterial originalMaterial;
		float currentSpeed;

		Vector2 movement;

		public override void FixedUpdate()
		{
			currentSpeed = c.rb.velocity.magnitude;
			if (currentSpeed <= t.minSurfingSpeed)
			{
				c.ChangeToState(c.defaultState);
			}

			movement = Vector2.Lerp(movement, c.inputReader.horizontalMovement, Time.fixedDeltaTime * t.directionChangeSpeed);
			movement.y = Mathf.Clamp01(movement.y);

			turning = movement.x * Time.fixedDeltaTime * t.turningTorqueForce;



			Vector3 forward = c.rb.velocity;
			forward.y = 0;
			//set player orientation
			transform.forward = forward;

			bool onGround = c.allCPs.Count > 0;
			slide.particleEmission = onGround;

			if (onGround)
			{
				c.rb.velocity = Quaternion.Euler(0, turning, 0) * c.rb.velocity;
				//Accelerate 
				c.rb.AddForce(c.rb.velocity.normalized * movement.y);
				c.rb.AddForce(c.allCPs[0].normal * t.groundStickForce);

				slide.transform.rotation = Quaternion.LookRotation(c.rb.velocity, c.allCPs[0].normal);
			}
			else
			{

				//In air turning does not editor velocity
				transform.Rotate(Vector3.up, turning);

				slide.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			}

		}
		public override void Update()
		{
			animator.SetBool(c.transitionSet.isGrounded.id, true); //Lie and deceive
			animator.SetFloat(c.transitionSet.horizontal.id, movement.x);
			animator.SetFloat(c.transitionSet.vertical.id, movement.y);//set it to forward velocity
			animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
		}

		public void OnJump(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started && c.allCPs.Count > 0)
			{
				c.rb.AddForce(c.allCPs[0].normal * t.jumpForce, ForceMode.VelocityChange);
			}
		}

		public void OnSprint(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				c.ChangeToState(c.defaultState);
			}
		}


	}

}