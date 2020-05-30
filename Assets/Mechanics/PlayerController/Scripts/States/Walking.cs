using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [Serializable]
    [RequiresParallelState(typeof(ToggleMenus))]
    [RequiresParallelState(typeof(Interact))]
    public class Walking : MovementState
    {
        public override string StateName => "Walking";
        [System.Serializable]
        public struct WalkingProperties
        {
            public float walkingSpeed;
            public float runningSpeed;
            public float crouchingSpeed;

            public float walkingHeight;
            public float crouchingHeight;

            public float groundClamp;
            public float maxAcceleration;
            public float maxStepHeight;
            public float stepSearchOvershoot;
            public float steppingTime;
            public float jumpForce;
            public GameObject playerWeapons;
        }
        WalkingProperties p => c.m_walkingProperties;
        Vector3 currentGroundNormal = new Vector3();

        Vector3 requiredForce;
        [SerializeField] Vector3 desiredVelocity;
        //used to continue momentum when the controller hits a stair
        Vector3 lastVelocity;
        Vector3 groundVelocity;
        //shooting variables for gizmos

        [NonSerialized] public DebugMenu.DebugEntry entry;

        public override void Start()
        {
            entry = DebugMenu.CreateEntry("Player", "Velocity: {0:0.0}", 0);

            //c.controllingCamera = false; // debug for camera parallel state

            //c.transform.up = Vector3.up;

            c.rb.isKinematic = false;
        }

        bool grounded;

        bool crouching;
        bool inControl = true;
        [NonSerialized] Collider[] crouchTestColliders = new Collider[2];

        float WalkingRunningCrouching(float crouchingSpeed, float runningSpeed, float walkingSpeed)
        {
            if (crouching)
                return crouchingSpeed;
            else if (c.mod.HasFlag(MovementModifiers.Sprinting))
                return runningSpeed;
            else
                return walkingSpeed;
        }

        public override void FixedUpdate()
        {
            if (c.onGround == false)
            {
                c.ChangeToState<Freefalling>();
                return;
            }
            if (!inControl) return; //currently being controlled by some other movement coroutine

            Vector3 velocity = c.rb.velocity;
            Vector3 playerDirection = c.cameraController.TransformInput(c.input.inputWalk);

            grounded = FindGround(out ContactPoint groundCP, c.allCPs);

            if (grounded)
            {

                //step up onto the stair, reseting the velocity to what it was
                if (FindStep(out Vector3 stepUpOffset, c.allCPs, groundCP, playerDirection))
                {
                    //transform.position += stepUpOffset;
                    //c.rb.velocity = lastVelocity;

                    c.StartCoroutine(StepToPoint(transform.position + stepUpOffset, lastVelocity));
                }
            }
            else
            {
                if (!c.onGround)
                {
                    c.ChangeToState<Freefalling>();
                }
            }




            //c.transform.rotation = Quaternion.Euler(0, cc.camRotation.x, 0);




            if (c.mod.HasFlag(MovementModifiers.Crouching))
            {
                c.collider.height = p.crouchingHeight;
                crouching = true;
            }
            else if (crouching)
            {
                //crouch button not pressed but still crouching
                Vector3 p1 = transform.position + Vector3.up * p.walkingHeight * 0.05F;
                Vector3 p2 = transform.position + Vector3.up * p.walkingHeight;
                Physics.OverlapCapsuleNonAlloc(p1, p2, c.collider.radius, crouchTestColliders, c.m_groundLayerMask, QueryTriggerInteraction.Ignore);
                if (crouchTestColliders[1] == null)
                    //There is no collider intersecting other then the player
                    crouching = false;
                else crouchTestColliders[1] = null;
            }

            if (!crouching)
                c.collider.height = p.walkingHeight;

            c.collider.center = Vector3.up * c.collider.height * 0.5f;



            Vector3 desiredVelocity;

            if (playerDirection.sqrMagnitude > 0.1f)
            {
                Quaternion walkingAngle = Quaternion.LookRotation(playerDirection);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, walkingAngle, Time.deltaTime * 800);
                if (Quaternion.Angle(transform.rotation, walkingAngle) > 30f)
                {
                    //Only allow the player to walk forward if they have finished turning to the direction
                    //But do allow the player to run at a slight angle
                    desiredVelocity = Vector3.zero;
                }
                else
                {
                    //Let the player move in the direction they are pointing

                    //scale required velocity by current speed
                    //only allow sprinting if the play is moving forward
                    float speed = WalkingRunningCrouching(p.crouchingSpeed, p.runningSpeed, p.walkingSpeed);

                    desiredVelocity = playerDirection * speed;
                }

            }
            else
            {
                desiredVelocity = Vector3.zero;
            }


            requiredForce = desiredVelocity - c.rb.velocity;
            requiredForce.y = 0;

            requiredForce = Vector3.ClampMagnitude(requiredForce, p.maxAcceleration * Time.fixedDeltaTime);

            //rotate the target based on the ground the player is standing on

            requiredForce = Vector3.ProjectOnPlane(requiredForce, currentGroundNormal);

            requiredForce -= currentGroundNormal * p.groundClamp;

            c.rb.AddForce(requiredForce, ForceMode.VelocityChange);

            lastVelocity = velocity;

            entry.values[0] = c.rb.velocity.magnitude;
        }

        /// Finds the MOST grounded (flattest y component) ContactPoint
        /// \param allCPs List to search
        /// \param groundCP The contact point with the ground
        /// \return If grounded
        public static bool FindGround(out ContactPoint groundCP, List<ContactPoint> allCPs)
        {
            groundCP = default(ContactPoint);
            bool found = false;
            foreach (ContactPoint cp in allCPs)
            {
                //Pointing with some up direction
                if (cp.normal.y > 0.0001f && (found == false || cp.normal.y > groundCP.normal.y))
                {
                    groundCP = cp;
                    found = true;
                }
            }

            return found;
        }
        /// Find the first step up point if we hit a step
        /// \param allCPs List to search
        /// \param stepUpOffset A Vector3 of the offset of the player to step up the step
        /// \return If we found a step
        bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allCPs, ContactPoint groundCP, Vector3 currVelocity)
        {
            stepUpOffset = default(Vector3);

            //No chance to step if the player is not moving
            Vector2 velocityXZ = new Vector2(currVelocity.x, currVelocity.z);
            if (velocityXZ.sqrMagnitude < 0.0001f)
                return false;
            for (int i = 0; i < allCPs.Count; i++)// test if every point is suitable for a step up
            {
                if (ResolveStepUp(out stepUpOffset, allCPs[i], groundCP, currVelocity))
                    return true;
            }
            return false;
        }
        /// Takes a contact point that looks as though it's the side face of a step and sees if we can climb it
        /// \param stepTestCP ContactPoint to check.
        /// \param groundCP ContactPoint on the ground.
        /// \param stepUpOffset The offset from the stepTestCP.point to the stepUpPoint (to add to the player's position so they're now on the step)
        /// \return If the passed ContactPoint was a step
        bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, ContactPoint groundCP, Vector3 velocity)
        {
            stepUpOffset = default(Vector3);
            Collider stepCol = stepTestCP.otherCollider;

            //( 1 ) Check if the contact point normal matches that of a step (y close to 0)
            // if (Mathf.Abs(stepTestCP.normal.y) >= 0.01f)
            // {
            //     return false;
            // }

            //if the step and the ground are too close, do not count
            if (Vector3.Dot(stepTestCP.normal, groundCP.normal) > 0.95f)
            {
                return false;
            }

            //( 2 ) Make sure the contact point is low enough to be a step
            if (!(stepTestCP.point.y - groundCP.point.y < c.m_walkingProperties.maxStepHeight))
            {
                return false;
            }





            //( 2.5 ) Make sure the step is in the direction the player is moving
            Vector3 stepDirection = stepTestCP.point - transform.position;
            if (Vector3.Dot(stepDirection.normalized, velocity.normalized) < 0.01f)
            {
                //not pointing in the general direction of movement - fail
                return false;
            }


            //( 3 ) Check to see if there's actually a place to step in front of us
            //Fires one Raycast
            RaycastHit hitInfo;
            float stepHeight = groundCP.point.y + c.m_walkingProperties.maxStepHeight + 0.0001f;

            Vector3 stepTestInvDir = new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;

            //check forward based off the direction the player is walking

            Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * c.m_walkingProperties.stepSearchOvershoot);
            Vector3 direction = Vector3.down;
            if (!(stepCol.Raycast(new Ray(origin, direction), out hitInfo, c.m_walkingProperties.maxStepHeight)))
            {
                return false;
            }

            //We have enough info to calculate the points
            Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * c.m_walkingProperties.stepSearchOvershoot);
            Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundCP.point.y, stepTestCP.point.z);

            //We passed all the checks! Calculate and return the point!
            stepUpOffset = stepUpPointOffset;
            return true;
        }

        IEnumerator StepToPoint(Vector3 point, Vector3 lastVelocity)
        {
            c.rb.isKinematic = true;
            inControl = false;
            Vector3 start = transform.position;
            Vector3 pos = Vector3.zero;
            Vector2 xzStart = new Vector2(start.x, start.z);
            Vector2 xzEnd = new Vector2(point.x, point.z);
            Vector2 xz;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / p.steppingTime;
                t = Mathf.Clamp01(t);
                //lerp y values
                //first quarter of sin graph is quick at first but slower later
                pos.y = Mathf.Lerp(start.y, point.y, Mathf.Sin(t * Mathf.PI * 0.5f));
                //lerp xz values
                xz = Vector2.Lerp(xzStart, xzEnd, t);
                pos.x = xz.x;
                pos.z = xz.y;
                transform.position = pos;
                yield return new WaitForEndOfFrame();
            }
            c.rb.isKinematic = false;
            c.rb.velocity = lastVelocity;
            inControl = true;
        }


        public override void Animate(AnimatorVariables vars)
        {
            animator.SetBool(vars.surfing.id, false);

            animator.SetFloat(vars.vertical.id, c.input.inputWalk.magnitude);
            //c.animator.SetFloat("InputHorizontal", c.input.inputWalk.x);
            animator.SetFloat("WalkingSpeed", WalkingRunningCrouching(0.5f, 1f, 0.7f));
            animator.SetBool("IsGrounded", true);
            animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
            animator.SetFloat("GroundDistance", c.currentHeight);
        }

        public override void OnJump(float state)
        {
            if (state == 1 && grounded)
            {
                //use acceleration to give constant upwards force regardless of mass
                Vector3 v = c.rb.velocity;
                v.y = c.m_walkingProperties.jumpForce;
                c.rb.velocity = v;

                ChangeToState<Freefalling>();
            }
        }


        public override void OnCollideGround(RaycastHit hit)
        {
            currentGroundNormal = hit.normal;
            //Make the player stand on a platform if it is kinematic
            if (hit.rigidbody != null && hit.rigidbody.isKinematic)
            {
                groundVelocity = hit.rigidbody.velocity;
                transform.SetParent(hit.transform, true);
            }
            else
            {
                transform.SetParent(null, true);
            }


            //attempt to lock the player to the ground while walking

        }
        public override void OnCollideCliff(RaycastHit hit)
        {
            if (
                hit.rigidbody != null &&
                hit.rigidbody.isKinematic == true &&
                Vector3.Dot(-hit.normal, c.cameraController.TransformInput(c.input.inputWalk)) > 0.5f)
            {
                if (Vector3.Angle(Vector3.up, hit.normal) > c.minAngleForCliff)
                {
                    ChangeToState<Climbing>();
                }
                else
                {
                    print("did not engage climb as {0} is too shallow", Vector3.Angle(Vector3.up, hit.normal));
                }
            }
        }


        public override void End()
        {
            transform.SetParent(null, true);
            DebugMenu.RemoveEntry(entry);

            //make sure the collider is left correctly
            c.collider.height = p.walkingHeight;
            c.collider.center = Vector3.up * c.collider.height * 0.5f;
        }
        public override void OnDrawGizmos()
        {
            for (int i = 0; i < c.allCPs.Count; i++)
            {
                //draw positions the ground is touching
                Gizmos.DrawWireSphere(c.allCPs[i].point, 0.05f);
                Gizmos.DrawLine(c.allCPs[i].point, c.allCPs[i].point + c.allCPs[i].normal * 0.1f);
            }

            Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * c.m_walkingProperties.maxStepHeight, Quaternion.identity, new Vector3(1, 0, 1));
            Gizmos.color = Color.yellow;
            //draw a place to reprosent max step height
            Gizmos.DrawWireSphere(Vector3.zero, c.m_walkingProperties.stepSearchOvershoot + 0.25f);
        }
    }
}