using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Armere.Inventory.UI;
using Armere.Inventory;

namespace Armere.PlayerController
{


	public class Walking : MovementState<WalkingTemplate>, IInteractReceiver
	{
		public override string StateName => "Walking";


		enum WalkingType { Walking, Sprinting, Crouching }



		Vector3 currentGroundNormal = new Vector3();


		//used to continue momentum when the controller hits a stair
		Vector3 lastVelocity;


		//shooting variables for gizmos
		DebugMenu.DebugEntry entry;

		bool forceForwardHeadingToCamera = false;
		bool grounded;
		bool holdingCrouchKey;
		bool holdingSprintKey;


		bool crouching { get => walkingType == WalkingType.Crouching; }
		bool sprinting { get => walkingType == WalkingType.Sprinting; }


		WalkingType walkingType = WalkingType.Walking;
		Dictionary<WalkingType, float> walkingSpeeds;
		bool inControl = true;
		Collider[] crouchTestColliders = new Collider[2];


		MeleeWeaponItemData meleeWeapon => (MeleeWeaponItemData)c.inventory.melee.items[c.currentMelee].item;



		public bool movingHoldable;
		public bool holdingBody;

		//Save system does not work with game references, yet.
		//This should be done but is not super important
		public HoldableBody holding;

		//WEAPONS
		Coroutine bowChargingRoutine;



		EquipmentSet<bool> equipping = new EquipmentSet<bool>(false, false, false);
		EquipmentSet<bool> activated = new EquipmentSet<bool>(false, false, false);

		bool backSwingSword = false;
		ShieldItemData SidearmAsShield => (ShieldItemData)c.inventory.sideArm[c.currentSidearm].item;
		bool SidearmIsShield => SidearmAsShield is ShieldItemData;


		bool holdingAltAttack = false;

		ScanForNearT<IAttackable> nearAttackables;

		public Spell currentCastingSpell;

		public bool Usable(ItemType t) => t switch
		{
			ItemType.SideArm => !c.sheathing.sidearm && !equipping.sidearm && !activated.sidearm && c.currentSidearm != -1,
			ItemType.Melee => !c.sheathing.melee && !equipping.melee && !activated.melee && c.currentMelee != -1,
			//Bow requires ammo and bow selection
			ItemType.Bow => !c.sheathing.bow && !equipping.bow && !activated.bow && c.currentBow != -1 && c.currentAmmo != -1,
			_ => false,
		};


		public bool Using(ItemType t)
		{
			switch (t)
			{
				case ItemType.SideArm: return !c.sheathing.sidearm && !equipping.sidearm && activated.sidearm && c.currentSidearm != -1;
				case ItemType.Melee: return !c.sheathing.melee && !equipping.melee && activated.melee && c.currentMelee != -1;
				//Bow requires ammo and bow selection
				case ItemType.Bow: return !c.sheathing.bow && !equipping.bow && activated.bow && c.currentBow != -1 && c.currentAmmo != -1;
				default: return false;
			}
		}


		public override void Start()
		{
			entry = DebugMenu.CreateEntry("Player", "Velocity: {0:0.0} Contact Point Count {1} Stepping Progress {2} On Ground {3}", 0, 0, 0, false);

			walkingSpeeds = new Dictionary<WalkingType, float>(){
				{WalkingType.Walking , t.walkingSpeed},
				{WalkingType.Crouching , t.crouchingSpeed},
				{WalkingType.Sprinting , t.sprintingSpeed},
			};

			//c.controllingCamera = false; // debug for camera parallel state

			//c.transform.up = Vector3.up;
			c.animationController.enableFeetIK = true;
			c.rb.isKinematic = false;

			animator.applyRootMotion = false;

			c.onPlayerInventoryItemAdded.onItemAddedEvent += OnItemAdded;

			c.health.onTakeDamage += OnTakeDamage;
			c.health.onBlockDamage += OnBlockDamage;


			nearAttackables = (ScanForNearT<IAttackable>)c.GetParallelState(typeof(ScanForNearT<IAttackable>));
			nearAttackables.updateEveryFrame = false;

			//Add input reader events

			c.inputReader.sprintEvent += OnSprint;
			c.inputReader.crouchEvent += OnCrouch;
			c.inputReader.attackEvent += OnAttack;
			c.inputReader.actionEvent += OnInteract;
			c.inputReader.altAttackEvent += OnAltAttack;
			c.inputReader.jumpEvent += OnJump;
			c.inputReader.selectSpellEvent += OnSelectSpell;

		}

		public override void End()
		{
			transform.SetParent(null, true);
			DebugMenu.RemoveEntry(entry);

			c.health.onTakeDamage -= OnTakeDamage;
			c.health.onBlockDamage -= OnBlockDamage;


			//make sure the collider is left correctly
			c.collider.height = c.walkingHeight;
			c.collider.center = Vector3.up * c.collider.height * 0.5f;


			c.onPlayerInventoryItemAdded.onItemAddedEvent -= OnItemAdded;

			//Remove input reader events

			c.inputReader.sprintEvent -= OnSprint;
			c.inputReader.crouchEvent -= OnCrouch;
			c.inputReader.attackEvent -= OnAttack;
			c.inputReader.actionEvent -= OnInteract;
			c.inputReader.altAttackEvent -= OnAltAttack;
			c.inputReader.jumpEvent -= OnJump;
			c.inputReader.selectSpellEvent -= OnSelectSpell;
			OnCancelBowEvent?.Invoke();

		}


