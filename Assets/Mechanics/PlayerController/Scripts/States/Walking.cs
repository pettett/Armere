using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Armere.Inventory.UI;
using Armere.Inventory;
using Yarn.Unity;
using System.Linq;
using Armere.UI;
using UnityEngine.Assertions;

namespace Armere.PlayerController
{


	public class Walking : MovementState<WalkingTemplate>, IInteractReceiver
	{
		public override string StateName => "Walking";

		[System.Flags]
		public enum GroundState
		{
			Grounded = 1,
			Falling = 2,
			Coyote = 4,
			Jumping = 8,
		}
		public enum WalkingType { Walking, Sprinting, Crouching, Stepping, Pushing }

		protected DialogueRunner runner => DialogueInstances.singleton.runner;


		Vector3 currentGroundNormal = default(Vector3);


		//used to continue momentum when the controller hits a stair
		Vector3 lastVelocity;

		readonly ParticleGroups movementParticles;
		//shooting variables for gizmos
		System.Text.StringBuilder entry;

		public bool forceForwardHeadingToCamera = false;

		bool isHoldingCrouchKey;
		bool isHoldingSprintKey;


		//WalkingType walkingType => 

		bool isCrouching { get => walkingType == WalkingType.Crouching; }
		bool isSprinting { get => walkingType == WalkingType.Sprinting; }

		public float walkingHeight => c.profile.m_standingHeight;

		WalkingType walkingType = WalkingType.Walking;
		public bool inControl = true;
		readonly Collider[] crouchTestColliders = new Collider[2];

		readonly PlayerWaterObject playerWaterObject;

		MeleeWeaponItemData meleeWeapon => (MeleeWeaponItemData)c.inventory.melee.items[c.currentMelee].item;


		//WEAPONS

		EquipmentSet<bool> equipping = new EquipmentSet<bool>(false, false, false);
		EquipmentSet<bool> activated = new EquipmentSet<bool>(false, false, false);

		bool backSwingSword = false;
		ShieldItemData SidearmAsShield => (ShieldItemData)c.inventory.sideArm[c.currentSidearm].item;
		bool SidearmIsShield => c.inventory.sideArm[c.currentSidearm].item is ShieldItemData;


		bool holdingAltAttack = false;

		ScanForNearT<IAttackable> nearAttackables;

		public Spell currentCastingSpell;

		GroundState ground;

		bool Coyote => ground.HasFlag(GroundState.Coyote);
		bool IsGrounded => ground.HasFlag(GroundState.Grounded);

		public Walking(PlayerMachine machine, WalkingTemplate t) : base(machine, t)
		{
			//Init sprint and crouch with correct values
			isHoldingCrouchKey = c.inputReader.IsCrouchPressed();
			isHoldingSprintKey = c.inputReader.IsSprintPressed();

			playerWaterObject = c.GetComponent<PlayerWaterObject>();
			movementParticles = c.GetComponent<ParticleGroups>();

			Assert.IsNotNull(c.animationController);
		}

		public bool Usable(ItemType t) => t switch
		{
			ItemType.SideArm => !c.sheathing.sidearm && !equipping.sidearm && !activated.sidearm && c.currentSidearm != -1,
			ItemType.Melee => !c.sheathing.melee && !equipping.melee && !activated.melee && c.currentMelee != -1,
			//Bow requires ammo and bow selection
			ItemType.Bow => !c.sheathing.bow && !equipping.bow && !activated.bow && c.currentBow != -1 && c.currentAmmo != -1,
			_ => false,
		};


		public bool Using(ItemType t) => t switch
		{
			ItemType.SideArm => !c.sheathing.sidearm && !equipping.sidearm && activated.sidearm && c.currentSidearm != -1,
			ItemType.Melee => !c.sheathing.melee && !equipping.melee && activated.melee && c.currentMelee != -1,
			//Bow requires ammo and bow selection
			ItemType.Bow => !c.sheathing.bow && !equipping.bow && activated.bow && c.currentBow != -1 && c.currentAmmo != -1,
			_ => false,
		};


		public override void Start()
		{
			entry = DebugMenu.CreateEntry("Player"/* , "Velocity: {0:0.0} Contact Point Count {1} Stepping Progress {2} On Ground {3}", 0, 0, 0, false */);



			//c.controllingCamera = false; // debug for camera parallel state

			//c.transform.up = Vector3.up;
			c.animationController.enableFeetIK = true;
			c.rb.isKinematic = false;

			animator.applyRootMotion = false;

			t.onPlayerInventoryItemAdded.onItemAddedEvent += OnItemAdded;

			c.health.onTakeDamage += OnTakeDamage;
			c.health.onBlockDamage += OnBlockDamage;


			nearAttackables = machine.GetState<ScanForNearT<IAttackable>>();
			nearAttackables.updateEveryFrame = false;

			//Add input reader events

			c.inputReader.sprintEvent += OnSprint;
			c.inputReader.crouchEvent += OnCrouch;
			c.inputReader.attackEvent += OnAttack;


			c.inputReader.equipBowEvent += OnAttackBow;
			c.inputReader.altAttackEvent += OnAltAttack;
			c.inputReader.jumpEvent += OnJump;
			c.inputReader.selectSpellEvent += OnSelectSpell;

		}

