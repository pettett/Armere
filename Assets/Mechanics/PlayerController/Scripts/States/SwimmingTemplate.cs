using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Swimming")]
	public class SwimmingTemplate : MovementStateTemplate
	{
		public float colliderHeight = 1.6f;
		//public float pelvisOffset = -0.2f;
		public float waterDrag = 1;
		public float waterMovementForce = 1;

		public float waterMovementSpeed = 1;
		public float waterSittingDepth = 1;

		public GameObject waterTrailPrefab;

		public override MovementState StartState(PlayerController c)
		{
			return new Swimming(c, this);
		}
	}
}