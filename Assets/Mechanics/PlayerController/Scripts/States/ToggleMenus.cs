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

        public void OnInput(InputAction.CallbackContext context)
        {
            if (context.action.name == "TabMenu" && context.ReadValue<float>() == 1)
                c.ChangeToState<Menus>();

        }

    }

}