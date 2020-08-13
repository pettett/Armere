using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerController
{
    public class Swimming : MovementState
    {
        public override string StateName => "Swimming";

        bool onSurface = true;
        const string animatorVariable = "IsSwimming";
        void ChangeDive(bool diving)
        {
            onSurface = !diving;
            animator.SetBool(c.animatorVariables.isGrounded.id, onSurface);
        }

        public override void Start()
        {
            c.rb.useGravity = false;
            c.rb.drag = c.waterDrag;
            c.animationController.enableFeetIK = false;
            c.animator.SetBool(animatorVariable, true);

            onSurface = c.rb.velocity.y > -1;
            Debug.Log(c.rb.velocity.y);
        }
        public override void End()
        {
            c.rb.useGravity = true;
            c.animator.SetBool(animatorVariable, false);
        }


        public override void FixedUpdate()
        {
            //Test to see if the player is still in deep water

            RaycastHit[] waterHits = new RaycastHit[2];
            float heightOffset = 2;
            int hits = Physics.RaycastNonAlloc(
                transform.position + new Vector3(0, heightOffset, 0),
                Vector3.down, waterHits, c.maxWaterStrideDepth + heightOffset,
                c.m_groundLayerMask, QueryTriggerInteraction.Collide);


            if (hits == 2)
            {
                WaterController w = waterHits[1].collider.GetComponentInParent<WaterController>();
                if (w != null)
                {
                    //Hit water and ground
                    float waterDepth = waterHits[0].distance - waterHits[1].distance;
                    if (waterDepth <= c.maxWaterStrideDepth)
                    {
                        //Within walkable water
                        c.ChangeToState<Walking>();
                    }
                }
            }
            //If underwater and going up but close to the surface, return to surface
            if (!onSurface && c.rb.velocity.y >= 0 && hits > 0 && waterHits[0].distance < heightOffset && heightOffset - waterHits[0].distance < c.maxWaterStrideDepth)
                ChangeDive(false);


            Vector3 playerDirection;

            if (onSurface)
                playerDirection = c.cameraController.TransformInput(c.input.horizontal) * c.waterMovementForce * Time.fixedDeltaTime;
            else
            {
                playerDirection = GameCameras.s.cameraTransform.TransformDirection(new Vector3(c.input.horizontal.x, c.input.vertical, c.input.horizontal.y)) * c.waterMovementForce * Time.fixedDeltaTime;

                transform.forward = playerDirection;

            }

            c.rb.AddForce(playerDirection);

            if (onSurface)
                //Always force player to be on water surface while simming
                transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position + Vector3.up * 1000) - Vector3.up * c.maxWaterStrideDepth * 0.5f;
            else
                transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position);

            //Transition to dive if space pressed
            if (onSurface && c.mod.HasFlag(MovementModifiers.Crouching))
            {
                ChangeDive(true);
                transform.position -= Vector3.up * c.maxWaterStrideDepth;
            }
        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.horizontal.id, 0);
        }
    }
}
