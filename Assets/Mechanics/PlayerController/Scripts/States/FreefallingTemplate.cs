using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Freefalling")]
	public class FreefallingTemplate : MovementStateTemplate
	{
		public MovementStateTemplate airInteract;
		public override MovementState StartState(PlayerController c)
		{
			return new Freefalling(c, this);
		}
	}
}