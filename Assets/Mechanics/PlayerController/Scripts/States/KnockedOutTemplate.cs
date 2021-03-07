using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Knocked out")]
	public class KnockedOutTemplate : MovementStateTemplate
	{
		public MovementStateTemplate returnState;
		[System.NonSerialized]
		public float time;
		public override MovementState StartState(PlayerController c)
		{
			return new KnockedOut(c, this);
		}
	}
}