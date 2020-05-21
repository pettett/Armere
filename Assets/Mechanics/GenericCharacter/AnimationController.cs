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

public class AnimationController : MonoBehaviour
{
    public readonly struct StringHashes
    {
        public readonly int VelocityX;
        public readonly int VelocityZ;

        public StringHashes(string VelocityX, string VelocityZ)
        {
            this.VelocityX = Animator.StringToHash(VelocityX);
            this.VelocityZ = Animator.StringToHash(VelocityZ);
        }
    }



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

    void OnAnimatorIK(int layerIndex)
    {
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

    void UpdateIK()
    {

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

}
