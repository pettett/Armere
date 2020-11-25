using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

public class GameCameras : MonoBehaviour
{
    public static GameCameras s;

    public float defaultTrackingOffset = 1.6f;
    float _playerTrackingOffset = 1.6f;

    public float defaultRigOffset = 0;
    float _playerRigOffset = 0;

    public float cameraTargetXOffset = 0;



    public CameraProfile shoulderViewProfile;
    public float shoulderViewStrength;

    //default to everything but player
    public LayerMask cameraCollisionMask;
    public LayerMask doNotCollideWithCamera;
    public Transform cameraCollisionTarget;
    Camera regularCamera;

    public float playerTrackingOffset
    {
        get => _playerTrackingOffset;
        set
        {
            _playerTrackingOffset = value;
            if (freeLook != null)
            {
                freeLook.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
                freeLook.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
                freeLook.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
            }
        }
    }

    public Transform cameraTarget
    {
        get => freeLook.Follow;
        set
        {
            freeLook.Follow = value;
            freeLook.LookAt = value;
        }
    }


    public float playerRigOffset
    {
        get => _playerRigOffset;
        set
        {
            _playerRigOffset = value;
        }
    }
    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y =
                regularCamera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }
    private void Awake()
    {
        s = this;
    }
    private void Start()
    {
        cameraTarget = LevelInfo.currentLevelInfo.playerTransform.Find("Camera_Track");
        regularCamera = cameraTransform.GetComponent<Camera>();

        cameraCollisionTarget = cameraTarget;
    }

    Vector3 targetVel;
    private void Update()
    {
        cameraTarget.localPosition = Vector3.SmoothDamp(cameraTarget.localPosition, Vector3.right * cameraTargetXOffset, ref targetVel, 0.1f);

        cameraTransform.position = Vector3.Lerp(cameraCollisionTarget.position + new Vector3(0, _playerTrackingOffset, 0), cameraTransform.parent.position, currentCameraLerp);

        //Stop jittering from desync between high fps and low physics updates
        currentCameraLerp += cameraLerpSpeed * Time.deltaTime * 0.25f;
    }

    float currentCameraLerp = 1f;
    float cameraLerpSpeed;
    float lastPhysicsLerp = 1f;

    private void FixedUpdate()
    {
        Vector3 halfExtents = CameraHalfExtends;
        Vector3 collisionTarget = cameraCollisionTarget.position + new Vector3(0, _playerTrackingOffset + halfExtents.y, 0);

        float distance = Vector3.Distance(collisionTarget, cameraTransform.parent.position);

        Collider[] cols = new Collider[1];
        int hits = Physics.OverlapBoxNonAlloc(
            cameraTransform.parent.position, halfExtents + new Vector3(0, 0, regularCamera.nearClipPlane),
            cols, cameraTransform.rotation, doNotCollideWithCamera);

        LayerMask l = cameraCollisionMask;
        if (hits > 0)
        {
            //camera is inside a do not collide with camera object, move in front
            l |= doNotCollideWithCamera;

            Debug.Log("Colliding with object");
        }

        //Linecast from camera to target to stop collision
        if (Physics.BoxCast(
            collisionTarget, halfExtents, -cameraTransform.forward,
            out RaycastHit hit, cameraTransform.rotation,
            distance - regularCamera.nearClipPlane, l))
        {
            cameraLerpSpeed = lastPhysicsLerp;
            //Hit something
            float newLerp = (hit.distance + regularCamera.nearClipPlane) / distance;

            if (lastPhysicsLerp < newLerp)
            {
                //Gotten closer to 1 so further away, lerp away
                currentCameraLerp = Mathf.Lerp(currentCameraLerp, newLerp, Time.deltaTime * 10);
            }
            else
            {
                //Gotten closer
                currentCameraLerp = newLerp;
            }

            lastPhysicsLerp = currentCameraLerp;


            cameraLerpSpeed -= lastPhysicsLerp;
            //Scale so it is in units per second. Also make negative
            cameraLerpSpeed /= -Time.fixedDeltaTime;

            //Debug.Log(cameraLerpSpeed);
        }
        else
        {
            //Did not hit, move back
            currentCameraLerp = Mathf.Lerp(currentCameraLerp, 1, Time.deltaTime * 10);

            cameraLerpSpeed = 0;
            lastPhysicsLerp = 0;
        }
    }


    public void SwitchCinemachineCameras(Cinemachine.CinemachineFreeLook from, Cinemachine.CinemachineFreeLook to)
    {
        //Switch priorities

        from.Priority = 10;
        to.Priority = 20;

        to.m_XAxis.Value = from.m_XAxis.Value;
        to.m_YAxis.Value = from.m_YAxis.Value;
    }


    public void FocusCutsceneCameraToTargets(params Transform[] targets)
    {
        //Setup camera
        //Add all targets including the player
        conversationGroup.m_Targets = targets.Select(t => GenerateTarget(t)).ToArray();
        //Make clipping work around the target
        cameraCollisionTarget = conversationGroup.transform;

        cutsceneCamera.Priority = 50;
    }

    public void DisableCutsceneCamera()
    {
        //Make clipping work around the target
        cameraCollisionTarget = cameraTarget;
        cutsceneCamera.Priority = 10;
    }

    public static CinemachineTargetGroup.Target GenerateTarget(Transform transform, float weight = 1, float radius = 1)
    {
        return new CinemachineTargetGroup.Target() { target = transform, weight = weight, radius = radius };
    }


    public Transform cameraTransform;
    public CinemachineFreeLook freeLook;

    public CinemachineTargetGroup conversationGroup;
    public CinemachineVirtualCamera cutsceneCamera;
}
