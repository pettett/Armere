using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Dead")]
	public class DeadTemplate : MovementStateTemplate
	{
		public override MovementState StartState(PlayerMachine c)
		{
			return new Dead(c, this);
		}
	}
}