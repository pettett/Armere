using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.PlayerController
{
	[RequireComponent(typeof(AudioSource))]

	public class CharacterAudioController : MonoBehaviour
	{
		AudioSource source;
		PlayerWaterObject c;

		private void Start()
		{
			source = GetComponent<AudioSource>();
			c = GetComponent<PlayerWaterObject>();

			GetComponentInChildren<AnimationController>().onFootDown += FootDown;
		}
		public void FootDown(int foot)
		{



			if (c != null && c.currentFluid != null)
			{
				//Make foot splash
				c.currentFluid.CreateSplash(c.currentFluid.GetSurfacePosition(transform.position), 0.5f);
			}
		}

	}
}