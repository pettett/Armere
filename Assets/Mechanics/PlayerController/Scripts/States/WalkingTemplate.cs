

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

		public Speeds(float movementSpeed, float verticalJumpHeight, float horizontalJumpDistance)
		{
			this.movementSpeed = movementSpeed;
			this.verticalJumpHeight = verticalJumpHeight;
			this.horizontalJumpDistance = horizontalJumpDistance;
		}

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

	[System.Serializable]
	public struct Propusion
	{
		public float maxAcceleration;
		public float rotationSpeed;
	}


	[CreateAssetMenu(menuName = "Game/PlayerController/Walking")]
	public class WalkingTemplate : MovementStateTemplate
	{
		//public MovementStateTemplate freefalling;

		[Header("Movement")]
		public Speeds crouching = new Speeds(1f, 0.25f, 0.5f);
		public Speeds walking = new Speeds(1f, 0.75f, 1f);
		public Speeds sprinting = new Speeds(1.5f, 0.5f, 1.5f);

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

		public Propusion groundPropulsion;
		public Propusion airPropulsion;
		public ref Propusion GetPropulsion(Walking.GroundState ground)
		{
			if (ground.HasFlag(Walking.GroundState.Grounded)) return ref groundPropulsion;
			return ref airPropulsion;
		}

		public float crouchingHeight = 0.9f;
		public float groundClamp = 1f;
		public float minStepHeight = 0.1f;
		public float maxStepHeight = 1f;
		public float maxStepDown = 0.25f;
		public float stepSearchOvershoot = 0.3f;
		public float steppingSpeed = 0.1f;
		public float slopeForce = 1f;
		public float coyoteTime = 0.05f;
		[Header("Holding")]

		public float throwForce = 100;
		[Header("Pushing")]
		public float pushSpeed = 1f;
		public float minPushMass = 10f;
		public float maxPushMass = 100f;

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
		public override MovementState StartState(PlayerMachine c)
		{
			return new Walking(c, this);
		}
	}
}