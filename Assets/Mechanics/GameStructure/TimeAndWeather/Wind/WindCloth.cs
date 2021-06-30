using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class WindCloth : MonoBehaviour
{
	public Cloth cloth;
	public GlobalVector3SO windDirectionGlobal;

	public float multiplier = 1f;

	private void Update()
	{
		Assert.IsNotNull(windDirectionGlobal, "Vector required");
		cloth.externalAcceleration = windDirectionGlobal.value * multiplier;
	}

}
