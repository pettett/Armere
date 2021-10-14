using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.PlayerController
{

	public class CharacterAudioController : MonoBehaviour
	{
		PlayerWaterObject c;

		private void Start()
		{
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