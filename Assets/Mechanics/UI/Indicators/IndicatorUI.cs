using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IndicatorUI : MonoBehaviour
{
	protected Transform target = null;
	protected Vector3 worldOffset;

	public void Init(Transform target, Vector3 worldOffset = default)
	{
		Assert.IsNotNull(target, "No target");
		if (target == null) enabled = false;
		this.target = target;
		this.worldOffset = worldOffset;
	}


	// Update is called once per frame
	void LateUpdate()
	{
		//position self on target
		Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + worldOffset);
		//Sort by distance
		screenPos.z = Vector3.SqrMagnitude(target.position + worldOffset - Camera.main.transform.position);

		transform.position = screenPos;
	}
}
