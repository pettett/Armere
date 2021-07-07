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


	//class to allow player interactions with the environment through IInteractable scripts
	public class Interact : MovementState<InteractTemplate>
	{
		public override string StateName => "Interact";
		int currentLookAt;
		List<IInteractable> interactablesInRange = new List<IInteractable>();
		IInteractable prevTarget;
		public bool enabled = false;

		public Interact(PlayerMachine c, InteractTemplate t) : base(c, t)
		{

		}

		public override void Start()
		{

			if (!enabled)
			{
				interactablesInRange = new List<IInteractable>();

				ScanForInteractables();
				c.inputReader.actionEvent += OnInteract;

				enabled = true;
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

				t.OnEndHighlight?.Invoke();

				c.inputReader.actionEvent -= OnInteract;
			}
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
				if (prevTarget != null)
					//Not looking at any targets or no targets exist
					RemovePrevTarget();
			}
			else if (prevTarget != interactablesInRange[currentLookAt])
			{
				//no target before this, start applying the prompt
				t.OnBeginHighlight?.Invoke(interactablesInRange[currentLookAt]);

				prevTarget = interactablesInRange[currentLookAt];

				c.animationController.SetLookAtTarget(interactablesInRange[currentLookAt].gameObject.transform);
			}
		}

		void RemovePrevTarget()
		{
			c.animationController.ClearLookAtTargets();
			prevTarget = null;


			t.OnEndHighlight?.Invoke();

		}

		void OnInteractableRemoved(IInteractable interactable)
		{
			//Test to see if there is another interactable. If not, exit
			ExitInteractable(interactable);
		}

		void ExitInteractable(IInteractable exit)
		{
			if (prevTarget == exit)
			{
				RemovePrevTarget();
			}
			interactablesInRange.Remove(exit);
		}

		public void OnInteract(InputActionPhase phase)
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
						//TODO: This should be done in the Interact(c) phase
						switch (i)
						{
							case AIDialogue converser:
								machine.ChangeToState(t.interactNPC.StartConversation(converser));
								break;
							case Climbable climbable:
								if (c.WorldUp == climbable.upDirection)
									machine.ChangeToState(t.interactLadder.Interact(climbable));
								break;
							case IDialogue dialogue:
								machine.ChangeToState(t.interactDialogue.Interact(dialogue));
								break;
							default:
								if (i.canInteract)
									foreach (var s in machine.currentStates)
										(s as IInteractReceiver)?.OnInteract(i);
								else
									ExitInteractable(interactablesInRange[currentLookAt]);
								break;
						}


					}
					else if (interactablesInRange != null)
						ExitInteractable(interactablesInRange[currentLookAt]);
				}
			}
		}

	}
}