		public override void End()
		{
			if (currentCastingSpell != null)
			{
				CancelSpellCast(false);
			}

			transform.SetParent(null, true);
			DebugMenu.RemoveEntry(entry);

			c.health.onTakeDamage -= OnTakeDamage;
			c.health.onBlockDamage -= OnBlockDamage;


			//make sure the collider is left correctly
			c.collider.height = walkingHeight;
			c.collider.center = Vector3.up * c.collider.height * 0.5f;


			t.onPlayerInventoryItemAdded.onItemAddedEvent -= OnItemAdded;

			//Remove input reader events

			c.inputReader.sprintEvent -= OnSprint;
			c.inputReader.crouchEvent -= OnCrouch;
			c.inputReader.attackEvent -= OnAttack;
			c.inputReader.equipBowEvent -= OnAttackBow;
			c.inputReader.altAttackEvent -= OnAltAttack;
			c.inputReader.jumpEvent -= OnJump;
			c.inputReader.selectSpellEvent -= OnSelectSpell;

		}


		public void OnCrouch(InputActionPhase phase)
		{
			isHoldingCrouchKey = InputReader.PhaseMeansPressed(phase);
		}
		public void OnSprint(InputActionPhase phase)
		{
			isHoldingSprintKey = InputReader.PhaseMeansPressed(phase);
		}


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
					new UIKeyPromptGroup.KeyPrompt("Rest", InputReader.GroundActionMapActions.Attack),
					new UIKeyPromptGroup.KeyPrompt("Roast Marshmellow", InputReader.GroundActionMapActions.AltAttack)
					);

				if (machine.TryGetState<Interact>(out var s))
					s.End();



