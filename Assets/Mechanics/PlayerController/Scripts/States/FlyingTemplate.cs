using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Flying")]
	public class FlyingTemplate : MovementStateTemplate
	{
		public override MovementState StartState(PlayerController c)
		{
			return new Flying(c, this);
		}
	}
	public class Flying : MovementState<FlyingTemplate>
	{
		public Flying(PlayerController c, FlyingTemplate t) : base(c, t)
		{
			//Fly
			c.rb.useGravity = false;
		}
		public override void FixedUpdate()
		{
			c.rb.velocity = PlayerInputUtility.WorldSpaceFullInput(c);
		}
		public override void End()
		{
			c.rb.useGravity = true;
		}

		public override string StateName => "Flying";
	}
}