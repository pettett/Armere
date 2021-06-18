using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.PlayerController
{
	public class SlideAnchor : MonoBehaviour
	{
		public Transform leftFoot;
		public Transform rightFoot;
		public AnimationController ac;
		new public ParticleSystem particleSystem;
		private void OnEnable()
		{
			ac.holdPoints.Add(new AnimationController.HoldPoint()
			{
				goal = AvatarIKGoal.LeftFoot,
				gripPoint = leftFoot,
				positionWeight = 1,
				rotationWeight = 1
			});
			ac.holdPoints.Add(new AnimationController.HoldPoint()
			{
				goal = AvatarIKGoal.RightFoot,
				gripPoint = rightFoot,
				positionWeight = 1,
				rotationWeight = 1
			});
		}
		public bool particleEmission
		{
			set
			{
				var e = particleSystem.emission;
				e.enabled = value;
			}
		}
		private void OnDisable()
		{
			ac.holdPoints.Clear();
		}

	}
}