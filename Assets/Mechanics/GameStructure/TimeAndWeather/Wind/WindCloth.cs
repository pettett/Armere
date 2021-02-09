using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindCloth : MonoBehaviour
{
	public Cloth cloth;
	public Vector3EventChannelSO onWindDirectionChanged;
	private void OnEnable()
	{
		onWindDirectionChanged.OnEventRaised += UpdateCloth;
	}
	private void OnDisable()
	{
		onWindDirectionChanged.OnEventRaised -= UpdateCloth;
	}
	public void UpdateCloth(Vector3 wind)
	{
		cloth.externalAcceleration = wind;
	}
}
