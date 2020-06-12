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

        void OnCommand(Console.Command command)
        {

            void DesiredInputs(int num)
            {
                if (command.values.Length < num)
                    throw new ArgumentException("Not enough inputs for command");
            }

            switch (command.func)
            {
                case "tp":
                    DesiredInputs(1);
                    if (TeleportWaypoints.singleton.waypoints.ContainsKey(command.values[0]))
                    {
                        var t = TeleportWaypoints.singleton.waypoints[command.values[0]];
                        transform.SetPositionAndRotation(t.position, t.rotation);
                    }
                    break;
                case "time":
                    DesiredInputs(1);
                    if (command.values[0] == "day")
                        print("Made day");
                    break;
            }
            inConsole = false;
            UpdateConsole();
        }
        public override void Start()
        {
            c.onPlayerInput += OnInput;
        }
        public override void End()
        {
            c.onPlayerInput -= OnInput;
        }


        public void OnInput(InputAction.CallbackContext context)
        {
            if (!inConsole && context.action.name == "TabMenu" && context.ReadValue<float>() == 1)
            {
                inMenus = !inMenus;
                UpdateMenus();
            }
            else if (!inMenus && context.action.name == "Console" && context.ReadValue<float>() == 1)
            {
                inConsole = !inConsole;
                UpdateConsole();
            }

        }
        void UpdateConsole()
        {
            if (inConsole)
            {
                print("Activated Console");
                SetPaused(true);
                Console.Enable(OnCommand);
            }
            else
            {
                print("Deactivated console");
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