				//TODO: Make sitting a separate movement state
				c.StartCoroutine(c.UnEquipAll(() =>
				{
					c.animationController.TriggerTransition(c.transitionSet.startSitting);

					cameras.playerTrackingOffset = 0.5f;

					bool inDialogue = false;
					void OnMovementCancelSit(Vector2 movement) => CancelSit();

					void CancelSit()
					{
						if (inDialogue) return;
						//Return to walking
						c.animationController.TriggerTransition(c.transitionSet.stopSitting);
						inControl = true;
						UIKeyPromptGroup.singleton.RemovePrompts();

						cameras.playerTrackingOffset = cameras.defaultTrackingOffset;

						c.inputReader.actionEvent -= EnterSleepDialogue;
						c.inputReader.movementEvent -= OnMovementCancelSit;
						s?.Start();
					}

					void TriggerTimeChange(float newTime)
					{
						c.StartCoroutine(c.FadeToActions(
							onFullFade: () =>
							{
								t.changeTimeEventChannel.RaiseEvent(newTime);
							}
						));
					}

					void EnterSleepDialogue(InputActionPhase phase)
					{
						if (phase == InputActionPhase.Started)
						{
							if (!inDialogue)
							{
								//Run the dialogue
								runner.Add(restPoint.dialogue);
								runner.StartDialogue("Start");

								runner.AddCommandHandler("Morning", _ =>
								{
									TriggerTimeChange(6);
								});
								runner.AddCommandHandler("Noon", _ =>
								{
									TriggerTimeChange(12);
								});
								runner.AddCommandHandler("Night", _ =>
								{
									TriggerTimeChange(24);
								});
								runner.AddCommandHandler("None", _ =>
								{
									Debug.Log("None");
								});

								DialogueInstances.singleton.ui.onDialogueEnd.AddListener(() =>
							  {
								  inDialogue = false;
								  runner.Clear();
								  runner.RemoveCommandHandler("Morning");
								  runner.RemoveCommandHandler("Noon");
								  runner.RemoveCommandHandler("Night");
								  runner.RemoveCommandHandler("None");
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
			Vector3 direction = Vector3.ProjectOnPlane(transform.position - attacker.transform.position, c.WorldDown);
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
		public void HoldHoldable(HoldableBody body)
		{
			StartSpell(new PlayerHoldableInteraction(this, body));
		}

		readonly RaycastHit[] waterHits = new RaycastHit[2];
		#region Movement

		public Vector3 GetDesiredVelocity(Vector3 playerInputDirection, float movementSpeed, float speedScalar)
		{
			if (forceForwardHeadingToCamera)
			{
				Vector3 forward = Vector3.ProjectOnPlane(cameras.cameraTransform.forward, c.WorldDown);
				transform.forward = forward;

				//Include speed scalar from water
				var desiredVelocity = playerInputDirection * movementSpeed * speedScalar;
				//Rotate the velocity based on ground
				desiredVelocity = Quaternion.AngleAxis(0, currentGroundNormal) * desiredVelocity;

				return desiredVelocity;
			}
			else if (playerInputDirection.sqrMagnitude > 0.1f)
			{
				Quaternion walkingAngle;

				if (cameras.m_UpdatingCameraDirection)
				{
					Vector3 dir = Vector3.ProjectOnPlane(cameras.FocusTarget - transform.position, c.WorldDown);
					walkingAngle = Quaternion.LookRotation(dir, c.WorldUp);
				}
				else
				{
					walkingAngle = Quaternion.LookRotation(playerInputDirection, c.WorldUp);
				}




				float angle = Quaternion.Angle(transform.rotation, walkingAngle);
				//If not forcing heading, rotate towards walking
				transform.rotation = Quaternion.RotateTowards(transform.rotation, walkingAngle, Time.deltaTime * t.GetPropulsion(ground).rotationSpeed);
				//Debug.Log(angle);
				if (angle > 170 && c.rb.velocity.sqrMagnitude > 0.5f)
				{
					//Perform a 180 manuever

					return Vector3.zero;
					//c.StartCoroutine(Perform180());
				}
				else if (angle > 30f)
				{
					//Only allow the player to walk forward if they have finished turning to the direction
					//But do allow the player to run at a slight angle
					return Vector3.zero;
				}
				else
				{
					//Let the player move in the direction they are pointing

					//scale required velocity by current speed
					//only allow sprinting if the play is moving forward

					//Include speed scalar from water
					var desiredVelocity = playerInputDirection * movementSpeed * speedScalar;
					//Rotate the velocity based on ground
					return Quaternion.AngleAxis(0, currentGroundNormal) * desiredVelocity;

				}
			}
			else
			{
				//No movement
				return Vector3.zero;
			}
		}
		// IEnumerator Perform180()
		// {
		// 	c.animationController.TriggerTransition(c.transitionSet.sword180);
		// 	c.rb.velocity = Vector3.zero;
		// 	//c.animator.runtimeAnimatorController.animationClips
		// 	inControl = false;
		// 	c.animator.applyRootMotion = true;
		// 	yield return null;

		// 	yield return new WaitForSeconds(c.animator.GetNextAnimatorClipInfo(0)[0].clip.length / 2 - Time.deltaTime);

		// 	if (c.weaponGraphics.holdables.melee.sheathed)
		// 	{
		// 		c.animationController.TriggerTransition(c.transitionSet.freeMovement);
		// 	}

		// 	animator.SetFloat(c.transitionSet.vertical.id, 1);
		// 	//transform.forward = -transform.forward;
		// 	c.animator.applyRootMotion = false;
		// 	inControl = true;

		// }

		public override void Update()
		{
			currentCastingSpell?.Update();



			float speed = c.inputReader.horizontalMovement.magnitude * (isSprinting ? 1.5f : 1);
			if (!inControl) speed = 0;

			animator.SetBool("Idle", speed == 0);

			const float dampTime = 0.1f;
			Vector3 localVel = transform.InverseTransformDirection(c.rb.velocity).normalized * speed;

			animator.SetFloat(c.transitionSet.vertical.id, localVel.z, dampTime, Time.deltaTime);
			animator.SetFloat(c.transitionSet.horizontal.id, localVel.x, dampTime, Time.deltaTime);


			//c.animator.SetFloat("InputHorizontal", c.input.inputWalk.x);

			animator.SetFloat("WalkingSpeed", 1);
			animator.SetBool("IsGrounded", IsGrounded);


			animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
			//animator.SetFloat(vars.groundDistance.id, c.currentHeight);
			animator.SetBool("Crouching", isCrouching);
			animator.SetBool("IsStrafing", true);

		}

		public Vector3 GetSlopeForce(Vector3 movement, Vector3 normal)
		{
			float dot = Vector3.Dot(c.WorldUp, normal);
			if (dot > c.profile.m_maxGroundSlopeDot)
			{
				float slope = Mathf.Clamp01(1 - dot);

				Vector3 floorDirection = Vector3.ProjectOnPlane(normal, c.WorldDown);


				float direction = Vector3.Dot(floorDirection, movement);

				//Debug.Log($"{slope}, {direction}");

				//negative direction, going up slope
				//positive direction, going down slope

				return c.WorldUp * t.slopeForce * -direction * slope;
			}

			return Vector3.zero;

		}

		/// <summary>
		///	Sheath all player held weapons
		/// </summary>
		/// <returns>true if everything is fully sheathed</returns>
		bool SheathAll()
		{
			//Do not allow sprinting while a weapon is equipped, so wait until
			//The weapon for the set is sheathed before allowing sprint

			if (!c.sheathing.melee && !c.weaponGraphics.holdables.melee.sheathed)
			{
				//Will only operate is sword exists
				c.StartCoroutine(c.SheathItem(ItemType.Melee));
			}

			//Will only operate if sidearm exists
			if (!c.sheathing.sidearm && !c.weaponGraphics.holdables.sidearm.sheathed)
			{
				//Will only operate is sword exists
				c.StartCoroutine(c.SheathItem(ItemType.SideArm));
			}

			if (!c.sheathing.bow && !c.weaponGraphics.holdables.bow.sheathed)
			{
				//Will only operate if bow exists
				c.StartCoroutine(c.SheathItem(ItemType.Bow));
				//Reset to default weapon set
				CancelSpellCast(true);
			}

			bool allSheathed = !c.sheathing.melee && !c.sheathing.sidearm && !c.sheathing.bow;

			return allSheathed;

		}

		void AttemptSprint()
		{
			bool shouldSprint = isHoldingSprintKey;

			if (shouldSprint)
			{
				if (cameras.m_UpdatingCameraDirection)
				{
					//Do not sprint while focusing
					shouldSprint = false;
				}
				else
				{
					SheathAll();
				}
			}


			//If no longer pressing the button return to normal movement
			if (shouldSprint)
				walkingType = WalkingType.Sprinting;
			else
				walkingType = WalkingType.Walking;
		}
		public struct WaterData
		{
			/// <summary>
			/// Is water present at all?
			/// </summary>
			public bool inWater;
			/// <summary>
			/// Scaled between 0 and 1
			/// </summary>
			[Range(0, 1)] public float depth;
			/// <summary>
			/// Should the controller move to swimming state
			/// </summary>
			public bool shouldSwim;
		}
		public WaterData GetWaterInfo()
		{
			WaterData data = default;
			//Test for water
			if (playerWaterObject?.currentFluid != null)
			{
				data.inWater = true;

				//Find depth of water
				//Buffer of two: One for water surface, one for water base
				float heightOffset = 2;
				//Cannot raycast up as raycasts do not hit back surface of shapes
				int hits = Physics.RaycastNonAlloc(
					transform.position + c.WorldUp * heightOffset,
					c.WorldDown, waterHits,
					c.profile.maxWaterStrideDepth + heightOffset,
					c.m_groundLayerMask | c.m_waterLayerMask,
					QueryTriggerInteraction.Collide);

				if (hits == 2)
				{
					Debug.Log("two hits");
					if (waterHits[1].collider.GetComponentInParent<WaterController>() != null)
					{
						//Hit water and ground
						float depth = waterHits[0].distance - waterHits[1].distance;

						float scaledDepth = depth / c.profile.maxWaterStrideDepth;
						if (scaledDepth > 1)
						{
							//Start swimming
							Debug.Log("over max depth, swimming");
							//c.ChangeToState(t.swimming);
							data.shouldSwim = true;
							return data;
						}

						//Striding through water
						//Slow speed of walk
						//Full depth walks at half speed
						data.depth = scaledDepth;// (1 - scaledDepth) * t.maxStridingDepthSpeedScalar + (1 - t.maxStridingDepthSpeedScalar);
						return data;

					}

					data.shouldSwim = true;
					return data;

				}
				else if (hits == 1 && waterHits[0].collider.GetComponentInParent<WaterController>() != null)
				{

					//Start swimming
					Debug.Log("only one hit, swimming");
					data.shouldSwim = true;
					return data;
				}
				//I dont think it should get here
				Debug.LogWarning("Unexpected water result");
				return data;
			}

			data.inWater = false;
			return data;
		}

		public float GetSpeedScalar(WaterData waterData)
		{
			float speedScalar = 1;

			if (waterData.inWater)
			{
				return (1 - waterData.depth) * t.maxStridingDepthSpeedScalar + (1 - t.maxStridingDepthSpeedScalar);
			}

			return speedScalar;
		}
		public float GetMaxAcceleration()
		{
			if (IsGrounded) return t.groundPropulsion.maxAcceleration;
			else return t.airPropulsion.maxAcceleration;
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


			//Debug.Log($"Average turning {averageTurning}");

			Vector3 inputDirectionWS = PlayerInputUtility.WorldSpaceFlatInput(c);

			bool wasGrounded = IsGrounded;

			bool onGround = FindGround(out currentGroundNormal, inputDirectionWS, c.allCPs);


			bool jumping = ground.HasFlag(GroundState.Jumping);

			if (walkingType == WalkingType.Stepping)
			{
				//Just finished stepping, does not could as landing
				walkingType = WalkingType.Walking;
				ground = GroundState.Grounded;
				onGround = true;

			}
			else if (!onGround && wasGrounded && !jumping)
			{
				ground = GroundState.Coyote | GroundState.Falling;
				c.StartCoroutine(DoCoyote(t.coyoteTime));
			}
			else if (onGround && !IsGrounded)
			{
				if (jumping)
				{
					//If on ground and jumping, test to see if we should move back onto the ground or continue in jump mode
					Vector3 vel = c.rb.velocity;

					//Only allow to return to ground if velocity is not in opposite ish direction to ground
					float dot = Vector3.Dot(vel, currentGroundNormal);
					if (dot < 0.5f || vel.sqrMagnitude < 0.5f)
						OnLanded();
				}
				else
				{
					OnLanded();
				}
			}

			if (IsGrounded && FindPushTarget(out var push, inputDirectionWS, c.allCPs))
			{
				Debug.Log($"Pushing {push.otherCollider}");
				walkingType = WalkingType.Pushing;
			}



			c.animationController.enableFeetIK = IsGrounded;

			AttemptSprint();

			//List<ContactPoint> groundCPs = new List<ContactPoint>();

			WaterData water = GetWaterInfo();

			if (water.shouldSwim)
			{
				machine.ChangeToState(t.swimming);
				return;
			}

			float currentMovementSpeed = t.GetSpeeds(walkingType).movementSpeed;

			Vector3 desiredVelocity = GetDesiredVelocity(inputDirectionWS, currentMovementSpeed, GetSpeedScalar(water));

			//Stepping also allowed when in air as it allows players to reach surfaces better when jumping
			Vector3 groundPoint = transform.position;

			Vector3 groundNormal = IsGrounded switch
			{
				true => currentGroundNormal,
				false => c.WorldUp,
			};

			//step up onto the stair, reseting the velocity to what it was
			if (FindStep(out Vector3 stepUpOffset, c.allCPs, groundPoint, groundNormal, desiredVelocity))
			{
				Debug.Log("Stepping");
				c.StartCoroutine(StepToPoint(transform.position + stepUpOffset, lastVelocity));
			}

			if (isHoldingCrouchKey)
			{
				c.collider.height = t.crouchingHeight;
			}
			else if (isCrouching)
			{
				//crouch button not pressed but still crouching
				Vector3 p1 = transform.position + c.WorldUp * walkingHeight * 0.05F;
				Vector3 p2 = transform.position + c.WorldUp * walkingHeight;
				Physics.OverlapCapsuleNonAlloc(p1, p2, c.collider.radius, crouchTestColliders, c.m_groundLayerMask, QueryTriggerInteraction.Ignore);
				if (crouchTestColliders[1] == null)
					//There is no collider intersecting other then the player
					walkingType = WalkingType.Walking;
				else crouchTestColliders[1] = null;
			}

			if (!isCrouching)
				c.collider.height = walkingHeight;

			c.collider.center = Vector3.up * c.collider.height * 0.5f;


			Vector3 movementForce = Vector3.ProjectOnPlane(desiredVelocity - c.rb.velocity, groundNormal);


			movementForce = Vector3.ClampMagnitude(movementForce, GetMaxAcceleration() * Time.fixedDeltaTime);

			//Add slope boost or reduction
			movementForce += GetSlopeForce(movementForce, currentGroundNormal);


			//rotate the target based on the ground the player is standing on


			if (IsGrounded)
			{
				c.rb.AddForce(-currentGroundNormal * t.groundClamp);
				//Stick to the ground


			}
			c.rb.AddForce(movementForce, ForceMode.VelocityChange);

			lastVelocity = velocity;

			if (DebugMenu.menuEnabled)
			{
				entry.Clear();
				entry.AppendLine($"Velocity: {c.rb.velocity.magnitude:0.0}, Contact Point Count: {c.allCPs.Count}");
				entry.AppendLine($"Ground: {ground}, Walk: {walkingType}");
			}
		}
		#endregion

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
			if (spell < c.spellTree.selectedNodes.Length && spell >= 0 && c.spellTree.selectedNodes[spell].canBeUsed)
			{
				StartSpell(c.spellTree.selectedNodes[spell].Use().BeginCast(this));
			}
		}
		public void StartSpell(Spell spell)
		{
			if (currentCastingSpell != null) CancelSpellCast(true);
			currentCastingSpell = spell;

			forceForwardHeadingToCamera = true;
			currentCastingSpell.Begin();
		}

		public void CancelSpellCast(bool manual)
		{
			currentCastingSpell.EndCast(manual);
			currentCastingSpell = null;
			forceForwardHeadingToCamera = false;
		}

		public void SheathBow()
		{
			c.StartCoroutine(c.UnEquipAll());
			c.weaponSet = PlayerController.WeaponSet.MeleeSidearm;
		}

		public void OnAttack(InputActionPhase phase)
		{
			if (!(currentCastingSpell?.CanAttackWhileInUse ?? true))
			{
				return;
			}


			else if (phase == InputActionPhase.Started && c.currentMelee != -1)
			{
				if (c.weaponSet != PlayerController.WeaponSet.MeleeSidearm)
				{
					SheathBow();
				}

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
		}
		bool bowKeyHeld = false;

		public void OnAttackBow(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Performed) bowKeyHeld = true;
			else if (phase == InputActionPhase.Canceled) bowKeyHeld = false;


			if (inControl)
			{

				if (phase == InputActionPhase.Performed)
				{
					if (c.currentBow != -1 && c.currentAmmo != -1)
					{
						//Button down - start charge
						//Player bow is kinda spell
						//Start the spell at this point so that it can be canceled while items are sheathing
						StartSpell(new PlayerBowAttack(c, this));

						if (c.weaponSet != PlayerController.WeaponSet.BowArrow)
						{
							c.StartCoroutine(c.UnEquipAll(() =>
							{
								c.weaponSet = PlayerController.WeaponSet.BowArrow;
								if (currentCastingSpell != null) //Change the bow is already cancelled
									((PlayerBowAttack)currentCastingSpell).BeginCharge();

							}));
						}
						else
						{
							((PlayerBowAttack)currentCastingSpell).BeginCharge();
						}
					}
					else
					{
						//TODO: Allow player to select a bow
						Debug.Log("No Bow Equipped");
					}
				}
			}
		}

		#region Events

		public void OnJumped()
		{
			movementParticles.PlayGroup("Jump");
			t.onJump.Invoke();
		}
		public void OnLanded()
		{
			ground = GroundState.Grounded;
			movementParticles.PlayGroup("Land");
		}


		#endregion
		#region Equipping

		public IEnumerator DrawItem(ItemType type, System.Action onComplete = null)
		{

			equipping[type] = true;

			if (isSprinting)
			{
				walkingType = WalkingType.Walking;
				isHoldingSprintKey = false; //Stop the player from immediately sprinting again
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
			trigger.destroyGrassInBounds = t.destroyGrassInBoundsEventChannel;
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
				direction = Vector3.ProjectOnPlane(direction, c.WorldDown);
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
				//TODO: Make player bend down

				// Vector3 target = closest.transform.TransformPoint(closest.offset);
				// c.animationController.lookAtPosition = target;

				// const float time = 0.07f;

				// LeanTween.value(c.gameObject, 0, 1, time).setOnUpdate(x => c.animationController.weights.bodyWeight = x);
				// LeanTween.value(c.gameObject, 0, 1, time).setOnUpdate(x => c.animationController.weights.weight = x);

				// c.animationController.weights.weight = 0;

			}
			//This is easier. Animation graphs suck


			c.animationController.TriggerTransition((backSwing, meleeWeapon.isDoubleHanded) switch
			{
				(true, true) => c.transitionSet.backSwingDoubleSword,
				(true, false) => c.transitionSet.backSwingSword,
				(false, true) => c.transitionSet.swingDoubleSword,
				(false, false) => c.transitionSet.swingSword,
			});

			void End()
			{
				RemoveWeaponTrigger(backSwing);

				c.animationController.onSwingStart -= AddWeaponTrigger;
				c.animationController.onSwingEnd -= End;
			}

			c.animationController.onSwingStart += AddWeaponTrigger;
			c.animationController.onSwingEnd += End;


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

				c.StartCoroutine(SwordUseCooldown());
			}

			backSwingSword = false;
		}
		IEnumerator SwordUseCooldown()
		{
			yield return new WaitForSeconds(t.swordUseDelay);
			inControl = true;
			activated.melee = false;
			//Go back to walking
			c.animationController.TriggerTransition(c.transitionSet.swordWalking);

			//float time = 0.07f;

			// LeanTween.value(
			// 	c.gameObject, c.animationController.weights.weight, 0, time
			// 	).setOnUpdate(x => c.animationController.weights.weight = x);
			// LeanTween.value(
			// 	c.gameObject, c.animationController.weights.bodyWeight, 0, time
			// 	).setOnUpdate(x =>c.animationController.weights.bodyWeight = x);
		}
		#endregion
		#region Sidearms

		public void OnAltAttack(InputActionPhase phase)
		{
			if (!inControl) return;

			//If cancel the spell on alt attack, cancel. if null, do not cancel nothing
			else if (currentCastingSpell?.CancelOnAltAttack ?? false) CancelSpellCast(true);

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
		public bool FindGround(out Vector3 groundNormal, Vector3 playerDirection, List<ContactPoint> allCPs)
		{
			groundNormal = default;
			bool found = false;
			float bestDirectionDot = 1;
			foreach (ContactPoint cp in allCPs)
			{
				//Pointing with some up direction
				if (c.profile.CanWalkOn(c.WorldUp, cp.normal))
				{
					//Get the most upwards pointing contact point
					//Also get the point that points most in the current direction the player desires to move
					float directionDot = Vector3.Dot(cp.normal, playerDirection);
					if (found == false || /*cp.normal.y > groundCP.normal.y ||*/ directionDot < bestDirectionDot)
					{
						groundNormal = cp.normal;
						bestDirectionDot = directionDot;
						found = true;
					}
				}
			}

			if (!found && IsGrounded && inControl)
			{
				//Raycast down to see if we have simply walked off a small ledge
				float startHeight = 1f;
				if (Physics.SphereCast(
					transform.position + c.WorldUp * startHeight,
					c.collider.radius, c.WorldDown,
					out var hit, startHeight + t.maxStepDown,
					c.m_groundLayerMask, QueryTriggerInteraction.Ignore) &&
					//Is walkable
					c.profile.CanWalkOn(hit.normal, c.WorldUp) &&
					//Is actually a step down
					hit.point.y < transform.position.y - t.minStepHeight

					)
				{
					Debug.Log("Stepping down");
					c.StartCoroutine(StepToPoint(hit.point, c.rb.velocity));

					groundNormal = hit.normal;
					return true;
				}
			}

			return found;
		}
		public bool FindPushTarget(out ContactPoint pushTarget, Vector3 playerDirection, List<ContactPoint> allCPs)
		{
			foreach (var item in allCPs)
			{
				if (item.otherCollider.attachedRigidbody is Rigidbody rb && !rb.isKinematic && rb.mass >= t.minPushMass && rb.mass < t.maxPushMass)
				{
					//Not null	
					float dot = Vector3.Dot(item.normal, playerDirection);

					if (dot < -0.9f)
					{
						pushTarget = item;
						return true;
					}
				}

			}
			pushTarget = default;
			return false;
		}

		/// Find the first step up point if we hit a step
		/// \param allCPs List to search
		/// \param stepUpOffset A Vector3 of the offset of the player to step up the step
		/// \return If we found a step
		bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allCPs, Vector3 groundPoint, Vector3 groundNormal, Vector3 currVelocity)
		{
			stepUpOffset = default(Vector3);
			//No chance to step if the player is not moving
			if (new Vector2(currVelocity.x, currVelocity.z).sqrMagnitude < 0.0001f)
				return false;
			for (int i = 0; i < allCPs.Count; i++)// test if every point is suitable for a step up
				if (ResolveStepUp(out stepUpOffset, allCPs[i], groundPoint, groundNormal, currVelocity))
					return true;
			return false;
		}



		/// Takes a contact point that looks as though it's the side face of a step and sees if we can climb it
		/// \param stepTestCP ContactPoint to check.
		/// \param groundCP ContactPoint on the ground.
		/// \param stepUpOffset The offset from the stepTestCP.point to the stepUpPoint (to add to the player's position so they're now on the step)
		/// \return If the passed ContactPoint was a step
		bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, Vector3 groundPoint, Vector3 groundNormal, Vector3 velocity)
		{
			stepUpOffset = default;
			Collider stepCol = stepTestCP.otherCollider;

			//FIXME: Only works in normal gravity

			//( 1 ) Check if the contact point normal matches that of a step (y close to 0)
			// if (Mathf.Abs(stepTestCP.normal.y) >= 0.01f)
			// {
			//     return false;
			// }


			//if the step and the ground are too close, do not count
			if (Vector3.Dot(stepTestCP.normal, groundNormal) > 0.9 && Vector3.Dot(groundNormal, c.WorldUp) < 0.5f)
			{
				if (t.debugStep) Debug.Log($"Contact too similar to ramp");
				return false;
			}


			float height = stepTestCP.point.y - groundPoint.y;
			//( 2 ) Make sure the contact point is low enough to be a step
			if (height < t.minStepHeight || height >= t.maxStepHeight)
			{
				if (t.debugStep) Debug.Log($"Contact to high or low, {height}");
				return false;
			}


			//( 2.5 ) Make sure the step is in the direction the player is moving
			if (Vector3.Dot(-stepTestCP.normal, velocity.normalized) < 0.01f)
			{
				if (t.debugStep) Debug.Log(Vector3.Dot(-stepTestCP.normal, velocity.normalized).ToString());
				//not pointing in the general direction of movement - fail
				return false;
			}

			//( 3 ) Check to see if there's actually a place to step in front of us
			//Fires one Raycast
			RaycastHit hitInfo;
			float stepHeight = groundPoint.y + t.maxStepHeight + 0.0001f;

			Vector3 stepTestInvDir = velocity.normalized; // new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;

			//check forward based off the direction the player is walking

			Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * t.stepSearchOvershoot);
			Vector3 direction = c.WorldDown;
			if (!stepCol.Raycast(new Ray(origin, direction), out hitInfo, t.maxStepHeight + t.maxStepDown))
			{
				if (t.debugStep) Debug.Log("Nothing to step to");
				return false;
			}


			if (!c.profile.CanWalkOn(hitInfo.normal, c.WorldUp))
			{
				if (t.debugStep) Debug.Log("Cannot walk onto destination");
				return false;
			}

			//We have enough info to calculate the points
			Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * t.stepSearchOvershoot);
			Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundPoint.y, stepTestCP.point.z);

			//We passed all the checks! Calculate and return the point!
			stepUpOffset = stepUpPointOffset;
			return true;
		}

		IEnumerator StepToPoint(Vector3 point, Vector3 lastVelocity)
		{
			c.rb.isKinematic = true;
			inControl = false;
			walkingType = WalkingType.Stepping;

			Vector3 start = transform.position;
			Vector3 pos = Vector3.zero;
			Vector2 xzStart = new Vector2(start.x, start.z);
			Vector2 xzEnd = new Vector2(point.x, point.z);
			Vector2 xz;
			float t = 0;
			float dist = Vector3.Distance(point, start);


			float time = dist / this.t.steppingSpeed;
			float rate = 1 / time;

			//Doing a step guarantees groundedness
			//So make grounded now in case of coming in from falling
			ground = GroundState.Grounded;

			if (start.y < point.y)
			{
				//Play the step up animation for the time that this will take to execute
				c.animationController.TriggerTransition(c.transitionSet.stepUp);
				yield return null;
				c.animationController.OverrideCurrentAnimationLength(c.transitionSet.animationSpeed, time, 0);
			}
			c.animationController.movePelvis = false;

			while (t < 1)
			{
				t += Time.deltaTime * rate;
				//entry.values[2] = t;
				t = Mathf.Clamp01(t);
				//lerp y values
				//first quarter of sin graph is quick at first but slower later
				pos.y = Mathf.Lerp(start.y, point.y, 1 - Mathf.Cos(t * Mathf.PI * 0.5f));
				//lerp xz values
				xz = Vector2.Lerp(xzStart, xzEnd, t);
				pos.x = xz.x;
				pos.z = xz.y;
				transform.position = pos;
				yield return null;
			}
			c.animationController.movePelvis = true;
			//entry.values[2] = 0;
			c.rb.isKinematic = false;
			c.rb.velocity = lastVelocity;

			inControl = true;

		}

		#endregion

		IEnumerator DoCoyote(float time)
		{
			yield return new WaitForSeconds(time);
			ground &= ~GroundState.Coyote;
		}

		public void OnJump(InputActionPhase phase)
		{
			if (inControl && phase == InputActionPhase.Started && (Coyote || IsGrounded))
			{
				//use acceleration to give constant upwards force regardless of mass
				// Vector3 v = c.rb.velocity;
				// v.y = c.jumpForce;
				// c.rb.velocity = v;

				Vector2 jump = t.GetSpeeds(walkingType).twoDJumpForce;

				Vector3 inputDirectionWS = PlayerInputUtility.WorldSpaceFlatInput(c);

				Vector3 jumpVelocity = inputDirectionWS;
				jumpVelocity *= jump.x;
				jumpVelocity += c.WorldUp * jump.y;
				c.rb.velocity = jumpVelocity;

				ground = GroundState.Falling | GroundState.Jumping;

				OnJumped();

				//c.ChangeToState(t.freefalling);
			}
		}


		public override void OnDrawGizmos()
		{
			for (int i = 0; i < c.allCPs.Count; i++)
			{
				Gizmos.color = c.profile.CanWalkOn(c.allCPs[i].normal, c.WorldUp) ? Color.green : Color.red;
				Gizmos.DrawLine(c.allCPs[i].point, c.allCPs[i].point + c.allCPs[i].normal);
			}


			currentCastingSpell?.OnDrawGizmos();
		}
	}
}