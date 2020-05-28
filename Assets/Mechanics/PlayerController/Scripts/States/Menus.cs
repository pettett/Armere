using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{
    //allow the player to access the menu system if tab is pressed
    [System.Serializable]
    public class Menus : MovementState
    {
        public override string StateName => "In Menus";
        public override void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            UIController.SetTabMenu(true);
            c.onPlayerInput += OnInput;

            c.paused = true;
        }
        public override void End()
        {
            UIController.SetTabMenu(false);
            c.onPlayerInput -= OnInput;

            c.paused = false;
        }

        public void OnInput(InputAction.CallbackContext context)
        {
            if (context.action.name == "TabMenu" && context.ReadValue<float>() == 1)
                c.ChangeToState<PlayerController.Player_CharacterController.Walking>();

        }


    }

}