		public void OnCrouch(InputActionPhase phase)
		{
			holdingCrouchKey = InputReader.PhaseMeansPressed(phase);
		}
		public void OnSprint(InputActionPhase phase)
		{
			holdingSprintKey = InputReader.PhaseMeansPressed(phase);
		}

		#region Holdables
		public void PlaceHoldable()
		{
			RemoveHoldable(Vector3.zero);
		}
		public void DropHoldable()
		{
			RemoveHoldable(Vector3.zero);
		}
		public void ThrowHoldable()
		{
			RemoveHoldable((transform.forward + Vector3.up).normalized * t.throwForce);
		}

		public void HoldHoldable(HoldableBody body)
		{
			c.StartCoroutine(c.UnEquipAll());

			holding = body;
			//Keep body attached to top of player;
			holding.transform.position = (transform.position + Vector3.up * (c.walkingHeight + holding.heightOffset));

			holding.joint.connectedBody = c.rb;

			holdingBody = true;

			UIKeyPromptGroup.singleton.ShowPrompts(
				c.inputReader,
				InputReader.groundActionMap,
				("Drop", InputReader.attackAction),
				("Throw", InputReader.altAttackAction));

			(c.GetParallelState(typeof(Interact)) as Interact).End();

			c.StartCoroutine(PickupHoldable());
		}

		IEnumerator PickupHoldable()
		{
			movingHoldable = true;
			yield return new WaitForSeconds(0.1f);
			movingHoldable = false;
		}
		#endregion
		#region Rest Points
		public void OnInteract(IInteractable interactable)
		{
			if (interactable is RestPoint restPoint)
			{
				Debug.Log("Started rest");
				inControl = false;

				UIKeyPromptGroup.singleton.ShowPrompts(
					c.inputReader,
					InputReader.groundActionMap,
					new UIKeyPromptGroup.KeyPrompt("Rest", InputReader.attackAction),
					new UIKeyPromptGroup.KeyPrompt("Roast Marshmellow", InputReader.altAttackAction)
					);

				if (c.TryGetParallelState<Interact>(out var s))
					s.End();



				//TODO: Make sitting a separate movement state
				c.StartCoroutine(c.UnEquipAll(() =>
				{
					c.animationController.TriggerTransition(c.transitionSet.startSitting);

					GameCameras.s.playerTrackingOffset = 0.5f;

					bool inDialogue = false;
					void OnMovementCancelSit(Vector2 movement) => CancelSit();

					void CancelSit()
					{
						if (inDialogue) return;
						//Return to walking
						c.animationController.TriggerTransition(c.transitionSet.stopSitting);
						inControl = true;
						UIKeyPromptGroup.singleton.RemovePrompts();

						GameCameras.s.playerTrackingOffset = GameCameras.s.defaultTrackingOffset;

						c.inputReader.actionEvent -= EnterSleepDialogue;
						c.inputReader.movementEvent -= OnMovementCancelSit;
						s?.Start();
					}

					IEnumerator TriggerTimeChange(float newTime)
					{

						float time = 1f;
						float fadeTime = 0.25f;

						fadeTime = Mathf.Clamp(fadeTime, 0, time / 2);

						// fadeoutImage.color = Color.clear; //Black with full transparency
						UIController.singleton.fadeoutImage.gameObject.SetActive(true);

						yield return UIController.singleton.Fade(0, 1, fadeTime, false);


						float fullyBlackTime = time - fadeTime * 2;

						yield return new WaitForSeconds(fullyBlackTime * 0.5f);
						c.changeTimeEventChannel.RaiseEvent(newTime);
						yield return new WaitForSeconds(fullyBlackTime * 0.5f);

						yield return UIController.singleton.Fade(1, 0, fadeTime, false);

						UIController.singleton.DisableFadeout();

					}

					void EnterSleepDialogue(InputActionPhase phase)
					{
						if (phase == InputActionPhase.Started)
						{
							if (!inDialogue)
							{
								//Run the dialogue
								c.runner.Add(restPoint.dialogue);
								c.runner.StartDialogue("Start");

								c.runner.AddCommandHandler("Morning", _ =>
								{
									c.StartCoroutine(TriggerTimeChange(6));
								});
								c.runner.AddCommandHandler("Noon", _ =>
								{
									c.StartCoroutine(TriggerTimeChange(12));
								});
								c.runner.AddCommandHandler("Night", _ =>
								{
									c.StartCoroutine(TriggerTimeChange(24));
								});
								c.runner.AddCommandHandler("None", _ =>
								{
									Debug.Log("None");
								});

								Yarn.Unity.DialogueUI.singleton.onDialogueEnd.AddListener(() =>
							   {
								   inDialogue = false;
								   c.runner.Clear();
								   c.runner.ClearStringTable();
								   c.runner.RemoveCommandHandler("Morning");
								   c.runner.RemoveCommandHandler("Noon");
								   c.runner.RemoveCommandHandler("Night");
								   c.runner.RemoveCommandHandler("None");
							   });
								inDialogue = true;
							}
						}
					}

					c.inputReader.actionEvent += EnterSleepDialogue;
					c.inputReader.movementEvent += OnMovementCancelSit;

				}
				));


			}
		}
		#endregion


