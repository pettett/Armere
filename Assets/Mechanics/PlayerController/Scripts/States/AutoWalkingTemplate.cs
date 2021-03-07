using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Auto Walking")]
	public class AutoWalkingTemplate : MovementStateTemplate
	{
		public override MovementState StartState(PlayerController c)
		{
			return new AutoWalking(c, this);
		}
	}
}