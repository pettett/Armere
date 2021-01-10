using UnityEngine;
using System;
using UnityEngine.InputSystem;
namespace Armere.PlayerController
{
    //allow the player to access the menu system if tab is pressed
    [System.Serializable]
    public class ToggleMenus : MovementState
    {
        public override string StateName => "Enter Menus";
        public override char StateSymbol => 't';

        [NonSerialized] bool inMenus;
        [NonSerialized] bool inConsole;


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
                //Only enter if the game is not paused
                if (inMenus) inMenus = false;
                else if (!c.paused) inMenus = true;
                else return true;

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
            UIController.SetTabMenu(inMenus);
            SetPaused(inMenus);
        }

        void SetPaused(bool p)
        {
            if (p)
            {
                c.PauseControl();
            }
            else
            {
                c.ResumeControl();
            }
        }
    }

}