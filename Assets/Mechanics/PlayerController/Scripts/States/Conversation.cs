using UnityEngine;

namespace PlayerController
{
    [System.Serializable]
    public class Conversation : MovementState
    {
        public override string StateName => "In Conversation";
        public override void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            c.rb.velocity = Vector3.zero;
            c.cameraController.DisableControl();
        }
        public override void End()
        {
            c.cameraController.EnableControl();
        }
    }
}