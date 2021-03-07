using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Swimming")]
	public class SwimmingTemplate : MovementStateTemplate
	{
		public override MovementState StartState(PlayerController c)
		{
			return new Swimming(c, this);
		}
	}
}