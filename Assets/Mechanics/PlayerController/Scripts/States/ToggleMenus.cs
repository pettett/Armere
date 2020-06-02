using UnityEngine;
using System;
using UnityEngine.InputSystem;
namespace PlayerController
{
    //allow the player to access the menu system if tab is pressed
    [System.Serializable]
    public class ToggleMenus : ParallelState
    {



        public override string StateName => "Enter Menus";
        public override void Start()
        {
            c.onPlayerInput += OnInput;
        }
        public override void End()
        {
            c.onPlayerInput -= OnInput;
        }

        bool inMenus;
        public void OnInput(InputAction.CallbackContext context)
        {
            if (context.action.name == "TabMenu" && context.ReadValue<float>() == 1)
            {
                inMenus = !inMenus;
                UpdateMenus();
            }
        }

        void UpdateMenus()
        {
            if (inMenus)
            {
                c.Pause();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                UIController.SetTabMenu(true);
                c.cameraController.lockingMouse = false;
            }
            else
            {
                UIController.SetTabMenu(false);
                c.cameraController.lockingMouse = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                c.Play();
            }
        }
    }

}