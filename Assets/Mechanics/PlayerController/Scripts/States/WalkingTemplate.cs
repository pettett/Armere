

using Armere.Inventory;
using UnityEngine;

namespace Armere.PlayerController
{
	[System.Serializable]
	public struct Speeds
	{

		public float movementSpeed;
		public float verticalJumpHeight;
		public float horizontalJumpDistance;

		public Vector2 twoDJumpForce
		{
			get
			{
				float grav = Physics.gravity.y;
				float invTime = 1 / Mathf.Sqrt(Mathf.Abs(2 * verticalJumpHeight / grav));

				float vertical = 2 * verticalJumpHeight * invTime;
				float horizontal = horizontalJumpDistance * invTime;
				return new Vector2(horizontal, vertical);
			}
		}
	}

	[CreateAssetMenu(menuName = "Game/PlayerController/Walking")]
	public class WalkingTemplate : MovementStateTemplate
	{
		public MovementStateTemplate freefalling;

		[Header("Movement")]
		public Speeds crouching;
		public Speeds walking;
		public Speeds sprinting;

		public ref Speeds GetSpeeds(Walking.WalkingType type)
		{
			switch (type)
			{
				case Walking.WalkingType.Walking:
					return ref walking;
				case Walking.WalkingType.Crouching:
					return ref crouching;
				case Walking.WalkingType.Sprinting:
					return ref sprinting;
				default:
					throw new System.ArgumentException("Type not known");
			}
		}


		public float crouchingHeight = 0.9f;
		public float groundClamp = 1f;
		public float maxAcceleration = 20f;
		public float maxStepHeight = 1f;
		public float maxStepDown = 0.25f;
		public float stepSearchOvershoot = 0.3f;
		public float steppingTime = 0.1f;
		public float jumpForce = 4f;
		public float coyoteTime = 0.05f;
		[Header("Holding")]

		public float throwForce = 100;

		[Header("Weapons")]
		public float swordUseDelay = 0.4f;
		public Vector2 arrowSpeedRange = new Vector2(70, 100);
		public BoundsFloatEventChannelSO destroyGrassInBoundsEventChannel;
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