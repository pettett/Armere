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

        }
        public override string StateName => "Shield Surfing";
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

            turning = c.input.horizontal.x * Time.fixedDeltaTime * c.turningTorqueForce;

            c.rb.velocity = Quaternion.Euler(0, turning, 0) * c.rb.velocity;

            //set player orientation
            transform.forward = c.rb.velocity;
            transform.rotation *= Quaternion.Euler(0, 0, c.input.horizontal.x * -c.turningAngle);
        }
        public override void Animate(AnimatorVariables vars)
        {
            animator.SetBool("IsSurfing", true);
            animator.SetBool("IsGrounded", c.onGround);
            animator.SetFloat("InputHorizontal", c.cameraController.TransformInput(c.input.horizontal).x);
            animator.SetFloat("InputVertical", c.cameraController.TransformInput(Vector2.up * c.rb.velocity.z).z);//set it to forward velocity
            animator.SetFloat("VerticalVelocity", c.rb.velocity.y);

        }
        public override void OnJump(InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started)
            {
                c.rb.AddForce(Vector3.up * c.jumpForce, ForceMode.Acceleration);
            }
        }

        public override void OnSprint(InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started)
            {
                c.ChangeToState<Walking>();
            }
        }



        public override void End()
        {
            c.collider.material = originalMaterial;

        }
    }

}