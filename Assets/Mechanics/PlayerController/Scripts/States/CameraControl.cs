using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [System.Serializable]
    public class CameraControl : MovementState
    {
        public override string StateName => "Camera Control";

        //used to change how the height of the camera will change for a short time
        [System.NonSerialized] DebugMenu.DebugEntry entry;
        bool controlling = true;
        public bool lockingMouse = true;
        Vector2 mouseDelta;

        public override void Start()
        {
            entry = DebugMenu.CreateEntry("Player", "Direction ({0:0.0} / {1:0.0}) Recoil ({2:0.0} / {3:0.0})", 180, 0, 0, 0);

            Cinemachine.CinemachineCore.GetInputAxis = GetInputAxis;
        }

        public override void End()
        {
            DebugMenu.RemoveEntry(entry);
        }

        public Vector3 TransformInput(Vector2 input)
        {
            Vector3 direction = new Vector3(input.x, 0, input.y);
            direction = Quaternion.Euler(0, GameCameras.s.cameraTransform.eulerAngles.y, 0) * direction;
            return direction;
        }

        public void DisableControl()
        {
            controlling = false;
            GameCameras.s.freeLook.Priority = 0;
        }
        public void EnableControl()
        {
            //start the transition from free camare to game camera
            controlling = true;
            GameCameras.s.freeLook.Priority = 10;
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
            if (controlling)
                mouseDelta = Mouse.current.delta.ReadValue() * SettingsManager.settings.sensitivity * 0.01f;
            Cursor.lockState = lockingMouse ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockingMouse;


            CameraVolumeController.UpdateVolumeEffect(transform.position);

        }

    }
}
