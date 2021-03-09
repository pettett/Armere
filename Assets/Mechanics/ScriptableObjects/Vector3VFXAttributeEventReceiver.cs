using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("Vector3 Event")]
public class Vector3VFXAttributeEventReceiver : VFXBinderBase
{
	[VFXPropertyBinding("Vector3")]
	public ExposedProperty vector3Property;
	public GlobalVector3SO vector3Global;
	public float multiplier = 1f;


	public override bool IsValid(VisualEffect component)
	{
		return vector3Global != null && component.HasVector3(vector3Property);
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetVector3(vector3Property, vector3Global.value * multiplier);
	}
}
