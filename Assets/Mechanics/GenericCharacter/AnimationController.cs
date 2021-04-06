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
	private Vector3 rightFootPosition, leftFootPosition, leftFootIKPosition, rightFootIKPosition;
	private Quaternion rightFootIKRotation, leftFootIKRotation;
	private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;
	#endregion

	#region Public Variables
	[Header("Feet Grounder")]
	public bool enableFeetIK = true;
	[Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;

	[Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;
	[SerializeField] private LayerMask groundLayer;
	public float pelvisOffset = 0f;
	[SerializeField] private float crouchOffset = 0f;
	public float footOffset = -0.19f;
	[Range(0, 20)] [SerializeField] private float pelvisVerticalSpeed = 0.28f, feetToIKPositionSpeed = 0.5f;

	public bool useFootCurvesForRotationGrounding;
	public float maxFootHeightForIK = 0.2f;

	public Transform localHeadLookTarget;

	[Header("AC")]

	public bool useIK;

	public TwoBoneIKConstraint rightHandConstraint;
	public TwoBoneIKConstraint leftHandConstraint;
	public MultiAimConstraint headLook;

	Transform leftFootBone;
	Transform rightFootBone;
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


	public Animator anim { get; private set; }

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
		headLook.weight = 1;
		lookAtTarget = target.transform;
	}
	public void ClearLookAtTargets()
	{
		headLook.weight = 0;
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
	private void Awake()
	{
		anim = GetComponent<Animator>();
	}
	void Start()
	{


		head = anim.GetBoneTransform(HumanBodyBones.Head);
		rb = GetComponent<Rigidbody>();

		leftFootBone = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
		rightFootBone = anim.GetBoneTransform(HumanBodyBones.RightFoot);

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
		if (lookAtTarget != null)
		{
			localHeadLookTarget.position = lookAtTarget.position;
		}

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
	private void OnAnimatorIK(int layerIndex)
	{
		if (enableFeetIK)
		{
			MovePelvisHeight();
			//Right foot ik position and rotation
			float left = 1;
			float right = 1;
			if (useFootCurvesForRotationGrounding)
			{
				float baseHeight = anim.GetFloat("FootBaseHeight");
				float leftHeight = transform.InverseTransformPoint(leftFootBone.transform.position).y + footOffset - baseHeight;
				float rightHeight = transform.InverseTransformPoint(rightFootBone.transform.position).y + footOffset - baseHeight;

				left = 1 - Mathf.Clamp01(leftHeight / maxFootHeightForIK);

				right = 1 - Mathf.Clamp01(rightHeight / maxFootHeightForIK);
				//print($"{right},{rightHeight}");
			}

			anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, left);
			anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, right);

			anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, left);
			anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, right);

			MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);
			MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
		}

	}


	#region Solver Methods

	void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
	{

		//foot.data.target.position = positionIKHolder;

		Vector3 targetIKPosition = anim.GetIKPosition(foot);// foot.data.target.position;

		if (positionIKHolder != default(Vector3))
		{
			targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
			positionIKHolder = transform.InverseTransformPoint(positionIKHolder);
			float y = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed * Time.deltaTime);
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
		newPelvisPos.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPos.y, pelvisVerticalSpeed * Time.deltaTime);
		anim.bodyPosition = newPelvisPos;
		lastPelvisPositionY = newPelvisPos.y;
	}
	void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPosition, ref Quaternion feetIKRotation)
	{
		//Highwayyy tooooo theeeee raycastzone!
		//Locate this foot's position with raycast
		RaycastHit feetOutHit;

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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		if (leftFootIKPosition != default)
		{
			Gizmos.DrawWireSphere(leftFootIKPosition, 0.05f);
			Gizmos.DrawWireSphere(leftFootPosition, 0.05f);
			Gizmos.DrawLine(leftFootPosition, leftFootIKPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast));
		}
		if (rightFootIKPosition != default)
		{
			Gizmos.DrawWireSphere(rightFootIKPosition, 0.05f);
			Gizmos.DrawWireSphere(rightFootPosition, 0.05f);
			Gizmos.DrawLine(rightFootPosition, rightFootIKPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast));
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


	//Animation events

	public event System.Action onFootDown;
	public void FootDown() => onFootDown?.Invoke();
	public event System.Action onClank;
	public void OnClank() => onClank?.Invoke();
	public event System.Action onSwingStart;
	public void SwingStart() => onSwingStart?.Invoke();
	public event System.Action onSwingEnd;
	public void SwingEnd() => onSwingEnd?.Invoke();

}