		void OnBlockDamage(GameObject attacker, GameObject victim)
		{
			c.animationController.TriggerTransition(c.transitionSet.shieldImpact);
		}

		void OnTakeDamage(GameObject attacker, GameObject victim)
		{
			Vector3 direction = transform.position - attacker.transform.position;
			direction.y = 0;
			float dot = Vector3.Dot(direction, transform.forward);

			if (dot < 0)
			{

				c.animationController.TriggerTransition(c.transitionSet.swordFrontImpact);

			}
			else
			{
				c.animationController.TriggerTransition(c.transitionSet.swordBackImpact);
			}

			if (activated.melee == true)
			{
				//Cancell the sword swing
				RemoveWeaponTrigger(true);
			}

		}

		void RemoveHoldable(Vector3 acceleration)
		{
			holding.OnDropped();
			holding.rb.AddForce(acceleration, ForceMode.Acceleration);


			holdingBody = false;
			UIKeyPromptGroup.singleton.RemovePrompts();

			(c.GetParallelState(typeof(Interact)) as Interact).Start();


			holding = null;
		}


		#region Movement
		public void MoveThroughWater(ref float speedScalar)
		{
			//Find depth of water
			//Buffer of two: One for water surface, one for water base
			RaycastHit[] waterHits = new RaycastHit[2];
			float heightOffset = 2;

			int hits = Physics.RaycastNonAlloc(
				transform.position + new Vector3(0, heightOffset, 0),
				Vector3.down, waterHits,
				c.maxWaterStrideDepth + heightOffset,
				c.m_groundLayerMask | c.m_waterLayerMask,
				QueryTriggerInteraction.Collide);

			if (hits == 2)
			{
				WaterController w = waterHits[1].collider.GetComponentInParent<WaterController>();
				if (w != null)
				{
					//Hit water and ground
					float depth = waterHits[0].distance - waterHits[1].distance;

					float scaledDepth = depth / c.maxWaterStrideDepth;
					if (scaledDepth > 1)
					{
						//Start swimming
						Debug.Log("Too deep to walk");
						c.ChangeToState(t.swimming);
					}
					else if (scaledDepth >= 0)
					{
						//Striding through water
						//Slow speed of walk
						//Full depth walks at half speed
						speedScalar = (1 - scaledDepth) * c.maxStridingDepthSpeedScalar + (1 - c.maxStridingDepthSpeedScalar);
					}

				}

			}
			else if (hits == 1)
			{

				//Start swimming
				Debug.Log("Too deep to walk");
				c.ChangeToState(t.swimming);
			}
		}

