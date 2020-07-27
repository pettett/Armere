using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [Serializable]
    public class Climbing : MovementState
    {
        [System.Serializable]
        public struct ClimbingProperties
        {
            public float speed;
            public float distanceFromCliffFace;
        }
        ClimbingProperties p => c.m_climbingProperties;

        Vector3 currentCliffNormal;
        Vector3 currentCliffPoint;

        Vector3 scanOffsetOffset;

        public override string StateName => "Climbing";

        public override void Start()
        {
            c.rb.isKinematic = true;
        }
        public override void End()
        {
            c.rb.isKinematic = false;
        }

        public override void OnCollideCliff(RaycastHit hit)
        {
            currentCliffNormal = hit.normal.normalized;
            currentCliffPoint = hit.point;


            if (Vector3.Angle(Vector3.up, currentCliffNormal) < c.minAngleForCliff)
            {
                c.transform.up = Vector3.up;
                ChangeToState<Walking>();
            }
            c.transform.forward = -currentCliffNormal;

            scanOffsetOffset = -c.transform.up;
            scanOffsetOffset.Scale(c.m_cliffScanOffset);

            c.transform.position =
                currentCliffPoint + scanOffsetOffset
            + currentCliffNormal * p.distanceFromCliffFace
            + c.transform.up * c.input.inputWalk.y * Time.deltaTime * p.speed
          + c.transform.right * c.input.inputWalk.x * Time.deltaTime * p.speed;



            if (c.input.inputWalk.y > 0f)//player moving up,
            {
                //scan to see if the top of the cliff has been reached
                if (!Physics.Raycast(c.transform.position + c.cliffTopScanOffset, c.transform.forward, c.cliffScanningDistance, c.m_groundLayerMask, QueryTriggerInteraction.Ignore))
                {
                    c.transform.position += c.transform.forward * c.cliffScanningDistance + c.cliffTopScanOffset;
                    ChangeToState<Walking>();
                }
            }

        }
        public override void OnCollideGround(RaycastHit hit)
        {
            //allow the climb to be cancelled when payer is moving down toward ground
            if (c.input.inputWalk.y < -0.5)
            {
                ChangeToState<Walking>();
            }
        }
        public override void Animate(AnimatorVariables vars)
        {
            c.animator.SetBool("IsSurfing", false);
            c.animator.SetFloat("InputVertical", c.input.inputWalk.magnitude * (c.mod.HasFlag(MovementModifiers.Sprinting) ? c.m_walkingProperties.runningSpeed : c.m_walkingProperties.walkingSpeed));
            c.animator.SetBool("IsGrounded", c.onGround);
            c.animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
            c.animator.SetFloat("GroundDistance", c.currentHeight);
        }
        public override void OnJump(InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started)//make player jump away from 
            {
                c.rb.isKinematic = false;
                c.rb.AddForce(currentCliffNormal * c.jumpForce, ForceMode.Acceleration);
                ChangeToState<Freefalling>();
            }
        }

        public override void OnDrawGizmos()
        {
            Gizmos.DrawLine(currentCliffPoint, currentCliffPoint + currentCliffNormal);
        }
    }

}