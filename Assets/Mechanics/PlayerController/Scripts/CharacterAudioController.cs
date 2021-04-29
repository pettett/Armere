using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.PlayerController
{
	[RequireComponent(typeof(AudioSource))]

	public class CharacterAudioController : MonoBehaviour
	{
		AudioSource source;
		PlayerController c;

		private void Start()
		{
			source = GetComponent<AudioSource>();
			c = GetComponent<PlayerController>();

			GetComponentInChildren<AnimationController>().onFootDown += FootDown;
		}
		public void FootDown(int foot)
		{



			if (c != null && c.currentWater != null)
			{
				//Make foot splash
				c.currentWater.CreateSplash(c.currentWater.GetSurfacePosition(transform.position), 0.5f);
			}
		}

	}
}