using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
namespace Armere.PlayerController
{

	/// <summary> Transition to another state
	public class TransitionState : MovementState<TransitionStateTemplate>
	{
		public TransitionState(PlayerController c, TransitionStateTemplate t) : base(c, t)
		{
			c.StartCoroutine(MoveToNext(t.time));
			c.rb.velocity = Vector3.zero;
		}

		public override string StateName => "Transitioning";

		IEnumerator MoveToNext(float time)
		{
			yield return new WaitForSeconds(time);
			c.ChangeToState(t.nextState);
		}
	}
}