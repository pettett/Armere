using UnityEngine;

namespace Armere.PlayerController
{
	public class TransitionStateTemplate : MovementStateTemplate
	{
		public MovementStateTemplate nextState;
		public float time = 5;
		public static TransitionStateTemplate GenerateTransition(float time, MovementStateTemplate next)
		{
			var tr = CreateInstance<TransitionStateTemplate>();
			tr.time = 0.2f;
			tr.nextState = next;
			tr.parallelStates = new MovementStateTemplate[1] { CreateInstance<ToggleMenusTemplate>() };
			return tr;
		}
		public override MovementState StartState(PlayerController c)
		{
			return new TransitionState(c, this);
		}
	}
}