using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [System.Serializable]
    public class CameraControlSettings
    {
        public float cameraOrbitingDistance;
        public float cameraVerticalOffset;
        public bool verticalOffsetRelativeToTarget;
        public float cameraHorizontalOffset;
        public bool horizontalOffsetRelativeToTarget;
        public Transform cameraTarget;

        public LayerMask cameraCollisionLayerMask;

        public bool inheritTargetRotation;
        public bool captureMouse;
        public AnimationCurve cameraStepCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    }
    [System.Serializable]
    public class CameraControl : MovementState
    {
        public override string StateName => "Camera Control";

        public bool cameraSmoothing = false;

        public float cameraSmoothAmount = 10f;
        public float cameraStepResetTime = 0.05f;



        //used to change how the height of the camera will change for a short time


        [System.NonSerialized] CameraControlSettings activeCameraSettings;

        Vector3 _camOffset;
        Vector2 mouseDelta;
        Camera cam;
        public Vector2 camRotation = Vector2.zero;
        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y =
                    cam.nearClipPlane *
                    Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);
                halfExtends.x = halfExtends.y * cam.aspect;
                halfExtends.z = 0f;
                return halfExtends;
            }
        }

        bool starting = false;
        float startT = 0;
        float startTVel = 0;
        Vector3 startPos;
        Quaternion startRot;

        [System.NonSerialized] DebugMenu.DebugEntry entry;
        RaycastHit[] hit = new RaycastHit[1];

        bool controlling = true;
        ///<summary>
        ///Change the active camera settings profile
        ///</summary>
        public void SetCameraControlSettings(CameraControlSettings settings)
        {
            activeCameraSettings = settings;
            //

            Cursor.lockState = settings.captureMouse ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = false;

        }
        public void ResetCameraControlSettings()
        {
            SetCameraControlSettings(c.playerCameraSettings);
        }

        public void SetVerticalOffset(float offset)
        {
            activeCameraSettings.cameraVerticalOffset = offset;
        }


        public override void Start()
        {
            ResetCameraControlSettings();

            entry = DebugMenu.CreateEntry("Player", "Direction ({0:0.0} / {1:0.0}) Recoil ({2:0.0} / {3:0.0})", 180, 0, 0, 0);

            if (c.persistentStateData.ContainsKey("camRotation"))
            {
                camRotation = (Vector2)c.persistentStateData["camRotation"];
            }
            cam = c.cameraTransform.GetComponent<Camera>();

            Cinemachine.CinemachineCore.GetInputAxis = GetInputAxis;

        }
        public override void End()
        {
            DebugMenu.RemoveEntry(entry);
            c.persistentStateData["camRotation"] = camRotation;
        }

        public Vector3 TransformInput(Vector2 input)
        {
            Vector3 direction = new Vector3(input.x, 0, input.y);
            direction = Quaternion.Euler(0, c.cameraTransform.eulerAngles.y, 0) * direction;
            return direction;
        }

        public void DisableControl()
        {
            controlling = false;
            c.freeLook.Priority = 0;
        }
        public void EnableControl()
        {
            //start the transition from free camare to game camera
            startPos = cam.transform.position;
            startRot = cam.transform.rotation;
            starting = true;
            startT = 0;
            controlling = true;
            c.freeLook.Priority = 10;
        }


        float GetInputAxis(string axisName)
        {
            switch (axisName)
            {
                case "Mouse X":
                    return -mouseDelta.x;
                case "Mouse Y":
                    return -mouseDelta.y;
                default:
                    return 0;
            }
        }

        public override void LateUpdate()
        {
            if (!controlling) return;

            mouseDelta = Mouse.current.delta.ReadValue() * SettingsManager.settings.sensitivity * 0.01f;

            return;

            if (c.currentCameraStepTime <= 1)
            {
                c.cameraStepHeightOffset = -c.cameraStepDistance * activeCameraSettings.cameraStepCurve.Evaluate(c.currentCameraStepTime);
                c.currentCameraStepTime += Time.deltaTime / cameraStepResetTime;
            }
            else
            {
                c.cameraStepHeightOffset = 0;
            }

            //all camera controls happen in late update so they can react to events that happened in the frame

            Quaternion lookRotation;
            //calculate target position

            _camOffset = activeCameraSettings.cameraTarget.position +
                (activeCameraSettings.verticalOffsetRelativeToTarget
                ? activeCameraSettings.cameraTarget.TransformDirection(activeCameraSettings.cameraVerticalOffset * Vector3.up)
                : c.cameraTransform.TransformDirection(activeCameraSettings.cameraVerticalOffset * Vector3.up)) +

                (activeCameraSettings.horizontalOffsetRelativeToTarget
                ? activeCameraSettings.cameraTarget.TransformDirection(activeCameraSettings.cameraHorizontalOffset * Vector3.right)
                : c.cameraTransform.TransformDirection(activeCameraSettings.cameraHorizontalOffset * Vector3.right));


            if (Gamepad.current != null)
                mouseDelta += Gamepad.current.rightStick.ReadValue() * Time.deltaTime * SettingsManager.settings.sensitivity * 20;

            //rotate left/right

            //if there is some camera look offset, use the mouse movement to counteract it
            if (mouseDelta.y != 0 && c.cameraLookOffset.y != 0)
            {
                //test to see if this movement would counteract the camera offset
                float newOffset = c.cameraLookOffset.y + mouseDelta.y;
                if (Mathf.Sign(mouseDelta.y) != Mathf.Sign(c.cameraLookOffset.y))
                {
                    //this would take the camera in the right direction
                    if (Mathf.Sign(newOffset) != Mathf.Sign(c.cameraLookOffset.y))
                    {
                        //this overshot the camera look offset
                        mouseDelta.y = newOffset;
                        c.cameraLookOffset.y = 0;
                    }
                    else
                    {
                        //not overshot
                        mouseDelta.y = 0;
                        c.cameraLookOffset.y = newOffset;
                    }
                }
            }



            //else, leave it to the camera rotation
            camRotation += mouseDelta;

            //clamp horizontal rotation into range -180 to 180
            if (camRotation.x > 180)
                camRotation.x -= 360;
            if (camRotation.x < -180)
                camRotation.x += 360;

            //clamp the max rotation
            var rot = camRotation + c.cameraLookOffset;

            camRotation.y = Mathf.Clamp(camRotation.y, -80, 80);
            rot.y = Mathf.Clamp(rot.y, -80, 80);

            lookRotation = Quaternion.Euler(-rot.y, rot.x, 0);

            if (activeCameraSettings.inheritTargetRotation)
            {
                lookRotation *= activeCameraSettings.cameraTarget.rotation;
            }

            if (cameraSmoothing)
            {
                lookRotation = Quaternion.Slerp(c.cameraTransform.rotation, lookRotation, cameraSmoothAmount * Time.deltaTime);
            }


            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = _camOffset - lookDirection * activeCameraSettings.cameraOrbitingDistance;


            if (Physics.BoxCastNonAlloc(
                 _camOffset, CameraHalfExtends, -c.cameraTransform.forward,
                 hit, lookRotation, activeCameraSettings.cameraOrbitingDistance - cam.nearClipPlane,
                 activeCameraSettings.cameraCollisionLayerMask, QueryTriggerInteraction.Ignore) != 0)
            {
                lookPosition = _camOffset - lookDirection * (hit[0].distance + cam.nearClipPlane);
            }
            if (starting)
            {
                startT = Mathf.SmoothDampAngle(startT, 1, ref startTVel, 0.3f, Mathf.Infinity);
                if (startT == 1)
                {
                    starting = false;
                }
                c.cameraTransform.SetPositionAndRotation(
                   Vector3.Lerp(startPos, lookPosition, startT),
                   Quaternion.Slerp(startRot, lookRotation, startT));
            }
            else
            {
                //move camera to target position
                c.cameraTransform.SetPositionAndRotation(lookPosition, lookRotation);
            }


            c.input.inputCamera = Vector2.zero;


            entry.values[0] = camRotation.x;
            entry.values[1] = -camRotation.y;
            entry.values[2] = c.cameraLookOffset.x;
            entry.values[3] = -c.cameraLookOffset.y;
        }







    }
}
