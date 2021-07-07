using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Freefalling")]
	public class FreefallingTemplate : MovementStateTemplate
	{
		public float coyoteTime = 0.05f;

		public MovementStateTemplate airInteract;

		public override MovementState StartState(PlayerMachine c)
		{
			return new Freefalling(c, this);
		}
	}
}