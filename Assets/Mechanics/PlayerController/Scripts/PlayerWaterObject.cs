using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Armere.PlayerController
{
	[RequireComponent(typeof(PlayerController))]
	public class PlayerWaterObject : MonoBehaviour, IWaterObject
	{
		public FluidTemplate[] deathFluids = new FluidTemplate[0];
		public WaterController currentFluid { get; private set; }

		public void OnWaterEnter(WaterController waterController)
		{
			if (deathFluids.Contains(waterController.template))
			{
				GetComponent<PlayerController>().TriggerFallingDeath();
			}
			else
			{
				currentFluid = waterController;
			}
		}

		public void OnWaterExit(WaterController waterController)
		{
			currentFluid = null;
		}
	}
}
