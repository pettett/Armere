using UnityEngine;

namespace Armere.PlayerController
{

	public class ToggleMenusTemplate : MovementStateTemplate
	{
		[Header("Channels")]
		public BoolEventChannelSO setTabMenuEventChannel;
		public IntEventChannelSO setTabMenuPanelEventChannel;
		public override MovementState StartState(PlayerMachine c)
		{
			return new ToggleMenus(c, this);
		}
	}
}