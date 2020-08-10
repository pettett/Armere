using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerController
{
    public class Swimming : MovementState
    {
        public override string StateName => "Swimming";
        public override void Start()
        {
            c.rb.useGravity = false;
            c.rb.drag = c.waterDrag;
        }
        public override void End()
        {
            c.rb.useGravity = true;
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
                    float depth = waterHits[0].distance - waterHits[1].distance;
                    float scaledDepth = depth / c.maxWaterStrideDepth;
                    if (scaledDepth <= 1)
                    {
                        //Within walkable water
                        c.ChangeToState<Walking>();
                    }
                }

            }


            Vector3 playerDirection = c.cameraController.TransformInput(c.input.inputWalk) * c.waterMovementForce * Time.fixedDeltaTime;
            c.rb.AddForce(playerDirection);

            //Always force player to be on water surface while simming
            transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position + Vector3.up * 1000) - Vector3.up * c.maxWaterStrideDepth * 0.5f;

            //Transition to dive if space pressed
        }
        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.horizontal.id, 0);
        }
    }
}
