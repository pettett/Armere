using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BarrelHoldRigController : MonoBehaviour
{
	public Rig barrelHoldRig;
	public Transform barrelHolder;
	public Vector3 barrelOffset;
	public Quaternion barrelRotation;
	public Quaternion holderRotation => barrelHolder.rotation;
	public Vector3 holderPosition => barrelHolder.TransformPoint(barrelOffset);
	public void AttachBarrel(GameObject barrel)
	{
		barrelHoldRig.weight = 1;
		barrel.transform.SetParent(barrelHolder, false);
		barrel.transform.localPosition = barrelOffset;
		barrel.transform.localRotation = barrelRotation;

	}
	public void SetWeight(float weight)
	{
		barrelHoldRig.weight = weight;
	}
	public void DetachBarrel()
	{
		barrelHoldRig.weight = 0;
	}
}