		public void GetDesiredVelocity(Vector3 playerDirection, float movementSpeed, float speedScalar, out Vector3 desiredVelocity)
		{
			if (forceForwardHeadingToCamera)
			{
				Vector3 forward = GameCameras.s.cameraTransform.forward;
				forward.y = 0;
				transform.forward = forward;

				//Include speed scalar from water
				desiredVelocity = playerDirection * movementSpeed * speedScalar;
				//Rotate the velocity based on ground
				desiredVelocity = Quaternion.AngleAxis(0, currentGroundNormal) * desiredVelocity;
			}
			else if (playerDirection.sqrMagnitude > 0.1f)
			{
				Quaternion walkingAngle;

				if (GameCameras.s.m_UpdatingCameraDirection)
				{
					Vector3 dir = GameCameras.s.FocusTarget - transform.position;
					dir.y = 0;
					walkingAngle = Quaternion.LookRotation(dir);
				}
				else
				{
					walkingAngle = Quaternion.LookRotation(playerDirection);
				}




				float angle = Quaternion.Angle(transform.rotation, walkingAngle);
				//Debug.Log(angle);
				if (angle > 170 && c.rb.velocity.sqrMagnitude > 0.5f)
				{
					//Perform a 180 manuever

					desiredVelocity = Vector3.zero;
					c.StartCoroutine(Perform180());
				}
				else if (angle > 30f)
				{
					//Only allow the player to walk forward if they have finished turning to the direction
					//But do allow the player to run at a slight angle
					desiredVelocity = Vector3.zero;
				}
				else
				{
					//Let the player move in the direction they are pointing

					//scale required velocity by current speed
					//only allow sprinting if the play is moving forward

					//Include speed scalar from water
					desiredVelocity = playerDirection * movementSpeed * speedScalar;
					//Rotate the velocity based on ground
					desiredVelocity = Quaternion.AngleAxis(0, currentGroundNormal) * desiredVelocity;
				}

				//If not forcing heading, rotate towards walking
				transform.rotation = Quaternion.RotateTowards(transform.rotation, walkingAngle, Time.deltaTime * 800);

			}
			else
			{
				//No movement
				desiredVelocity = Vector3.zero;
			}
		}
		IEnumerator Perform180()
		{
			c.animationController.TriggerTransition(c.transitionSet.sword180);
			c.rb.velocity = Vector3.zero;
			//c.animator.runtimeAnimatorController.animationClips
			inControl = false;
			c.animator.applyRootMotion = true;
			yield return null;

			yield return new WaitForSeconds(c.animator.GetNextAnimatorClipInfo(0)[0].clip.length / 2 - Time.deltaTime);

			if (c.weaponGraphics.holdables.melee.sheathed)
			{
				c.animationController.TriggerTransition(c.transitionSet.freeMovement);
			}

			animator.SetFloat(c.animatorVariables.vertical.id, 1);
			//transform.forward = -transform.forward;
			c.animator.applyRootMotion = false;
			inControl = true;

		}
		public override void Update()
		{
			currentCastingSpell?.Update();
		}
		public override void FixedUpdate()
		{
			// if (c.onGround == false)
			// {
			//     //c.ChangeToState<Freefalling>();
			//     return;
			// }




			if (!inControl) return; //currently being controlled by some other movement coroutine

			Vector3 velocity = c.rb.velocity;
			Vector3 playerDirection = GameCameras.s.TransformInput(c.inputReader.horizontalMovement);

			grounded = FindGround(out var groundCP, out currentGroundNormal, playerDirection, c.allCPs);

			c.animationController.enableFeetIK = grounded;

			if (holdingSprintKey)
			{
				if (GameCameras.s.m_UpdatingCameraDirection)
				{
					//Do not sprint while focusing
					walkingType = WalkingType.Walking;
				}

				//Do not allow sprinting while a weapon is equipped, so wait until
				//The weapon for the set is sheathed before allowing sprint
				else if (c.weaponSet == PlayerController.WeaponSet.MeleeSidearm)
				{
					if (!c.sheathing.melee)
					{
						if (!c.weaponGraphics.holdables.melee.sheathed)
						{
							//Will only operate is sword exists
							c.StartCoroutine(c.SheathItem(ItemType.Melee));
						}
						else
						{
							walkingType = WalkingType.Sprinting;
						}
					}

					//Will only operate if sidearm exists
					if (!c.sheathing[ItemType.SideArm] && !c.weaponGraphics.holdables[ItemType.SideArm].sheathed)
					{
						//Will only operate is sword exists
						c.StartCoroutine(c.SheathItem(ItemType.SideArm));
					}

				}
				else
				{

					if (!c.sheathing.bow)
					{
						if (!c.weaponGraphics.holdables.bow.sheathed)
						{
							//Will only operate if bow exists
							OnCancelBowEvent?.Invoke();
							c.StartCoroutine(c.SheathItem(ItemType.Bow));
							//Reset to default weapon set
							c.weaponSet = (PlayerController.WeaponSet)0;
						}
						else
						{
							walkingType = WalkingType.Sprinting;
						}
					}
				}
			}



			//If no longer pressing the button return to normal movement
			else if (sprinting) walkingType = WalkingType.Walking;

			//List<ContactPoint> groundCPs = new List<ContactPoint>();

			float speedScalar = 1;

			//Test for water

			if (c.currentWater != null)
			{
				MoveThroughWater(ref speedScalar);
			}


			float currentMovementSpeed = walkingSpeeds[walkingType];

			GetDesiredVelocity(playerDirection, currentMovementSpeed, speedScalar, out Vector3 desiredVelocity);


			if (grounded)
			{
				//step up onto the stair, reseting the velocity to what it was
				if (FindStep(out Vector3 stepUpOffset, c.allCPs, groundCP, desiredVelocity))
				{
					//transform.position += stepUpOffset;
					//c.rb.velocity = lastVelocity;

					c.StartCoroutine(StepToPoint(transform.position + stepUpOffset, lastVelocity));
				}
			}
			else
			{
				if (!c.onGround)
				{
					//c.ChangeToState<Freefalling>();
				}
			}


			//c.transform.rotation = Quaternion.Euler(0, cc.camRotation.x, 0);
			if (holdingCrouchKey)
			{
				c.collider.height = t.crouchingHeight;
				walkingType = WalkingType.Crouching;
			}
			else if (crouching)
			{
				//crouch button not pressed but still crouching
				Vector3 p1 = transform.position + Vector3.up * c.walkingHeight * 0.05F;
				Vector3 p2 = transform.position + Vector3.up * c.walkingHeight;
				Physics.OverlapCapsuleNonAlloc(p1, p2, c.collider.radius, crouchTestColliders, c.m_groundLayerMask, QueryTriggerInteraction.Ignore);
				if (crouchTestColliders[1] == null)
					//There is no collider intersecting other then the player
					walkingType = WalkingType.Walking;
				else crouchTestColliders[1] = null;
			}

			if (!crouching)
				c.collider.height = c.walkingHeight;

			c.collider.center = Vector3.up * c.collider.height * 0.5f;


			Vector3 requiredForce = desiredVelocity - c.rb.velocity;
			requiredForce.y = 0;

			requiredForce = Vector3.ClampMagnitude(requiredForce, t.maxAcceleration * Time.fixedDeltaTime);

			//rotate the target based on the ground the player is standing on


			if (grounded)
				requiredForce -= currentGroundNormal * t.groundClamp;

			c.rb.AddForce(requiredForce, ForceMode.VelocityChange);

			lastVelocity = velocity;

			entry.values[0] = c.rb.velocity.magnitude;
			entry.values[1] = c.allCPs.Count;
			entry.values[3] = grounded;
		}
		#endregion
		public void OnInteract(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
				if (holdingBody && !movingHoldable) PlaceHoldable();
		}

