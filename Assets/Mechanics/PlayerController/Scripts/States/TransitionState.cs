using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
namespace Armere.PlayerController
{

	/// <summary> Transition to another state
	public class TransitionState : MovementState
	{
		readonly MovementStateTemplate next;
		public TransitionState(PlayerMachine machine, TransitionStateTemplate t) : base(machine)
		{
			next = t.nextState;
			c.StartCoroutine(MoveToNext(t.time));
			c.rb.velocity = Vector3.zero;
		}

		public override string StateName => "Transitioning";

		public override char stateSymbol => 'T';

		IEnumerator MoveToNext(float time)
		{
			yield return new WaitForSeconds(time);
			machine.ChangeToState(next);
		}
	}
}