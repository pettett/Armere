using System.Collections.Generic;
using UnityEngine;


public abstract class Vision : MonoBehaviour
{
	[MyBox.MinMaxRange(0, 100)] public MyBox.RangedFloat clippingPlanes = new MyBox.RangedFloat(0.1f, 10f);


	public abstract float FOV { get; }

	public abstract float Aspect { get; }
	public abstract Vector3 ViewPoint { get; }

	public abstract float ProportionBoundsVisible(Bounds b);


}