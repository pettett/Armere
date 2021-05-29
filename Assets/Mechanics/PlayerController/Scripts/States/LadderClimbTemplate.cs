using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Ladder Climb")]
	public class LadderClimbTemplate : MovementStateTemplate
	{
		[System.NonSerialized] public Climbable climbable;
		[Header("Climbing")]
		public float climbingColliderHeight = 1.6f;
		public float climbingSpeed = 4f;
		public float meshAnimationSpeed = 40f;
		public float transitionTime = 4f;
		[Range(0, 180)] public float maxHeadBodyRotationDifference = 5f;
		public override MovementState StartState(PlayerController c)
		{
			return new LadderClimb(c, this);
		}

		public LadderClimbTemplate Interact(Climbable c)
		{
			climbable = c;
			return this;
		}
	}
}