using UnityEngine;
using UnityEngine.InputSystem;
namespace PlayerController
{

    /// <summary> Same as walking except uses a navmeshagent to control the player
    [System.Serializable]
    public class LadderClimb : MovementState
    {
        public override string StateName => "Auto-Walking";
        Ladder ladder;
        public override void Start(params object[] info)
        {
            ladder = info[0] as Ladder;
        }
        public override void End()
        {

        }
        public override void Update()
        {
            transform.position = ladder.transform.position + Vector3.up * ladder.ladderHeight * 0.5f;
        }

        public override void Animate(AnimatorVariables vars)
        {
            //animator.SetFloat(vars.vertical.id, agent.velocity.magnitude);
            //animator.SetFloat(vars.horizontal.id, 0f);
        }
    }

}