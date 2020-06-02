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
            c.rb.isKinematic = true;
            c.cutsceneCamera.Priority = 50;
        }
        public override void End()
        {
            c.rb.isKinematic = false;
            c.cameraController.EnableControl();
            c.cutsceneCamera.Priority = 0;
        }
        public override void Animate(AnimatorVariables vars)
        {
            animator.SetFloat(vars.vertical.id, 0);
        }
    }
}