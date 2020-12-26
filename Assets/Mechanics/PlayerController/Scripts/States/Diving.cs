using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.PlayerController
{
    public class Diving : MovementState
    {
        public override string StateName => "Diving";
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

            float waterDepth = c.maxWaterStrideDepth;

            if (hits == 2)
            {
                WaterController w = waterHits[1].collider.GetComponentInParent<WaterController>();
                if (w != null)
                {
                    //Hit water and ground
                    waterDepth = waterHits[0].distance - waterHits[1].distance;
                    float scaledDepth = waterDepth / c.maxWaterStrideDepth;
                    if (scaledDepth <= 1)
                    {
                        //Within walkable water
                        c.ChangeToState<Walking>();
                    }
                }
            }


            Vector3 playerDirection = GameCameras.s.TransformInput(c.input.horizontal) * c.waterMovementForce * Time.fixedDeltaTime;
            c.rb.AddForce(playerDirection);

            //Always force player to be on water surface while simming
            transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position);

            //Transition to dive if space pressed
            if (c.holdingCrouchKey)
            {

            }
        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.horizontal.id, 0);
        }
    }
}
