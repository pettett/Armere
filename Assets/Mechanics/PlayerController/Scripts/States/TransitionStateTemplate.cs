using UnityEngine;

namespace Armere.PlayerController
{
	public class TransitionStateTemplate : MovementStateTemplate
	{
		static TransitionStateTemplate transition;
		public static TransitionStateTemplate GenerateTransition(float time, MovementStateTemplate next)
		{
			if (transition == null)
			{
				transition = CreateInstance<TransitionStateTemplate>();
				transition.parallelStates = new MovementStateTemplate[1] { CreateInstance<ToggleMenusTemplate>() };
			}

			transition.time = time;
			transition.nextState = next;
			return transition;
		}

		public MovementStateTemplate nextState;
		public float time = 5;
		public override MovementState StartState(PlayerMachine c)
		{
			return new TransitionState(c, this);
		}
	}
}