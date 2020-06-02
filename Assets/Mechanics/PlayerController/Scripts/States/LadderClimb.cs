using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
namespace PlayerController
{

    /// <summary> Same as walking except uses a navmeshagent to control the player
    [System.Serializable]
    [RequiresParallelState(typeof(ToggleMenus))]
    public class LadderClimb : MovementState
    {
        public override string StateName => "Climbing Ladder";
        Ladder ladder;
        float height;
        public float climbingSpeed = 2.5f;
        public override void Start(params object[] info)
        {
            if (info.Length > 0 && info[0] is Ladder)
                ladder = info[0] as Ladder;
            else
                throw new System.ArgumentException("A Ladder object must be supplied to this state");
            //calculate height the player is at if they come at the ladder from above
            height = Mathf.Clamp(ladder.transform.InverseTransformPoint(transform.position).y, 0, ladder.ladderHeight - 0.1f);

            transform.SetPositionAndRotation(GetLadderPos(height), ladder.transform.rotation);
            c.rb.isKinematic = true;
            c.animationController.enableFeetIK = false;
        }

        public Vector3 GetLadderPos(float h)
        {
            return ladder.LadderPosAtHeight(h, c.collider.radius * 2f);
        }

        public override void End()
        {
            //Dont do if manually moved
            c.rb.isKinematic = false;
            c.animationController.enableFeetIK = true;
        }

        public int RoundToNearest(float value, int interval, int offset) => Mathf.RoundToInt((value - offset) / interval) * interval + offset;

        float lhRung, rhRung, lfRung, rfRung = 0;

        void UpdateRung(ref float rung, float height, int offset)
        {
            rung = Mathf.Lerp(rung, RoundToNearest(height, 2, offset), Time.deltaTime * 25);//Closest even rung
        }

        public override void OnAnimatorIK(int layerIndex)
        {
            float rung = (height + 1.4f - ladder.rungOffset) / ladder.rungDistance;
            //Update the position of every body part
            UpdateRung(ref lhRung, rung, 0);
            UpdateRung(ref rhRung, rung, 1);
            //The height of feet is lower then the hands ; target a different position
            rung = (height + 0.4f - ladder.rungOffset) / ladder.rungDistance;
            UpdateRung(ref lfRung, rung, 0);
            UpdateRung(ref rfRung, rung, 1);

            SetPosition(AvatarIKGoal.LeftHand, ladder.LadderPosByRung(lhRung, -0.1f));
            SetPosition(AvatarIKGoal.RightHand, ladder.LadderPosByRung(rhRung, 0.1f));

            //Do the same for feet

            SetPosition(AvatarIKGoal.LeftFoot, ladder.LadderPosByRung(lfRung, -0.1f));
            SetPosition(AvatarIKGoal.RightFoot, ladder.LadderPosByRung(rfRung, 0.1f));
        }
        void SetPosition(AvatarIKGoal goal, Vector3 pos)
        {
            c.animator.SetIKPositionWeight(goal, 1);
            c.animator.SetIKPosition(goal, pos);
        }
        public override void OnJump(float state)
        {
            if (state == 1)
            {
                //Jump off the ladder
                c.rb.isKinematic = false;
                c.rb.AddForce(ladder.transform.forward * -100);
                c.ChangeToState<Freefalling>();
            }
        }
        public override void Update()
        {
            height += c.input.inputWalk.y * Time.deltaTime * climbingSpeed;
            height = Mathf.Clamp(height, 0, ladder.ladderHeight);

            transform.position = GetLadderPos(height);
            if (height == ladder.ladderHeight)
            {
                //Get off at the top of the ladder
                transform.position = GetLadderPos(height) + ladder.transform.forward * 1f;
                c.ChangeToState<Walking>();
            }
        }

        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.vertical.id, 0);
            animator.SetFloat(vars.horizontal.id, 0f);
        }
    }

}