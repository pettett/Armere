

using Armere.Inventory;
using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Walking")]
	public class WalkingTemplate : MovementStateTemplate
	{
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

		[Header("Weapons")]
		public float swordUseDelay = 0.4f;
		public Vector2 arrowSpeedRange = new Vector2(70, 100);
		[Header("Camera")]

		public float shoulderViewXOffset = 0.6f;
		[Header("Water")]

		[Range(0, 1), Tooltip("0 means no movement in water, 1 means full speed at full depth")]
		public float maxStridingDepthSpeedScalar = 0.6f;

		public MovementStateTemplate swimming;

		[Header("Channels")]
		public ItemAddedEventChannelSO onPlayerInventoryItemAdded;
		public VoidEventChannelSO onAimModeEnable;
		public VoidEventChannelSO onAimModeDisable;
		public FloatEventChannelSO changeTimeEventChannel;
		public override MovementState StartState(PlayerController c)
		{
			return new Walking(c, this);
		}
	}
}