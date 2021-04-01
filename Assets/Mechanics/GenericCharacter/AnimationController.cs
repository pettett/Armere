using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Animations.Rigging;

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
[Serializable]
public struct AnimatorVariable
{
	public string name;
	private int? _id;
	public int id
	{
		get
		{
			if (!_id.HasValue)
				_id = Animator.StringToHash(name);
			return _id.Value;
		}
	}
	public AnimatorVariable(string name)
	{
		this.name = name;
		_id = null;
	}
}

[RequireComponent(typeof(Animator))]
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
	public float pelvisOffset = 0f;
	[SerializeField] private float crouchOffset = 0f;
	[Range(0, 1)] [SerializeField] private float pelvisVerticalSpeed = 0.28f, feetToIKPositionSpeed = 0.5f;
	public string leftFootAnimVariableName = "LeftFootCurve";
	public string rightFootAnimVariableName = "RightFootCurve";

	public bool useFootCurvesForRotationGrounding;
	public bool showSolver;

	public Transform localHeadLookTarget;

	[Header("AC")]

	public bool useIK;

	public TwoBoneIKConstraint rightHandConstraint;
	public TwoBoneIKConstraint leftHandConstraint;
	public TwoBoneIKConstraint rightFootConstraint;
	public TwoBoneIKConstraint leftFootConstraint;
	public MultiAimConstraint headLook;


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




	public List<HoldPoint> holdPoints;


	Animator anim;

	[System.NonSerialized] public float headWeight;
	[Range(0, 1)] public float lookAtHeadWeight = 1;
	public Transform lookAtTarget { get; private set; }
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

	public void SetLookAtTarget(Transform target)
	{
		headLook.weight = lookAtHeadWeight;
		headLook.data.sourceObjects.Add(new WeightedTransform(target, 1));
		lookAtTarget = target.transform;
	}
	public void ClearLookAtTargets()
	{
		headLook.data.sourceObjects.Clear();
		SetLookAtTarget(localHeadLookTarget);
		lookAtTarget = null;
	}

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
		if (lookToVelocity && Time.deltaTime > 0)
		{
			//record current rotation for graphics controller
			float angle = Vector3.SignedAngle(transform.forward, lastDirection, Vector3.up) / Time.deltaTime;
			averageTurning = Mathf.SmoothDamp(
				averageTurning,
				angle,
				ref turingVel, turningTime);

			averageTurning = Mathf.Clamp(averageTurning, -maxTurningAngle, maxTurningAngle);

			lastDirection = transform.forward;
		}

		if (enableFeetIK)
		{
			//MovePelvisHeight();
			//Right foot ik position and rotation

			if (useFootCurvesForRotationGrounding)
				leftFootConstraint.weight = anim.GetFloat(leftFootAnimVariableName);
			else
				leftFootConstraint.weight = 1;

			MoveFeetToIKPoint(rightFootConstraint, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);

			if (useFootCurvesForRotationGrounding)
				leftFootConstraint.weight = anim.GetFloat(leftFootAnimVariableName);
			else
				leftFootConstraint.weight = 1;

			MoveFeetToIKPoint(leftFootConstraint, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
		}

		if (useIK)
		{
			// foreach (HoldPoint point in holdPoints)
			// {
			// 	if (point.gripPoint == null)
			// 		continue;

			// 	anim.SetIKPositionWeight(point.goal, point.positionWeight);
			// 	anim.SetIKRotationWeight(point.goal, point.rotationWeight);

			// 	anim.SetIKPosition(point.goal, point.gripPoint.position);
			// 	anim.SetIKRotation(point.goal, point.gripPoint.rotation);
			// }

			if (lookAtTarget == null && lookToVelocity)
			{

				Quaternion rotation = Quaternion.Euler(0, -averageTurning * turningMultiplier, 0);
				Vector3 lookAt;
				if (rb.velocity.sqrMagnitude > 0.1f)
				{
					lookAt = (head.position + rotation * rb.velocity);
				}
				else
				{
					lookAt = (head.position + rotation * transform.forward);
				}

				headLook.data.sourceObjects[0].transform.position = lookAt;
			}
		}
	}



	#region Solver Methods

	void MoveFeetToIKPoint(TwoBoneIKConstraint foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
	{
		Vector3 targetIKPosition = foot.data.target.position;
		if (positionIKHolder != default(Vector3))
		{
			targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
			positionIKHolder = transform.InverseTransformPoint(positionIKHolder);
			float y = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed);
			targetIKPosition.y += y;
			lastFootPositionY = y;
			targetIKPosition = transform.TransformPoint(targetIKPosition);

			foot.data.target.rotation = rotationIKHolder;
		}
		foot.data.target.position = targetIKPosition;
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
