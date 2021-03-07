using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Dead")]
	public class DeadTemplate : MovementStateTemplate
	{
		public override MovementState StartState(PlayerController c)
		{
			return new Dead(c, this);
		}
	}
}