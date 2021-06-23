using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "Game/Characters/Character Profile")]
public class CharacterProfile : ScriptableObject
{
	[Header("Movement")]

	[Range(0, 1)]
	public float dynamicFriction = 0.2f;
	public float m_minKnockoutForce = 100;
	public AnimationCurve m_knockoutTimePerMassPerSpeed = AnimationCurve.EaseInOut(0, 0, 10000, 30);
	public float m_standingHeight = 1.8f;


	[Header("Ground detection")]
	[Range(0, 90)] public float m_maxGroundSlopeAngle = 70;
	[ReadOnly] public float maxGroundDot = 0.5f;
	public float m_maxGroundSlopeDot
	{
		get
		{
			// if (float.IsNaN(maxGroundDot))
			// 	maxGroundDot = Mathf.Cos(m_maxGroundSlopeAngle * Mathf.Deg2Rad);
			return maxGroundDot;
		}
	}
	private void OnValidate()
	{
		maxGroundDot = Mathf.Cos(m_maxGroundSlopeAngle * Mathf.Deg2Rad);
	}

	public bool CanWalkOn(Vector3 normal, Vector3 up)
	{
		//Debug.Log($"{Vector3.Dot(normal, up)},{ m_maxGroundSlopeDot}");
		return Vector3.Dot(normal, up) > m_maxGroundSlopeDot;
	}

	[Header("Water")]
	public float maxWaterStrideDepth = 1;
}
