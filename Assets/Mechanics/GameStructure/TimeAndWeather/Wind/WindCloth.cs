using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindCloth : MonoBehaviour
{
	public Cloth cloth;
	public GlobalVector3SO windDirectionGlobal;
	private void Update()
	{
		cloth.externalAcceleration = windDirectionGlobal.value;
	}

}
