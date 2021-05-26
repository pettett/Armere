using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.PlayerController
{
	public class GravityTrigger : PlayerTrigger
	{
		public Vector3 gravityDirection = Vector3.down;
		public override void OnPlayerTrigger(PlayerController player)
		{
			player.SetGravityDirection(gravityDirection);
		}
	}
}
