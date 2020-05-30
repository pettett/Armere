using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
namespace PlayerController
{

    /// <summary> Same as walking except uses a navmeshagent to control the player
    [System.Serializable]
    public class AutoWalking : MovementState
    {
        public override string StateName => "Auto-Walking";
        NavMeshAgent agent;
        public override void Start()
        {
            agent = c.gameObject.AddComponent<NavMeshAgent>();
            agent.radius = c.collider.radius;
            agent.height = c.collider.height;
            agent.stoppingDistance = 0.1f;
            c.collider.material.dynamicFriction = 1;
        }
        public override void End()
        {
            MonoBehaviour.Destroy(agent);
        }

        public void WalkTo(Vector3 position)
        {
            agent.SetDestination(position);
        }
        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.vertical.id, agent.velocity.magnitude);
            animator.SetFloat(vars.horizontal.id, 0f);
        }
    }

}