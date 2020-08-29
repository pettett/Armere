using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
namespace PlayerController
{

    /// <summary> Climb climbable objects
    [System.Serializable]
    [RequiresParallelState(typeof(ToggleMenus))]
    public class LadderClimb : MovementState
    {
        public override string StateName => "Climbing Ladder";
        Climbable ladder;
        float height;


        const float footLadderHeight = 0.1f;
        const float handLadderHeight = 1.4f;

        Vector3 currentNormal;
        Vector3 currentPosition;
        bool collidingTop = false;
        float oldColliderHeight;
        public override void Start(params object[] info)
        {
            if (info.Length > 0 && info[0] is Climbable climbable)
                ladder = climbable;
            else
                throw new System.ArgumentException("A Ladder object must be supplied to this state");

            oldColliderHeight = c.collider.height;
            c.collider.height = c.climbingColliderHeight;
            switch (ladder.surfaceType)
            {
                case Climbable.ClimbableSurface.Line:
                    //calculate height the player is at if they come at the ladder from above
                    height = Mathf.Clamp(ladder.transform.InverseTransformPoint(transform.position).y, 0, ladder.ladderHeight - 0.1f);
                    transform.SetPositionAndRotation(GetLadderPos(height), ladder.transform.rotation);
                    break;
                case Climbable.ClimbableSurface.Mesh:
                    //Place player below climb-up threshold
                    var point = ladder.GetClosestPointOnMesh(transform.position + Vector3.down * c.collider.height * 1.1f);
                    //Debug.Log(point.point);
                    currentPosition = point.point;
                    currentNormal = point.normal;
                    MovePlayerToMesh(point.point, point.normal);

                    break;

            }
            c.rb.isKinematic = true;
            c.animationController.enableFeetIK = false;
            animator.SetBool("OnLadder", true);
        }

        public Vector3 GetLadderPos(float h)
        {
            return ladder.LadderPosAtHeight(h, c.collider.radius * 2f);
        }

        public override void End()
        {
            //Dont do if manually moved
            c.collider.height = oldColliderHeight;
            //prepare for climb up animation
            c.animator.SetFloat(c.animatorVariables.horizontal.id, 0);
            c.animator.SetFloat(c.animatorVariables.vertical.id, 0);
            animator.applyRootMotion = true;
            animator.SetBool("OnLadder", false);
        }

        public int RoundToNearest(float value, int interval, int offset) => Mathf.RoundToInt((value - offset) / interval) * interval + offset;

        float lhRung, rhRung, lfRung, rfRung = 0;

        void UpdateRung(ref float rung, float height, int offset)
        {
            rung = Mathf.Lerp(rung, RoundToNearest(height, 2, offset), Time.deltaTime * 25);//Closest even rung
        }



        public override void OnAnimatorIK(int layerIndex)
        {
            // float rung = (height + handLadderHeight - ladder.rungOffset) / ladder.rungDistance;
            // //Update the position of every body part
            // UpdateRung(ref lhRung, rung, 0);
            // UpdateRung(ref rhRung, rung, 1);
            // //The height of feet is lower then the hands ; target a different position
            // rung = (height + footLadderHeight - ladder.rungOffset) / ladder.rungDistance;
            // UpdateRung(ref lfRung, rung, 0);
            // UpdateRung(ref rfRung, rung, 1);

            // SetPosition(AvatarIKGoal.LeftHand, "LeftHandCurve", ladder.LadderPosByRung(lhRung, -0.1f));
            // SetPosition(AvatarIKGoal.RightHand, "RightHandCurve", ladder.LadderPosByRung(rhRung, 0.1f));

            // //Do the same for feet
            // SetPosition(AvatarIKGoal.LeftFoot, "LeftFootCurve", ladder.LadderPosByRung(lfRung, -0.1f));
            // SetPosition(AvatarIKGoal.RightFoot, "RightFootCurve", ladder.LadderPosByRung(rfRung, 0.1f));
        }

        void SetPosition(AvatarIKGoal goal, string curve, Vector3 pos)
        {
            animator.SetIKPositionWeight(goal, animator.GetFloat(curve));
            animator.SetIKPosition(goal, pos);
        }
        public override void OnJump(InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started)
            {
                //Jump off the ladder
                c.rb.isKinematic = false;
                c.rb.AddForce(ladder.transform.forward * -100);
                c.ChangeToState<Walking>();
            }
        }

        void MovePlayerToMesh(Vector3 pos, Vector3 normal)
        {
            transform.position = pos + normal * c.collider.radius;
            transform.forward = -normal;
        }

