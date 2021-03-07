using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Shield Surfing")]
	public class ShieldSurfingTemplate : MovementStateTemplate
	{

		[Header("Shield Surfing")]
		public float turningTorqueForce;
		public float minSurfingSpeed;
		public float turningAngle;

		public float friction = 0.05f;
		public float jumpForce = 4f;
		public override MovementState StartState(PlayerController c)
		{
			return new ShieldSurfing(c, this);
		}
	}
}