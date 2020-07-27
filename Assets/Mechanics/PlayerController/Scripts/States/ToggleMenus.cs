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

        bool inMenus;
        bool inConsole;


        public override void Start()
        {
            c.onPlayerInput += OnInput;
        }
        public override void End()
        {
            c.onPlayerInput -= OnInput;
        }


        public bool OnInput(InputAction.CallbackContext context)
        {
            if (!inConsole && context.action.name == "TabMenu" && context.phase == InputActionPhase.Started)
            {
                inMenus = !inMenus;
                UpdateMenus();
                return false;
            }
            else if (!inMenus && context.action.name == "Console" && context.phase == InputActionPhase.Started)
            {
                inConsole = !inConsole;
                UpdateConsole();
                return false;
            }
            return true;
        }
        void UpdateConsole()
        {
            if (inConsole)
            {
                SetPaused(true);
                Console.Enable(() =>
                {
                    inConsole = false;
                    UpdateConsole();
                });
            }
            else
            {
                Console.Disable();
                SetPaused(false);
            }
        }



        void UpdateMenus()
        {
            if (inMenus)
            {
                UIController.SetTabMenu(true);
                SetPaused(true);
            }
            else
            {
                UIController.SetTabMenu(false);
                SetPaused(false);
            }
        }

        void SetPaused(bool p)
        {
            if (p)
            {
                c.Pause();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                c.cameraController.lockingMouse = false;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                c.cameraController.lockingMouse = true;
                c.Play();
            }
        }
    }

}