        public override void Update()
        {
            switch (ladder.surfaceType)
            {
                case Climbable.ClimbableSurface.Line:
                    height += c.input.horizontal.y * Time.deltaTime * c.climbingSpeed;
                    height = Mathf.Clamp(height, 0, ladder.ladderHeight);

                    transform.position = GetLadderPos(height);
                    if (height == ladder.ladderHeight)
                    {
                        //Get off at the top of the ladder
                        transform.position = GetLadderPos(height) + ladder.transform.forward * 1f;
                        c.ChangeToState<TransitionState<Walking>>(c.transitionTime);
                    }
                    break;
                case Climbable.ClimbableSurface.Mesh:
                    //Move with input on the mesh
                    Vector3 leftTangent = -Vector3.Cross(Vector3.up, currentNormal).normalized;
                    Vector3 upTangent = Vector3.Cross(leftTangent, currentNormal);


                    Vector3 deltaPos = (leftTangent * c.input.horizontal.x + upTangent * c.input.horizontal.y) * Time.deltaTime * c.climbingSpeed;
                    currentPosition += deltaPos;

                    var point = ladder.GetClosestPointOnMesh(currentPosition);

                    bool hittingBase = deltaPos.y < 0 && currentPosition.y - point.point.y <= deltaPos.y * 0.5f;

                    //print("delta {0}:real {1}: hitting {2}", deltaPos.y, currentPosition.y - point.point.y, hittingBase);

                    //If moving downwards and no real movement
                    if (hittingBase)
                    {
                        //Not able to go down further, test if we are standing on ground
                        const float upOffset = 0.5f;
                        const float minGroundDistance = 0.2f;
                        Vector3 origin = currentPosition + Vector3.up * upOffset + currentNormal * c.collider.radius;
                        //Debug.DrawLine(origin, origin + Vector3.down * (upOffset + minGroundDistance), Color.blue, Time.deltaTime);
                        if (Physics.Raycast(origin, Vector3.down, upOffset + minGroundDistance, c.m_groundLayerMask, QueryTriggerInteraction.Ignore))
                        {
                            //What we hit is not important as we know it is in range and on ground layer
                            c.ChangeToState<Walking>();
                        }
                    }

                    currentPosition = point.point;
                    currentNormal = point.normal;


                    //Head position should be above center point, distance of collider away
                    Quaternion rotation = Quaternion.LookRotation(currentNormal);

                    //Test if were the head of the player is
                    var headPoint = ladder.GetClosestPointOnMesh(currentPosition + upTangent * c.collider.height);


                    var headPos = headPoint.point;
                    Vector3 localHeadPos = rotation * (headPos - currentPosition);

                    Vector3 flatHeadNormal = headPoint.normal;
                    flatHeadNormal.y = 0;

                    Vector3 flatBodyNormal = point.normal;
                    flatBodyNormal.y = 0;

                    float bodyHeadRotationDifference = Vector3.SignedAngle(flatHeadNormal, flatBodyNormal, Vector3.up);
                    if (Mathf.Abs(bodyHeadRotationDifference) > c.maxHeadBodyRotationDifference)
                    {
                        //go back left / right
                        currentPosition.x -= deltaPos.x;
                        currentPosition.z -= deltaPos.z;
                    }

                    float sqrHeadDistance = localHeadPos.y * localHeadPos.y + localHeadPos.z * localHeadPos.z;

                    bool dirtyPosition = false;
                    if (sqrHeadDistance < c.collider.height * c.collider.height)
                    {
                        float headDistance = Mathf.Sqrt(sqrHeadDistance);
                        //print("Local head pos : {0} distance: {1}", localHeadPos.ToString("F3"), headDistance);
                        //Cannot go this way, go back down or jump up to top surface

                        Vector3 origin = currentPosition + Vector3.up * (oldColliderHeight + c.collider.height) - currentNormal * c.collider.radius * 2;

                        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, oldColliderHeight * 1.05f, c.m_groundLayerMask, QueryTriggerInteraction.Ignore) &&
                        hit.distance > oldColliderHeight * 0.95f && Vector3.Dot(Vector3.up, hit.normal) > c.m_maxGroundDot)
                        {
                            //go to tops

                            c.ChangeToState<TransitionState<Walking>>(c.transitionTime);

                        }
                        else
                        {
                            //Go back down
                            var offset = upTangent * (c.collider.height - headDistance);

                            currentPosition -= offset;

                            dirtyPosition = true;
                        }
                    }

                    //measure from current point so we dont double- apply the limits
                    if (Mathf.Abs(localHeadPos.x) > 0.01f)
                    {
                        //Go back on the player's x axis
                        Vector3 diff = Quaternion.LookRotation(leftTangent) * Vector3.forward * -localHeadPos.x;
                        currentPosition += diff;
                        dirtyPosition = true;
                    }

                    if (dirtyPosition)
                    {
                        //refind the point for updated normal
                        point = ladder.GetClosestPointOnMesh(currentPosition);
                        currentPosition = point.point;
                        currentNormal = point.normal;
                    }

                    MovePlayerToMesh(currentPosition, currentNormal);

                    break;
            }
        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.vertical.id, c.input.horizontal.y * c.climbingSpeed);
            animator.SetFloat(vars.horizontal.id, 0f);
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {

        }
#endif

    }

}