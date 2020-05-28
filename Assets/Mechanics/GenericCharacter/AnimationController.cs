using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    [Range(0, 1)] [SerializeField] private float pelvisVerticalSpeed = 0.28f;
    [Range(0, 1)] [SerializeField] private float feetToIKPositionSpeed = 0.5f;
    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";

    public bool useFootCurvesForRotationGrounding;
    public bool showSolver;

    [Header("AC")]

    public bool useIK;
    public bool localPlayer = false;


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
    public HoldPoint[] holdPoints;

    Animator anim;


    public float headLookAtPositionWeight;
    public Vector3 headLookAtPosition;

    public GameObject mesh;

    public bool useAnimationHook = false;

    public float velocityScaler = 1;
    IAnimatable animationHook;
    StringHashes hashes;

    #endregion



    void Start()
    {


        anim = GetComponent<Animator>();
        mesh.SetActive(!localPlayer);

        if (useAnimationHook)
        {
            animationHook = GetComponent<IAnimatable>();
            hashes = new StringHashes("VelocityX", "VelocityZ");
        }
    }

    bool _thirdPerson;
    public bool thirdPerson
    {
        get
        {
            return _thirdPerson;
        }
        set
        {
            _thirdPerson = value;
            //only disable the mesh if it is the local player in third person
            mesh.SetActive(_thirdPerson || !localPlayer);
        }
    }

    private void FixedUpdate()
    {
        if (enableFeetIK && anim != null)
        {
            AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

            //find and raycast to the ground to find positions
            FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation); // find ground under right foot
            FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation); // find ground under right foot
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

            anim.SetLookAtWeight(headLookAtPositionWeight);
            anim.SetLookAtPosition(headLookAtPosition);

        }
    }
    private void Update()
    {
        if (useAnimationHook)
        {
            Vector3 localVelocity = animationHook.transform.InverseTransformDirection(animationHook.velocity / animationHook.maxSpeed);
            anim.SetFloat(hashes.VelocityX, localVelocity.x * velocityScaler);
            anim.SetFloat(hashes.VelocityZ, localVelocity.z * velocityScaler);
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
        float totalOffset = Mathf.Min(lOffsetPosition, rOffsetPosition);

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
        feetPosition = anim.GetBoneTransform(foot).position;
        feetPosition.y = transform.position.y + heightFromGroundRaycast;
    }
    #endregion

}
