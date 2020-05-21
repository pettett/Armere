using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace PlayerController
{
    [System.Serializable]
    //class to allow player interactions with the environment through IInteractable scripts
    public class Interact : ParallelState
    {
        public override string StateName => "Interact";

        IInteractable currentInteractable;

        public override void OnTriggerEnter(Collider interactable)
        {
            if (interactable.TryGetComponent(out IInteractable i))
            {
                currentInteractable = i;
                PlayerInput playerInput = c.GetComponent<PlayerInput>();
                UIPrompt.ApplyPrompt("Interact", playerInput.actions.FindAction("Action").controls[0].displayName);
            }
        }
        public override void OnTriggerExit(Collider interactable)
        {
            //if this was the interactable, remove it
            if (interactable.GetComponent<IInteractable>() == currentInteractable)
            {
                ExitInteractable();
            }
        }
        public override void Update()
        {
            if (currentInteractable != null && currentInteractable.enabled == false)
            {
                ExitInteractable();
            }
        }
        void ExitInteractable()
        {
            currentInteractable = null;
            UIPrompt.ResetPrompt();
        }

        public override void OnInteract(float state)
        {
            if (state == 1)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.Interact(c);
                }
            }
        }
        public override void End()
        {
            ExitInteractable();
        }
    }
}
