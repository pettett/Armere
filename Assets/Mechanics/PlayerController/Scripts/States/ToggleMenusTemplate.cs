using UnityEngine;

namespace Armere.PlayerController
{

	public class ToggleMenusTemplate : MovementStateTemplate
	{
		public override MovementState StartState(PlayerController c)
		{
			return new ToggleMenus(c, this);
		}
	}
}