		public void OnItemAdded(ItemStackBase stack, ItemType type, int index, bool hiddenAddition)
		{
			//If this is ammo and none is equipped, equip it
			if (c.currentAmmo == -1 && type == ItemType.Ammo)
			{
				//Ammo is only allowed to be -1 if none is left
				c.SelectAmmo(index);
			}
		}





		public void OnSelectSpell(InputActionPhase phase, int spell)
		{
			if (phase != InputActionPhase.Performed) return;
			//Create a spell casting instance
			if (spell < c.spellTree.selectedNodes.Length && spell >= 0)
				if (c.spellTree.selectedNodes[spell] != null)
				{
					currentCastingSpell = c.spellTree.selectedNodes[spell].BeginCast(c);
					forceForwardHeadingToCamera = true;

				}

		}

		public void CastSpell()
		{
			currentCastingSpell.Cast();
			currentCastingSpell = null;
			forceForwardHeadingToCamera = false;
		}
		public void CancelSpellCast(bool manual)
		{
			currentCastingSpell.CancelCast(manual);
			currentCastingSpell = null;
			forceForwardHeadingToCamera = false;
		}




		event System.Action OnReleaseBowEvent;
		event System.Action OnCancelBowEvent;

		public void OnAttack(InputActionPhase phase)
		{

			if (holdingBody) PlaceHoldable();
			else if (currentCastingSpell != null) CastSpell();
			else if (c.weaponSet == PlayerController.WeaponSet.MeleeSidearm && phase == InputActionPhase.Started && c.currentMelee != -1)
			{
				if (inControl)
				{
					if (c.weaponGraphics.holdables.melee.sheathed == true)
					{
						c.StartCoroutine(DrawItem(ItemType.Melee));
					}
					else if (Usable(ItemType.Melee))
					{
						SwingSword(false);
					}

				}
				else if (activated.melee)
				{
					backSwingSword = true;
				}
			}
			else if (inControl && c.weaponSet == PlayerController.WeaponSet.BowArrow && c.currentBow != -1 && c.currentAmmo != -1)
			{
				if (phase == InputActionPhase.Started)
				{
					//Button down - start charge
					bowChargingRoutine = c.StartCoroutine(ChargeBow());
				}
				else if (phase == InputActionPhase.Canceled)
				{
					//button up - end charge
					OnReleaseBowEvent?.Invoke();
				}
			}
		}
		#region Bows
		float GetArrowSpeed(float bowCharge) => Mathf.Lerp(c.arrowSpeedRange.x, c.arrowSpeedRange.y, bowCharge);

		public void EnableBowAimView()
		{
			GameCameras.s.cameraTargetXOffset = c.shoulderViewXOffset;

			GameCameras.s.EnableFreeLookAimMode();

			c.onAimModeEnable.RaiseEvent();
		}
		public void DisableBowAimView()
		{
			GameCameras.s.cameraTargetXOffset = 0;

			GameCameras.s.DisableFreeLookAimMode();

			c.onAimModeDisable.RaiseEvent();
		}
		IEnumerator ChargeBow()
		{
			float bowCharge = 0;
			bool chargingBow = true;

			void ReleaseBow()
			{

				if (bowCharge > 0.2f)
					FireBow();

				End();
			}

			void FireBow()
			{
				//print("Charged bow to {0}", bowCharge);
				//Fire ammo
				var bow = c.weaponGraphics.holdables.bow.gameObject.GetComponent<Bow>();
				bow.ReleaseArrow(GameCameras.s.cameraTransform.forward * GetArrowSpeed(bowCharge));

				//Initialize arrow
				//Remove one of ammo used
				c.inventory.TakeItem(c.currentAmmo, ItemType.Ammo);

				//Test if ammo left for shooting
				if (c.inventory.ItemCount(c.currentAmmo, ItemType.Ammo) == 0)
				{
					Debug.Log($"Run out of arrow type {c.currentAmmo}");
					//Keep current ammo within range of avalibles
					c.SelectAmmo(c.currentAmmo);
				}
				else
				{
					c.NotchArrow();
				}
			}
			void End()
			{

				DisableBowAimView();

				forceForwardHeadingToCamera = false;
				c.animationController.lookAtPositionWeight = 0; // don't need to do others - master switch

				c.weaponGraphics.holdables.bow.gameObject.GetComponent<Animator>().SetFloat("Charge", 0);



				OnReleaseBowEvent -= ReleaseBow;
				OnCancelBowEvent -= End;
				chargingBow = false; //Ends the loop
			}



			if (c.weaponGraphics.holdables.bow.sheathed)
			{
				c.weaponGraphics.holdables.bow.gameObject.GetComponent<Bow>().InitBow(c.inventory.bow.items[c.currentBow].itemData);
				c.NotchArrow(); //TODO: Notch arrow should play animation, along with drawing bow
				yield return DrawItem(ItemType.Bow);
			}

			OnReleaseBowEvent += ReleaseBow;
			OnCancelBowEvent += End;

			forceForwardHeadingToCamera = true;


			EnableBowAimView();


			c.animationController.lookAtPositionWeight = 1;
			c.animationController.headLookAtPositionWeight = 1;
			c.animationController.eyesLookAtPositionWeight = 1;
			c.animationController.bodyLookAtPositionWeight = 1;
			c.animationController.clampLookAtPositionWeight = 0.5f; //180 degrees

			var bowAC = c.weaponGraphics.holdables.bow.gameObject.GetComponent<Animator>();
			while (chargingBow)
			{
				yield return null;
				if (Physics.Raycast(GameCameras.s.cameraTransform.position, GameCameras.s.cameraTransform.forward, out RaycastHit hit))
				{
					c.animationController.lookAtPosition = hit.point;
					c.weaponGraphics.holdables.bow.gameObject.transform.LookAt(hit.point);
				}
				else
				{
					c.animationController.lookAtPosition = GameCameras.s.cameraTransform.forward * 1000 + GameCameras.s.cameraTransform.position;
					c.weaponGraphics.holdables.bow.gameObject.transform.forward = GameCameras.s.cameraTransform.forward;
				}


				bowCharge += Time.deltaTime;
				bowCharge = Mathf.Clamp01(bowCharge);
				bowAC.SetFloat("Charge", bowCharge);
				//Update trajectory (in local space)
			}
		}
		#endregion
		#region Equipping

