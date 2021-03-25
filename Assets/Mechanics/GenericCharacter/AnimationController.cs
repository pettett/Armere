using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public interface IAnimatable
{
	Vector3 velocity { get; }
	float maxSpeed { get; }
	Transform transform { get; }
}
readonly struct StringHashes
{
	public readonly int VelocityX;
	public readonly int VelocityZ;

	public StringHashes(string VelocityX, string VelocityZ)
	{
		this.VelocityX = Animator.StringToHash(VelocityX);
		this.VelocityZ = Animator.StringToHash(VelocityZ);
	}
}
[System.Flags]
public enum Layers
{
	None = 0,
	Everything = BaseLayer | UpperBody,
	BaseLayer = 1,
	UpperBody = 2,
}

[System.Serializable]
public struct AnimationTransition
{
	int? _nameHash;
	public int nameHash
	{
		get
		{
			if (!_nameHash.HasValue)
				_nameHash = Animator.StringToHash(_name);
			return _nameHash.Value;
		}
	}

	public string _name;
	public float duration;
	public float offset;
	public Layers layers;

	public AnimationTransition(string name, float duration, float offset, Layers layers)
	{
		_nameHash = null;
		_name = name;
		this.duration = duration;
		this.offset = offset;
		this.layers = layers;
	}
}

public class AnimationController : MonoBehaviour
{
	#region Private Variables
	private Vector3 rightFootPosition, leftFootPosition, leftFootIKPosition, rightFootIKPosition = default(Vector3);
	private Quaternion rightFootIKRotation, leftFootIKRotation;
	private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY = default(float);
	#endregion

	#region Public Variables
	[Header("Feet Grounder")]
	public bool enableFeetIK = true;
	[Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;

	[Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float pelvisOffset = 0f;
	[SerializeField] private float crouchOffset = 0f;
	[Range(0, 1)] [SerializeField] private float pelvisVerticalSpeed = 0.28f, feetToIKPositionSpeed = 0.5f;
	public string leftFootAnimVariableName = "LeftFootCurve";
	public string rightFootAnimVariableName = "RightFootCurve";

	public bool useFootCurvesForRotationGrounding;
	public bool showSolver;

	[Header("AC")]

	public bool useIK;


	[System.Serializable]
	public struct HoldPoint
	{
		public AvatarIKGoal goal;
		public Transform gripPoint;
		[Range(0, 1)]
		public float positionWeight;
		[Range(0, 1)]
		public float rotationWeight;
	}

	[System.Serializable]
	public struct LookAtWeights
	{
		[Range(0, 1)] public float weight, bodyWeight, headWeight, eyesWeight, clampWeight;
	}




	public List<HoldPoint> holdPoints;


	Animator anim;

	[System.NonSerialized] public LookAtWeights weights;
	public LookAtWeights lookAtTargetWeights;

	[System.NonSerialized] public Transform lookAtTarget;
	[System.NonSerialized] public Vector3 lookAtPosition;

	public bool useAnimationHook = false;

	public float velocityScaler = 1;
	IAnimatable animationHook;
	StringHashes hashes;
	Transform head;
	Rigidbody rb;
	#endregion
	public bool lookToVelocity;
	public float maxTurningAngle = 1;
	public float turningMultiplier = 3;
	public float turningTime = 0.5f;
	float turingVel = 0;
	float averageTurning = 0;

	Vector3 lastDirection;
	void TriggerTransition(in AnimationTransition transition, int layer)
	{
		anim.CrossFadeInFixedTime(transition.nameHash, transition.duration, layer, transition.offset);
	}

	public void TriggerTransition(in AnimationTransition transition)
	{
		if (transition.layers.HasFlag(Layers.BaseLayer))
			TriggerTransition(transition, 0);
		if (transition.layers.HasFlag(Layers.UpperBody))
			TriggerTransition(transition, 1);
	}

	void Start()
	{

		anim = GetComponent<Animator>();

		head = anim.GetBoneTransform(HumanBodyBones.Head);
		rb = GetComponent<Rigidbody>();

		if (useAnimationHook)
		{
			animationHook = GetComponent<IAnimatable>();
			hashes = new StringHashes("VelocityX", "VelocityZ");
		}
	}
	private void FixedUpdate()
	{
		if (useAnimationHook)
		{
			Vector3 localVelocity = animationHook.transform.InverseTransformDirection(animationHook.velocity / animationHook.maxSpeed);
			anim.SetFloat(hashes.VelocityX, localVelocity.x * velocityScaler);
			anim.SetFloat(hashes.VelocityZ, localVelocity.z * velocityScaler);
		}
		if (enableFeetIK && anim != null)
		{
			AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
			AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

			//find and raycast to the ground to find positions
			FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation); // find ground under right foot
			FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation); // find ground under right foot
		}
	}
	private void Update()
	{
		if (lookToVelocity)
		{
			//record current rotation for graphics controller

			averageTurning = Mathf.SmoothDamp(
				averageTurning,
				Vector3.SignedAngle(transform.forward, lastDirection, Vector3.up) / Time.deltaTime,
				ref turingVel, turningTime);

			averageTurning = Mathf.Clamp(averageTurning, -maxTurningAngle, maxTurningAngle);

			lastDirection = transform.forward;
		}
	}
	void OnAnimatorIK(int layerIndex)
	{
		if (anim == null) return;

		if (enableFeetIK)
		{
			MovePelvisHeight();
			//Right foot ik position and rotation

			anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
			if (useFootCurvesForRotationGrounding)            //pro features?
				anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(leftFootAnimVariableName));
			MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);

			anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
			if (useFootCurvesForRotationGrounding)            //pro features?
				anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
			MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
		}

		if (useIK)
		{
			foreach (HoldPoint point in holdPoints)
			{
				if (point.gripPoint == null)
					continue;

				anim.SetIKPositionWeight(point.goal, point.positionWeight);
				anim.SetIKRotationWeight(point.goal, point.rotationWeight);

				anim.SetIKPosition(point.goal, point.gripPoint.position);
				anim.SetIKRotation(point.goal, point.gripPoint.rotation);
			}

			if (lookAtTarget != null)
			{
				anim.SetLookAtWeight(lookAtTargetWeights.weight, lookAtTargetWeights.bodyWeight, lookAtTargetWeights.headWeight, lookAtTargetWeights.eyesWeight, lookAtTargetWeights.clampWeight);
				anim.SetLookAtPosition(lookAtTarget.position);
			}
			else if (lookToVelocity)
			{
				Quaternion rotation = Quaternion.Euler(0, -averageTurning * turningMultiplier, 0);
				anim.SetLookAtWeight(1, 0, 0.5f, 1, 0.15f);
				if (rb.velocity.sqrMagnitude > 0.1f)
				{
					anim.SetLookAtPosition(head.position + rotation * rb.velocity);
				}
				else
				{
					anim.SetLookAtPosition(head.position + rotation * transform.forward);
				}
			}
			else
			{

				anim.SetLookAtWeight(weights.weight, weights.bodyWeight, weights.headWeight, weights.eyesWeight, weights.clampWeight);
				anim.SetLookAtPosition(lookAtPosition);
			}
		}
	}



	#region Solver Methods

	void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
	{
		Vector3 targetIKPosition = anim.GetIKPosition(foot);
		if (positionIKHolder != default(Vector3))
		{
			targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
			positionIKHolder = transform.InverseTransformPoint(positionIKHolder);
			float y = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed);
			targetIKPosition.y += y;
			lastFootPositionY = y;
			targetIKPosition = transform.TransformPoint(targetIKPosition);
			anim.SetIKRotation(foot, rotationIKHolder);
		}
		anim.SetIKPosition(foot, targetIKPosition);
	}
	void MovePelvisHeight()
	{
		if (rightFootIKPosition == default(Vector3) || leftFootIKPosition == default(Vector3) || lastPelvisPositionY == default(float))
		{
			lastPelvisPositionY = anim.bodyPosition.y;
			return;
		}

		float lOffsetPosition = leftFootIKPosition.y - transform.position.y;
		float rOffsetPosition = rightFootIKPosition.y - transform.position.y;
		float totalOffset = Mathf.Min(lOffsetPosition, rOffsetPosition) - crouchOffset;

		Vector3 newPelvisPos = anim.bodyPosition + Vector3.up * totalOffset;
		newPelvisPos.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPos.y, pelvisVerticalSpeed);
		anim.bodyPosition = newPelvisPos;
		lastPelvisPositionY = newPelvisPos.y;
	}
	void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPosition, ref Quaternion feetIKRotation)
	{
		//Highwayyy tooooo theeeee raycastzone!
		//Locate this foot's position with raycast
		RaycastHit feetOutHit;
		if (showSolver)
			Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);

		if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, groundLayer, QueryTriggerInteraction.Ignore))
		{
			feetIKPosition = fromSkyPosition;
			feetIKPosition.y = feetOutHit.point.y + pelvisOffset;
			feetIKRotation = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
		}
		else
		{
			feetIKPosition = default(Vector3); //No Raycast
		}

	}
	void AdjustFeetTarget(ref Vector3 feetPosition, HumanBodyBones foot)
	{
		Transform f = anim.GetBoneTransform(foot);
		if (f != null)
		{
			feetPosition = f.position;
			feetPosition.y = transform.position.y + heightFromGroundRaycast;
		}
	}
	#endregion

}
