

using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Walking")]
	public class WalkingTemplate : MovementStateTemplate
	{
		public MovementStateTemplate swimming;
		public MovementStateTemplate freefalling;

		[Header("Movement")]
		public float walkingSpeed = 2f;
		public float sprintingSpeed = 5f;
		public float crouchingSpeed = 1f;
		public float crouchingHeight = 0.9f;
		public float groundClamp = 1f;
		public float maxAcceleration = 20f;
		public float maxStepHeight = 1f;
		public float maxStepDown = 0.25f;
		public float stepSearchOvershoot = 0.3f;
		public float steppingTime = 0.1f;
		public float jumpForce = 4f;
		[Header("Holding")]

		public float throwForce = 100;

		public override MovementState StartState(PlayerController c)
		{
			return new Walking(c, this);
		}
	}
}