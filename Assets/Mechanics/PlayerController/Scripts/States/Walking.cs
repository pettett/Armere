using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [
        Serializable,
        RequiresParallelState(typeof(ToggleMenus)),
        RequiresParallelState(typeof(Interact))
    ]
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
            public float maxStepDown;
            public float stepSearchOvershoot;
            public float steppingTime;
            public float jumpForce;
            public GameObject playerWeapons;
        }
        WalkingProperties p => c.m_walkingProperties;
        Vector3 currentGroundNormal = new Vector3();

        Vector3 requiredForce;
        Vector3 desiredVelocity;
        //used to continue momentum when the controller hits a stair
        Vector3 lastVelocity;
        Vector3 groundVelocity;


        //shooting variables for gizmos
        [NonSerialized] public DebugMenu.DebugEntry entry;


        bool grounded;

        bool crouching;
        bool inControl = true;
        [NonSerialized] Collider[] crouchTestColliders = new Collider[2];
        [NonSerialized] ContactPoint groundCP;
        int currentSidearm = -1;
        bool sidearmHolstered = false;
        public override void Start()
        {
            entry = DebugMenu.CreateEntry("Player", "Velocity: {0:0.0} Contact Point Count {1} Stepping Progress {2} On Ground {3}", 0, 0, 0, false);

            //c.controllingCamera = false; // debug for camera parallel state

            //c.transform.up = Vector3.up;
            c.animationController.enableFeetIK = true;
            c.rb.isKinematic = false;
            InventoryController.singleton.OnSelectItemEvent += OnSelectItem;
        }

        public override void End()
        {
            transform.SetParent(null, true);
            DebugMenu.RemoveEntry(entry);

            //make sure the collider is left correctly
            c.collider.height = p.walkingHeight;
            c.collider.center = Vector3.up * c.collider.height * 0.5f;
            InventoryController.singleton.OnSelectItemEvent -= OnSelectItem;
        }
        public void OnSelectItem(ItemType type, int index)
        {
            switch (type)
            {
                case ItemType.Weapon:
                    OnSelectWeapon(index);
                    break;
                case ItemType.SideArm:
                    SelectSidearm(index);
                    break;
            }
        }


        float WalkingRunningCrouching(float crouchingSpeed, float runningSpeed, float walkingSpeed)
        {
            if (crouching)
                return crouchingSpeed;
            else if (c.mod.HasFlag(MovementModifiers.Sprinting))
                return runningSpeed;
            else
                return walkingSpeed;
        }
        int currentWeapon = -1;
        public override void OnSelectWeapon(int index)
        {
            if (InventoryController.singleton.weapon.items.Count > index)
            {
                currentWeapon = index;
                c.weaponGraphicsController.SetHeldWeapon(InventoryController.singleton.weapon.items[index].name, c.db);
            }
        }

        public void SelectSidearm(int index)
        {
            if (InventoryController.singleton.sideArm.items.Count > index)
            {
                if (currentSidearm != -1)
                    foreach (var p in c.db[InventoryController.singleton.sideArm.items[currentSidearm].name].properties)
                        p.OnItemDeEquip(animator);

                foreach (var p in c.db[InventoryController.singleton.sideArm.items[index].name].properties)
                    p.OnItemEquip(animator);
                currentSidearm = index;
                sidearmHolstered = false;
                c.weaponGraphicsController.SetHeldSidearm(InventoryController.singleton.sideArm.items[index].name, c.db);
            }
        }
        public void DeEquipSidearm()
        {
            if (currentSidearm != -1)
            {
                foreach (var p in c.db[InventoryController.singleton.sideArm.items[currentSidearm].name].properties)
                    p.OnItemDeEquip(animator);
                c.weaponGraphicsController.RemoveSidearm();
                sidearmHolstered = true;
            }
        }
        bool requestedSwordSwing = false;
        public override void OnAttack(float state)
        {
            if (state == 1 && currentWeapon != -1)
            {
                if (inControl)
                {
                    c.weaponGraphicsController.swordSheathed = false;
                    c.StartCoroutine(SwingSword());
                }
                else
                {
                    requestedSwordSwing = true;
                }
            }
        }

        IEnumerator SwingSword()
        {
            inControl = false;
            //swing the sword

            animator.SetTrigger("Swing Sword");
            requestedSwordSwing = false;
            //Check for triggers from the sword
            void OnTrigger(Collider other)
            {
                if (other.TryGetComponent<IAttackable>(out IAttackable a))
                {
                    a.Attack();
                }
            }

            var ccc = c.weaponGraphicsController.heldWeapon.AddComponent<MeshCollider>();
            ccc.convex = true;
            ccc.isTrigger = true;
            var trig = c.weaponGraphicsController.heldWeapon.AddComponent<WeaponTrigger>();
            trig.onTriggerEnter = OnTrigger;
            //Make the player take a step forward
            yield return new WaitForSeconds(0.583f);


            //If the player has clicked again in the time of the first swing, 
            if (requestedSwordSwing == true)
            {
                //Swing the sword back
                animator.SetTrigger("Swing Sword");
                yield return new WaitForSeconds(0.583f);
            }
            //Clean up the trigger detection of the sword
            MonoBehaviour.Destroy(ccc);
            MonoBehaviour.Destroy(trig);

            inControl = true;

        }

        public override void OnAltAttack(float state)
        {
            if (inControl && state == 1)
            {
                if (sidearmHolstered)
                {
                    SelectSidearm(currentSidearm);
                }
            }
        }

        public override void FixedUpdate()
        {
            // if (c.onGround == false)
            // {
            //     //c.ChangeToState<Freefalling>();
            //     return;
            // }

            if (!inControl) return; //currently being controlled by some other movement coroutine

            Vector3 velocity = c.rb.velocity;
            Vector3 playerDirection = c.cameraController.TransformInput(c.input.inputWalk);

            grounded = FindGround(out groundCP, out currentGroundNormal, c.allCPs);

            c.animationController.enableFeetIK = grounded;

            if (c.mod.HasFlag(MovementModifiers.Sprinting))
            {
                c.weaponGraphicsController.swordSheathed = true;
                //Will only operate if sidearm exists
                DeEquipSidearm();
            }

            //List<ContactPoint> groundCPs = new List<ContactPoint>();

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
                    //c.ChangeToState<Freefalling>();
                }
            }
            float speedScalar = 1;
            if (c.currentWater != null)
            {
                //Find depth of water
                //Buffer of two: One for water surface, one for water base
                RaycastHit[] waterHits = new RaycastHit[2];
                float heightOffset = 2;
                float maxStrideDepth = 1;
                int hits = Physics.RaycastNonAlloc(transform.position + new Vector3(0, heightOffset, 0), Vector3.down, waterHits, maxStrideDepth + heightOffset, c.m_groundLayerMask, QueryTriggerInteraction.Collide);
                print(hits.ToString());

                if (hits == 2)
                {
                    WaterController w = waterHits[1].collider.GetComponentInParent<WaterController>();
                    if (w != null)
                    {
                        //Hit water and ground
                        print("Hit water and ground");
                        float depth = waterHits[0].distance - waterHits[1].distance;

                        float scaledDepth = depth / maxStrideDepth;
                        if (scaledDepth > 1)
                        {
                            //Start swimming
                            print("Too deep to walk");
                        }
                        else if (scaledDepth >= 0)
                        {
                            //Striding through water
                            //Slow speed of walk
                            //Full depth walks at half speed
                            speedScalar = scaledDepth * 0.5f + 0.5f;
                            print("Striding in water");
                        }

                    }

                }
                else if (hits == 1)
                {

                    //Start swimming
                    print("Too deep to walk");
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
                    //Include speed scalar from water
                    desiredVelocity = playerDirection * speed * speedScalar;
                    //Rotate the velocity based on ground
                    desiredVelocity = Quaternion.AngleAxis(0, currentGroundNormal) * desiredVelocity;
                }

            }
            else
            {
                //No movement
                desiredVelocity = Vector3.zero;
            }

            requiredForce = desiredVelocity - c.rb.velocity;
            requiredForce.y = 0;

            requiredForce = Vector3.ClampMagnitude(requiredForce, p.maxAcceleration * Time.fixedDeltaTime);

            //rotate the target based on the ground the player is standing on


            if (grounded)
                requiredForce -= currentGroundNormal * p.groundClamp;

            c.rb.AddForce(requiredForce, ForceMode.VelocityChange);

            lastVelocity = velocity;

            entry.values[0] = c.rb.velocity.magnitude;
            entry.values[1] = c.allCPs.Count;
            entry.values[3] = grounded;
        }

        /// Finds the MOST grounded (flattest y component) ContactPoint
        /// \param allCPs List to search
        /// \param groundCP The contact point with the ground
        /// \return If grounded
        public bool FindGround(out ContactPoint groundCP, out Vector3 groundNormal, List<ContactPoint> allCPs)
        {
            groundCP = default;

            bool found = false;
            float dot;
            float bestDirectionDot = 1;
            groundNormal = default;
            foreach (ContactPoint cp in allCPs)
            {
                dot = Vector3.Dot(Vector3.up, cp.normal);

                //Pointing with some up direction
                if (dot > c.m_maxGroundDot)
                {

                    //Get the most upwards pointing contact point

                    //Also get the point that points most in the current direction the player desires to move
                    float directionDot = Vector3.Dot(cp.normal, desiredVelocity);



                    if (found == false || /*cp.normal.y > groundCP.normal.y ||*/ directionDot < bestDirectionDot)
                    {
                        groundCP = cp;
                        bestDirectionDot = directionDot;
                        found = true;
                        groundNormal = cp.normal;
                    }
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

        bool debugStep = false;

        /// Takes a contact point that looks as though it's the side face of a step and sees if we can climb it
        /// \param stepTestCP ContactPoint to check.
        /// \param groundCP ContactPoint on the ground.
        /// \param stepUpOffset The offset from the stepTestCP.point to the stepUpPoint (to add to the player's position so they're now on the step)
        /// \return If the passed ContactPoint was a step
        bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, ContactPoint groundCP, Vector3 velocity)
        {
            stepUpOffset = default;
            Collider stepCol = stepTestCP.otherCollider;

            //( 1 ) Check if the contact point normal matches that of a step (y close to 0)
            // if (Mathf.Abs(stepTestCP.normal.y) >= 0.01f)
            // {
            //     return false;
            // }

            //if the step and the ground are too close, do not count
            if (Vector3.Dot(stepTestCP.normal, Vector3.up) > c.m_maxGroundDot)
            {
                if (debugStep) print("Contact too close to ground normal {0}", Vector3.Dot(stepTestCP.normal, Vector3.up));
                return false;
            }

            //( 2 ) Make sure the contact point is low enough to be a step
            if (!(stepTestCP.point.y - groundCP.point.y < p.maxStepHeight))
            {
                if (debugStep) print("Contact to high");
                return false;
            }


            //( 2.5 ) Make sure the step is in the direction the player is moving
            if (Vector3.Dot(-stepTestCP.normal, velocity.normalized) < 0.01f)
            {
                if (debugStep) print(Vector3.Dot(-stepTestCP.normal, velocity.normalized).ToString());
                //not pointing in the general direction of movement - fail
                return false;
            }

            //( 3 ) Check to see if there's actually a place to step in front of us
            //Fires one Raycast
            RaycastHit hitInfo;
            float stepHeight = groundCP.point.y + p.maxStepHeight + 0.0001f;

            Vector3 stepTestInvDir = velocity.normalized; // new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;

            //check forward based off the direction the player is walking

            Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * p.stepSearchOvershoot);
            Vector3 direction = Vector3.down;
            if (!stepCol.Raycast(new Ray(origin, direction), out hitInfo, p.maxStepHeight + p.maxStepDown))
            {
                if (debugStep) print("Nothing to step to");
                return false;
            }

            //We have enough info to calculate the points
            Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * p.stepSearchOvershoot);
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
                entry.values[2] = t;
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
            entry.values[2] = 0;
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
            animator.SetBool("Crouching", crouching);
        }

        public override void OnJump(float state)
        {
            if (state == 1 && grounded)
            {
                //use acceleration to give constant upwards force regardless of mass
                Vector3 v = c.rb.velocity;
                v.y = c.m_walkingProperties.jumpForce;
                c.rb.velocity = v;

                //ChangeToState<Freefalling>();
            }
        }


        public override void OnCollideGround(RaycastHit hit)
        {
            //currentGroundNormal = hit.normal;
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
        // public override void OnCollideCliff(RaycastHit hit)
        // {
        //     if (
        //         hit.rigidbody != null &&
        //         hit.rigidbody.isKinematic == true &&
        //         Vector3.Dot(-hit.normal, c.cameraController.TransformInput(c.input.inputWalk)) > 0.5f)
        //     {
        //         if (Vector3.Angle(Vector3.up, hit.normal) > c.minAngleForCliff)
        //         {
        //             ChangeToState<Climbing>();
        //         }
        //         else
        //         {
        //             print("did not engage climb as {0} is too shallow", Vector3.Angle(Vector3.up, hit.normal));
        //         }
        //     }
        // }


        public override void OnDrawGizmos()
        {
            FindGround(out groundCP, out currentGroundNormal, c.allCPs);
            for (int i = 0; i < c.allCPs.Count; i++)
            {
                //draw positions the ground is touching
                if (c.allCPs[i].point == groundCP.point)
                    Gizmos.color = Color.red;
                else //Change color of cps depending on importance
                    Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(c.allCPs[i].point, 0.05f);
                Gizmos.DrawLine(c.allCPs[i].point, c.allCPs[i].point + c.allCPs[i].normal * 0.1f);
            }
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + desiredVelocity);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + requiredForce.normalized);
            Gizmos.DrawLine(transform.position, transform.position + currentGroundNormal.normalized * 0.5f);

            Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * c.m_walkingProperties.maxStepHeight, Quaternion.identity, new Vector3(1, 0, 1));
            Gizmos.color = Color.yellow;
            //draw a place to reprosent max step height
            Gizmos.DrawWireSphere(Vector3.zero, c.m_walkingProperties.stepSearchOvershoot + 0.25f);
        }
    }
}