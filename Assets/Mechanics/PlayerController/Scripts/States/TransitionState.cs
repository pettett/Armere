using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
namespace PlayerController
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
        }
        IEnumerator MoveToNext(float time)
        {
            yield return new WaitForSeconds(time);
            c.ChangeToState(typeof(NextState));
        }
    }
}