		IEnumerator DrawItem(ItemType type, System.Action onComplete = null)
		{

			equipping[type] = true;

			if (sprinting)
			{
				walkingType = WalkingType.Walking;
				holdingSprintKey = false; //Stop the player from immediately sprinting again
			}

			yield return c.weaponGraphics.DrawItem(type, c.transitionSet);

			equipping[type] = false;
			onComplete?.Invoke();
		}
		#endregion
		#region Swords
		void AddWeaponTrigger()
		{
			// var collider = c.weaponGraphicsController.holdables.melee.worldObject.gameObject.AddComponent<MeshCollider>();
			// collider.convex = true;
			// collider.isTrigger = true;
			var trigger = c.weaponGraphics.holdables.melee.gameObject.gameObject.GetComponent<WeaponTrigger>();
			trigger.enableTrigger = true;

			if (!trigger.inited)
			{
				trigger.Init(meleeWeapon.hitSparkEffect);
				trigger.weaponItem = meleeWeapon;
				trigger.controller = gameObject;
			}
		}

		//Play the animation and use triggers to swing the player's sword
		void SwingSword(bool backSwing)
		{
			c.rb.velocity = Vector3.zero; //Stop the player moving
			inControl = false;
			activated.melee = true;
			//swing the sword

			//While the sword is swinging, test if the player was looking towards an attackable to adjust the direction
			//Of the player's torso
			nearAttackables.Scan();
			float bestDot = -1;
			IAttackable closest = null;
			//Linear search for most direct target
			for (int i = 0; i < nearAttackables.nearObjects.Count; i++)
			{
				Vector3 direction = nearAttackables.nearObjects[i].transform.TransformPoint(nearAttackables.nearObjects[i].offset) - transform.position - nearAttackables.scanCenterOffset;
				direction.y = 0;
				direction.Normalize();
				float dot = Vector3.Dot(transform.forward, direction);
				if (dot > bestDot)
				{
					bestDot = dot;
					closest = nearAttackables.nearObjects[i];
				}
			}

			if (closest != null && bestDot > 0.5f) //Only bend body if it is clear the player is aiming at this attackable
			{
				Vector3 target = closest.transform.TransformPoint(closest.offset);
				c.animationController.lookAtPosition = target;

				float time = 0.07f;
				c.StartCoroutine(LerpNumber((x) => c.animationController.bodyLookAtPositionWeight = x, 0, 1, time));
				c.StartCoroutine(LerpNumber((x) => c.animationController.lookAtPositionWeight = x, 0, 1, time));

				c.animationController.headLookAtPositionWeight = 0;

			}
			//This is easier. Animation graphs suck
			if (backSwing)
			{
				if (meleeWeapon.isDoubleHanded)
					c.animationController.TriggerTransition(c.transitionSet.backSwingDoubleSword);
				else
					c.animationController.TriggerTransition(c.transitionSet.backSwingSword);
			}
			else
			{
				if (meleeWeapon.isDoubleHanded)
					c.animationController.TriggerTransition(c.transitionSet.swingDoubleSword);
				else
					c.animationController.TriggerTransition(c.transitionSet.swingSword);
			}



			c.onSwingStateChanged = (bool on) =>
			{
				if (on) AddWeaponTrigger();
				else RemoveWeaponTrigger(backSwing);
			};

		}
		void RemoveWeaponTrigger(bool wasBackSwing)
		{
			//Clean up the trigger detection of the sword
			var trigger = c.weaponGraphics.holdables.melee.gameObject.gameObject.GetComponent<WeaponTrigger>();
			trigger.enableTrigger = false;

			if (backSwingSword && !wasBackSwing)
			{
				SwingSword(true);
			}
			else
			{
				//Transition back to normal movement
				c.onSwingStateChanged = null;
				c.StartCoroutine(SwordUseCooldown());
			}

			backSwingSword = false;
		}
		IEnumerator SwordUseCooldown()
		{
			yield return new WaitForSeconds(c.swordUseDelay);
			inControl = true;
			activated.melee = false;
			//Go back to walking
			c.animationController.TriggerTransition(c.transitionSet.swordWalking);

			float time = 0.07f;
			c.StartCoroutine(LerpNumber((x) => c.animationController.bodyLookAtPositionWeight = x, c.animationController.bodyLookAtPositionWeight, 0, time));
			c.StartCoroutine(LerpNumber((x) => c.animationController.lookAtPositionWeight = x, c.animationController.lookAtPositionWeight, 0, time));
		}
		#endregion
		IEnumerator LerpNumber(Action<float> update, float from, float to, float time)
		{
			float t = 0;
			float invTime = 1 / time;
			while (t < 1)
			{
				t += Time.deltaTime * invTime;
				update(Mathf.Lerp(from, to, t));
				yield return null;
			}
			update(to);
		}

