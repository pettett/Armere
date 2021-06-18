using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{


	public class KnockedOut : MovementState<KnockedOutTemplate>
	{
		public readonly float knockoutTime = 4f;

		public KnockedOut(PlayerController c, KnockedOutTemplate t) : base(c, t)
		{
			this.knockoutTime = t.time;
		}

		public override string StateName => "Knocked Out";

		public override void Start()
		{

			canBeTargeted = false;

			c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = true;
			GameCameras.s.playerTrackingOffset = 0;
			c.StartCoroutine(WaitForRespawn());
		}

		public IEnumerator WaitForRespawn()
		{
			yield return new WaitForSeconds(knockoutTime);
			WakeUp();
		}

		public void WakeUp()
		{
			//transform.position = LevelController.respawnPoint.position;
			//transform.rotation = LevelController.respawnPoint.rotation;
			c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = false;



			//go back to the spawn point
			c.ChangeToState(t.returnState);
		}

		//place this in end to make sure it always returns camera control even if state is externally changed
		public override void End()
		{
			GameCameras.s.playerTrackingOffset = c.profile.m_standingHeight;
		}
	}
}