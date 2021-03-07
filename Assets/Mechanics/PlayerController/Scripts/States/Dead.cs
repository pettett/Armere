using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{


	public class Dead : MovementState<DeadTemplate>
	{
		public Dead(PlayerController c, DeadTemplate t) : base(c, t)
		{
		}

		public override string StateName => "Dead";
		public override void Start()
		{
			canBeTargeted = false;
			GameCameras.s.EnableControl();
			c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = true;
		}
	}
}