		#region Sidearms

		public void OnAltAttack(InputActionPhase phase)
		{
			if (!inControl) return;

			if (holdingBody)
			{
				ThrowHoldable();
			}
			else if (currentCastingSpell != null) CancelSpellCast(true);

			else if (c.weaponSet == PlayerController.WeaponSet.MeleeSidearm)
			{
				if (phase == InputActionPhase.Started)
				{
					holdingAltAttack = true;

					if (c.currentSidearm != -1)
					{
						//Equip the sidearm if it wasn't
						if (!c.sheathing.sidearm && c.weaponGraphics.holdables.sidearm.sheathed)
						{
							Debug.Log("Drawing sidearm");
							if (SidearmIsShield)
								c.StartCoroutine(DrawItem(ItemType.SideArm, RaiseShield));
							else
								c.StartCoroutine(DrawItem(ItemType.SideArm));
						}
						else if (Usable(ItemType.SideArm) && SidearmIsShield)
						{
							RaiseShield();
						}
					}
				}
				else if (phase == InputActionPhase.Canceled)
				{
					holdingAltAttack = false;

					LowerShield();
				}
			}
			else
			{
				if (phase == InputActionPhase.Performed)
				{
					OnCancelBowEvent?.Invoke();
				}
			}
		}

		public void RaiseShield()
		{
			//Only raise shield if alt attack is still being held
			if (!activated.sidearm && holdingAltAttack)
			{
				Debug.Log("Raising shield");
				activated.sidearm = true;
				c.animationController.TriggerTransition(c.transitionSet.shieldRaise);

				c.health.blockingDamage = true;
				c.health.minBlockingDot = SidearmAsShield.minBlockingDot;
			}
		}
		public void LowerShield()
		{
			if (c.currentSidearm != -1 && activated.sidearm && SidearmIsShield)
			{
				activated.sidearm = false;
				c.animationController.TriggerTransition(c.transitionSet.shieldLower);

				c.health.blockingDamage = false;
			}
		}
		#endregion
		#region Ground Detection
		/// Finds the MOST grounded (flattest y component) ContactPoint
		/// \param allCPs List to search
		/// \param groundCP The contact point with the ground
		/// \return If grounded
		public bool FindGround(out ContactPoint groundCP, out Vector3 groundNormal, Vector3 playerDirection, List<ContactPoint> allCPs)
		{
			groundCP = default;

			bool found = false;
			float dot;
			float bestDirectionDot = 1;
			groundNormal = default;
			foreach (ContactPoint cp in allCPs)
			{
				dot = Vector3.Dot(Vector3.up, cp.normal);

				//Pointing with some up direction
				if (dot > c.m_maxGroundDot)
				{

					//Get the most upwards pointing contact point

					//Also get the point that points most in the current direction the player desires to move
					float directionDot = Vector3.Dot(cp.normal, playerDirection);



					if (found == false || /*cp.normal.y > groundCP.normal.y ||*/ directionDot < bestDirectionDot)
					{
						groundCP = cp;
						bestDirectionDot = directionDot;
						found = true;
						groundNormal = cp.normal;
					}
				}
			}

			return found;
		}
		/// Find the first step up point if we hit a step
		/// \param allCPs List to search
		/// \param stepUpOffset A Vector3 of the offset of the player to step up the step
		/// \return If we found a step
		bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allCPs, ContactPoint groundCP, Vector3 currVelocity)
		{
			stepUpOffset = default(Vector3);

			//No chance to step if the player is not moving
			Vector2 velocityXZ = new Vector2(currVelocity.x, currVelocity.z);
			if (velocityXZ.sqrMagnitude < 0.0001f)
				return false;
			for (int i = 0; i < allCPs.Count; i++)// test if every point is suitable for a step up
			{
				if (ResolveStepUp(out stepUpOffset, allCPs[i], groundCP, currVelocity))
					return true;
			}
			return false;
		}

		bool debugStep = false;

		public Walking(PlayerController c, WalkingTemplate t) : base(c, t)
		{
		}



