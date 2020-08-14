using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace PlayerController
{
    [System.Serializable]
    //class to allow player interactions with the environment through IInteractable scripts
    public class Interact : ParallelState
    {
        public override string StateName => "Interact";
        int currentLookAt;
        List<IInteractable> interactablesInRange = new List<IInteractable>();

        IInteractable prevTarget;

        public override void Start()
        {

            ScanForInteractables();
        }
        ///<summary>Perform an overlap capsule to check for interactables, returning whether it did or not</summary>
        void ScanForInteractables()
        {
            interactablesInRange.Clear();

            //Test for interactables on position
            Collider[] hits = Physics.OverlapCapsule(
                transform.position,
                transform.position + c.collider.height * Vector3.up,
                 c.collider.radius,
                 Physics.AllLayers,
                 QueryTriggerInteraction.Collide);

            foreach (var c in hits)
            {
                TestForInteractable(c);
            }

        }

        public override void OnTriggerEnter(Collider other)
        {
            TestForInteractable(other);
        }

        bool TestForInteractable(Collider interactable)
        {
            if (interactable.TryGetComponent(out IInteractable i))
            {
                if (i.canInteract)
                {
                    interactablesInRange.Add(i);



                    return true;
                }
            }
            return false;
        }

        public override void OnTriggerExit(Collider interactable)
        {
            //if this was the interactable, remove it
            if (interactable.TryGetComponent<IInteractable>(out var i))
            {
                OnInteractableRemoved(i);
            }
        }
        public override void Update()
        {
            Vector3 direction = transform.forward;
            Vector3 interactableDir;
            float dot;
            float bestDot = -1;
            currentLookAt = -1;

            for (int i = 0; i < interactablesInRange.Count; i++)
            {
                if (!interactablesInRange[i].canInteract)
                {
                    OnInteractableRemoved(interactablesInRange[i]);
                }
                else
                {
                    //Test to see if this interactable is the one the player is looking most at
                    interactableDir = (interactablesInRange[i].gameObject.transform.position - transform.position).normalized;

                    dot = Vector3.Dot(direction, interactableDir);
                    //Needs to be within interactable look dot
                    if (dot > bestDot && dot > interactablesInRange[i].requiredLookDot)
                    {
                        bestDot = dot;
                        currentLookAt = i;
                    }

                }
            }


            if (currentLookAt == -1)
            {
                //Not looking at any targets or no targets exist
                RemovePrevTarget();
            }
            else if (prevTarget != interactablesInRange[currentLookAt])
            {
                if (prevTarget != null)
                    prevTarget.OnEndHighlight();
                else //no target before this, start applying the prompt
                    UIPrompt.ApplyPrompt("Interact", c.playerInput.actions.FindAction("Action").controls[0].displayName);

                interactablesInRange[currentLookAt].OnStartHighlight();
                prevTarget = interactablesInRange[currentLookAt];
            }

        }
        void RemovePrevTarget()
        {
            if (prevTarget != null)
            {
                prevTarget.OnEndHighlight();
                prevTarget = null;
                UIPrompt.ResetPrompt();
            }
        }
        void OnInteractableRemoved(IInteractable interactable)
        {
            //Test to see if there is another interactable. If not, exit
            ExitInteractable(interactable);
        }

        void ExitInteractable(IInteractable exit)
        {
            exit.OnEndHighlight();
            interactablesInRange.Remove(exit);
        }

        public override void OnInteract(InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started)
            {
                //activate the interactable that is pointed most to the player

                if (currentLookAt != -1)
                {
                    interactablesInRange[currentLookAt].Interact(c);
                    //Somehow this needs to be improved
                    switch (interactablesInRange[currentLookAt])
                    {
                        case NPC npc:
                            c.ChangeToState<Conversation>(npc);
                            break;
                    }
                }
            }
        }
        public override void End()
        {
            for (int i = 0; i < interactablesInRange.Count; i++)
            {
                ExitInteractable(interactablesInRange[i]);
            }
            //Remove the "Interact" prompt
            UIPrompt.ResetPrompt();
        }
    }
}
