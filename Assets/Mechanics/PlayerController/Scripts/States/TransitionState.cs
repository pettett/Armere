using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
namespace Armere.PlayerController
{

    /// <summary> Transition to another state
    [System.Serializable]
    [RequiresParallelState(typeof(ToggleMenus))]
    public class TransitionState<NextState> : MovementState where NextState : MovementState, new()
    {
        public override string StateName => "Transitioning";
        public override void Start(params object[] args)
        {
            c.StartCoroutine(MoveToNext((float)args[0]));
            c.rb.velocity = Vector3.zero;
        }
        IEnumerator MoveToNext(float time)
        {
            yield return new WaitForSeconds(time);
            c.ChangeToState(typeof(NextState));
        }
    }
}