		/// Takes a contact point that looks as though it's the side face of a step and sees if we can climb it
		/// \param stepTestCP ContactPoint to check.
		/// \param groundCP ContactPoint on the ground.
		/// \param stepUpOffset The offset from the stepTestCP.point to the stepUpPoint (to add to the player's position so they're now on the step)
		/// \return If the passed ContactPoint was a step
		bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, ContactPoint groundCP, Vector3 velocity)
		{
			stepUpOffset = default;
			Collider stepCol = stepTestCP.otherCollider;

			//( 1 ) Check if the contact point normal matches that of a step (y close to 0)
			// if (Mathf.Abs(stepTestCP.normal.y) >= 0.01f)
			// {
			//     return false;
			// }

			//if the step and the ground are too close, do not count
			if (Vector3.Dot(stepTestCP.normal, Vector3.up) > c.m_maxGroundDot)
			{
				if (debugStep) Debug.Log($"Contact too close to ground normal { Vector3.Dot(stepTestCP.normal, Vector3.up)}");
				return false;
			}

			//( 2 ) Make sure the contact point is low enough to be a step
			if (!(stepTestCP.point.y - groundCP.point.y < t.maxStepHeight))
			{
				if (debugStep) Debug.Log("Contact to high");
				return false;
			}


			//( 2.5 ) Make sure the step is in the direction the player is moving
			if (Vector3.Dot(-stepTestCP.normal, velocity.normalized) < 0.01f)
			{
				if (debugStep) Debug.Log(Vector3.Dot(-stepTestCP.normal, velocity.normalized).ToString());
				//not pointing in the general direction of movement - fail
				return false;
			}

			//( 3 ) Check to see if there's actually a place to step in front of us
			//Fires one Raycast
			RaycastHit hitInfo;
			float stepHeight = groundCP.point.y + t.maxStepHeight + 0.0001f;

			Vector3 stepTestInvDir = velocity.normalized; // new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;

			//check forward based off the direction the player is walking

			Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * t.stepSearchOvershoot);
			Vector3 direction = Vector3.down;
			if (!stepCol.Raycast(new Ray(origin, direction), out hitInfo, t.maxStepHeight + t.maxStepDown))
			{
				if (debugStep) Debug.Log("Nothing to step to");
				return false;
			}

			//We have enough info to calculate the points
			Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * t.stepSearchOvershoot);
			Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundCP.point.y, stepTestCP.point.z);

			//We passed all the checks! Calculate and return the point!
			stepUpOffset = stepUpPointOffset;
			return true;
		}

		IEnumerator StepToPoint(Vector3 point, Vector3 lastVelocity)
		{
			c.rb.isKinematic = true;
			inControl = false;
			Vector3 start = transform.position;
			Vector3 pos = Vector3.zero;
			Vector2 xzStart = new Vector2(start.x, start.z);
			Vector2 xzEnd = new Vector2(point.x, point.z);
			Vector2 xz;
			float t = 0;

			while (t < 1)
			{
				t += Time.deltaTime / this.t.steppingTime;
				entry.values[2] = t;
				t = Mathf.Clamp01(t);
				//lerp y values
				//first quarter of sin graph is quick at first but slower later
				pos.y = Mathf.Lerp(start.y, point.y, Mathf.Sin(t * Mathf.PI * 0.5f));
				//lerp xz values
				xz = Vector2.Lerp(xzStart, xzEnd, t);
				pos.x = xz.x;
				pos.z = xz.y;
				transform.position = pos;
				yield return null;
			}
			entry.values[2] = 0;
			c.rb.isKinematic = false;
			c.rb.velocity = lastVelocity;
			inControl = true;
		}

		#endregion
		public override void Animate(AnimatorVariables vars)
		{

			float speed = c.inputReader.horizontalMovement.magnitude * (sprinting ? 1.5f : 1);
			if (!inControl) speed = 0;

			animator.SetBool("Idle", speed == 0);

			const float dampTime = 0.1f;
			Vector3 localVel = transform.InverseTransformDirection(c.rb.velocity).normalized * speed;

			animator.SetFloat(vars.vertical.id, localVel.z, dampTime, Time.deltaTime);
			animator.SetFloat(vars.horizontal.id, localVel.x, dampTime, Time.deltaTime);


			//c.animator.SetFloat("InputHorizontal", c.input.inputWalk.x);

			animator.SetFloat("WalkingSpeed", 1);
			animator.SetBool("IsGrounded", true);


			animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
			//animator.SetFloat(vars.groundDistance.id, c.currentHeight);
			animator.SetBool("Crouching", crouching);
			animator.SetBool("IsStrafing", true);
		}

		public void OnJump(InputActionPhase phase)
		{
			if (inControl && phase == InputActionPhase.Started && grounded)
			{
				//use acceleration to give constant upwards force regardless of mass
				// Vector3 v = c.rb.velocity;
				// v.y = c.jumpForce;
				// c.rb.velocity = v;
				c.rb.AddForce(Vector3.up * t.jumpForce, ForceMode.VelocityChange);

				c.ChangeToState(t.freefalling);
			}
		}


		public override void OnDrawGizmos()
		{
			currentCastingSpell?.OnDrawGizmos();
		}
	}
}