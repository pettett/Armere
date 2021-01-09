using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

namespace Armere.PlayerController
{
    interface IInteractReceiver
    {
        void OnInteract(IInteractable interactable);
    }

    [System.Serializable]
    //class to allow player interactions with the environment through IInteractable scripts
    public class Interact : MovementState
    {
        public override string StateName => "Interact";
        int currentLookAt;
        [NonSerialized] List<IInteractable> interactablesInRange = new List<IInteractable>();
        [NonSerialized] IInteractable prevTarget;
        public bool enabled = true;


        public override void Start()
        {
            interactablesInRange = new List<IInteractable>();
            enabled = true;


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
            if (!enabled) return;
            TestForInteractable(other);

        }


        bool TestForInteractable(Collider interactable)
        {
            if (!interactable.isTrigger) return false;

            if (interactable.TryGetComponent(out IInteractable i))
            {
                //Sometimes at the start the same interactable can be detected multiple times as it scans multiple triggers 
                //Make sure this interactable has not occured
                if (i.canInteract && !interactablesInRange.Contains(i))
                {
                    AddInteractable(i);
                    return true;
                }
            }
            else if (interactable.TryGetComponent(out IPassiveInteractable p))
            {
                if (p.canInteract)
                    p.Interact(c);
            }

            return false;
        }
        public void AddInteractable(IInteractable i)
        {
            interactablesInRange.Add(i);
        }

        public override void OnTriggerExit(Collider interactable)
        {
            if (!enabled) return;


            //if this was the interactable, remove it
            if (interactable.TryGetComponent<IInteractable>(out var i))
            {
                OnInteractableRemoved(i);
            }
        }
        public override void Update()
        {
            if (!enabled) return;

            Vector3 direction = transform.forward;
            Vector3 interactableDir;
            float dot;
            float bestDot = -1;
            currentLookAt = -1;

            for (int i = 0; i < interactablesInRange.Count; i++)
            {
                if (((Component)interactablesInRange[i]) == null || !interactablesInRange[i].canInteract)
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
                //no target before this, start applying the prompt
                UIPrompt.ApplyPrompt(interactablesInRange[currentLookAt].interactionDescription, c.playerInput.actions.FindAction("Action").controls[0].displayName);

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
            if (enabled && phase == InputActionPhase.Started)
            {
                //activate the interactable that is pointed most to the player

                if (currentLookAt != -1)
                {
                    IInteractable i = interactablesInRange[currentLookAt];
                    i.Interact(c);

                    if (i.canInteract)
                    {
                        //Somehow this needs to be improved
                        switch (i)
                        {
                            case NPC npc:
                                c.ChangeToState<Conversation>(npc);
                                break;
                            case Climbable climbable:
                                c.ChangeToState<LadderClimb>(climbable);
                                break;
                            case IDialogue dialogue:
                                c.ChangeToState<Dialogue>(dialogue);
                                break;
                        }

                        if (i.canInteract)
                        {
                            (c.currentState as IInteractReceiver)?.OnInteract(i);
                        }
                        else
                            ExitInteractable(interactablesInRange[currentLookAt]);
                    }
                    else if (interactablesInRange != null)
                        ExitInteractable(interactablesInRange[currentLookAt]);
                }
            }
        }

        public override void End()
        {
            if (enabled)
            {
                enabled = false;
                for (int i = 0; i < interactablesInRange.Count; i++)
                {
                    ExitInteractable(interactablesInRange[i]);
                }
                interactablesInRange.Clear();
                interactablesInRange = null;

                //Remove the "Interact" prompt
                UIPrompt.ResetPrompt();

            }
        }
    }
}
