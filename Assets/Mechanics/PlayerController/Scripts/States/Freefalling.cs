using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [Serializable]
    public class Freefalling : MovementState
    {

        [System.Serializable]
        public struct FreefallingProperties
        {
            public int airJumps;
            public float airJumpVelocity;
            public float airJumpAngleFromVertical;
        }



        FreefallingProperties p => c.m_freefallingProperties;

        Vector3 desiredVelocity;

        int airJumps;


        public override void FixedUpdate()
        {
            desiredVelocity = c.cameraController.TransformInput(c.input.inputWalk);

            c.rb.AddForce(desiredVelocity);

            //only change back when the body is actually touching the ground

        }


        public override void OnCollideGround(RaycastHit hit)
        {
            //Only go to walking if they player is not moving upwards
            if (Vector3.Dot(hit.normal, Vector3.up) > c.m_maxGroundDot && c.rb.velocity.y <= 0)
            {
                ChangeToState<Walking>();
            }
        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetBool(vars.surfing.id, false);
            animator.SetFloat(vars.vertical.id, c.input.inputWalk.magnitude * (c.mod.HasFlag(MovementModifiers.Sprinting) ? c.m_walkingProperties.runningSpeed : c.m_walkingProperties.walkingSpeed));
            animator.SetBool(vars.isGrounded.id, c.onGround);
            animator.SetFloat(vars.verticalVelocity.id, c.rb.velocity.y);
            animator.SetFloat(vars.groundDistance.id, c.currentHeight);
        }

        public override void OnInteract(InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started)//shield surfing combo - shield, jump, interact
            {
                ChangeToState<Shieldsurfing>();
            }
        }

        public override void OnJump(InputActionPhase phase)
        {
            if (airJumps > 0 && phase == InputActionPhase.Started)
            {
                airJumps--;
                c.rb.AddForce(Vector3.up * (p.airJumpVelocity - c.rb.velocity.y), ForceMode.VelocityChange);
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
            airJumps = p.airJumps;
            c.animationController.enableFeetIK = false;
        }
    }
}