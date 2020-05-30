using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{


    [Serializable]
    public class Shieldsurfing : MovementState
    {

        public override void Start()
        {
            originalMaterial = c.collider.material;
            c.collider.material = p.surfPhysicMat;
        }
        public override string StateName => "Shield Surfing";
        [System.Serializable]
        public struct ShieldsurfingProperties
        {
            public float turningTorqueForce;
            public float minSurfingSpeed;
            public float turningAngle;

            public PhysicMaterial surfPhysicMat;
        }
        ShieldsurfingProperties p => c.m_shieldsurfingProperties;
        float turning;

        PhysicMaterial originalMaterial;
        float currentSpeed;

        public override void FixedUpdate()
        {
            currentSpeed = c.rb.velocity.magnitude;
            if (currentSpeed <= p.minSurfingSpeed)
            {
                c.ChangeToState<Walking>();
            }

            turning = c.input.inputWalk.x * Time.fixedDeltaTime * p.turningTorqueForce;

            c.rb.velocity = Quaternion.Euler(0, turning, 0) * c.rb.velocity;

            //set player orientation
            transform.forward = c.rb.velocity;
            transform.rotation *= Quaternion.Euler(0, 0, c.input.inputWalk.x * -p.turningAngle);
        }
        public override void Animate(AnimatorVariables vars)
        {
            animator.SetBool("IsSurfing", true);
            animator.SetBool("IsGrounded", c.onGround);
            animator.SetFloat("InputHorizontal", c.cameraController.TransformInput(c.input.inputWalk).x);
            animator.SetFloat("InputVertical", c.cameraController.TransformInput(Vector2.up * c.rb.velocity.z).z);//set it to forward velocity
            animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
        }
        public override void OnJump(float state)
        {
            if (state == 1)
            {
                c.rb.AddForce(Vector3.up * c.jumpForce, ForceMode.Acceleration);
            }
        }

        public override void OnSprint(float state)
        {
            if (state == 1)
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