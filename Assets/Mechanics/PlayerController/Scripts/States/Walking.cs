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
namespace Armere.PlayerController
{


	public class Walking : MovementState<WalkingTemplate>, IInteractReceiver
	{
		public override string StateName => "Walking";


		public enum WalkingType { Walking, Sprinting, Crouching }

		protected DialogueRunner runner => DialogueInstances.singleton.runner;


		Vector3 currentGroundNormal = default(Vector3);


		//used to continue momentum when the controller hits a stair
		Vector3 lastVelocity;


		//shooting variables for gizmos
		System.Text.StringBuilder entry;

		public bool forceForwardHeadingToCamera = false;
		bool isGrounded;
		bool isHoldingCrouchKey;
		bool isHoldingSprintKey;


		//WalkingType walkingType => 

		bool isCrouching { get => walkingType == WalkingType.Crouching; }
		bool isSprinting { get => walkingType == WalkingType.Sprinting; }

		public float walkingHeight => c.walkingHeight;

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

		bool debugStep = false;
		bool coyote = false;

		public Walking(PlayerController c, WalkingTemplate t) : base(c, t)
		{
			//Init sprint and crouch with correct values
			isHoldingCrouchKey = c.inputReader.IsCrouchPressed();
			isHoldingSprintKey = c.inputReader.IsSprintPressed();

			playerWaterObject = c.GetComponent<PlayerWaterObject>();
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


			nearAttackables = (ScanForNearT<IAttackable>)c.GetParallelState(typeof(ScanForNearT<IAttackable>));
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
					new UIKeyPromptGroup.KeyPrompt("Rest", InputReader.attackAction),
					new UIKeyPromptGroup.KeyPrompt("Roast Marshmellow", InputReader.altAttackAction)
					);

				if (c.TryGetParallelState<Interact>(out var s))
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
			StartSpell(new PlayerHoldableInteraction(c, this, body));
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
				c.WorldDown, waterHits,
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
						speedScalar = (1 - scaledDepth) * t.maxStridingDepthSpeedScalar + (1 - t.maxStridingDepthSpeedScalar);
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
				Vector3 forward = Vector3.ProjectOnPlane(cameras.cameraTransform.forward, c.WorldDown);
				transform.forward = forward;

				//Include speed scalar from water
				desiredVelocity = playerDirection * movementSpeed * speedScalar;
				//Rotate the velocity based on ground
				desiredVelocity = Quaternion.AngleAxis(0, currentGroundNormal) * desiredVelocity;
			}
			else if (playerDirection.sqrMagnitude > 0.1f)
			{
				Quaternion walkingAngle;

				if (cameras.m_UpdatingCameraDirection)
				{
					Vector3 dir = Vector3.ProjectOnPlane(cameras.FocusTarget - transform.position, c.WorldDown);
					walkingAngle = Quaternion.LookRotation(dir, c.WorldUp);
				}
				else
				{
					walkingAngle = Quaternion.LookRotation(playerDirection, c.WorldUp);
				}




				float angle = Quaternion.Angle(transform.rotation, walkingAngle);
				//Debug.Log(angle);
				if (angle > 170 && c.rb.velocity.sqrMagnitude > 0.5f)
				{
					//Perform a 180 manuever

					desiredVelocity = Vector3.zero;
					//c.StartCoroutine(Perform180());
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
			animator.SetBool("IsGrounded", true);


			animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
			//animator.SetFloat(vars.groundDistance.id, c.currentHeight);
			animator.SetBool("Crouching", isCrouching);
			animator.SetBool("IsStrafing", true);

		}

		public Vector3 GetSlopeForce(Vector3 movement, Vector3 normal)
		{
			float dot = Vector3.Dot(c.WorldUp, normal);
			if (dot > c.m_maxGroundSlopeDot)
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

			Vector3 playerDirection = PlayerInputUtility.WorldSpaceFlatInput(c, c.WorldUp);

			bool wasGrounded = isGrounded;

			isGrounded = FindGround(out var groundCP, out currentGroundNormal, playerDirection, c.allCPs);

			if (!isGrounded && !wasGrounded)
			{
				coyote = true;
				c.StartCoroutine(Coyote(t.coyoteTime));
			}





			c.animationController.enableFeetIK = isGrounded;

			if (isHoldingSprintKey)
			{
				if (cameras.m_UpdatingCameraDirection)
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
			else if (isSprinting) walkingType = WalkingType.Walking;

			//List<ContactPoint> groundCPs = new List<ContactPoint>();

			float speedScalar = 1;

			//Test for water

			if (playerWaterObject?.currentFluid != null)
			{
				MoveThroughWater(ref speedScalar);
			}


			float currentMovementSpeed = t.GetSpeeds(walkingType).movementSpeed;

			GetDesiredVelocity(playerDirection, currentMovementSpeed, speedScalar, out Vector3 desiredVelocity);


			if (isGrounded)
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


			Vector3 requiredForce = Vector3.ProjectOnPlane(desiredVelocity - c.rb.velocity, c.WorldDown);


			requiredForce = Vector3.ClampMagnitude(requiredForce, t.maxAcceleration * Time.fixedDeltaTime);

			//Add slope boost or reduction
			requiredForce += GetSlopeForce(requiredForce, groundCP.normal);


			//rotate the target based on the ground the player is standing on


			if (isGrounded)
				requiredForce -= currentGroundNormal * t.groundClamp;

			c.rb.AddForce(requiredForce, ForceMode.VelocityChange);

			lastVelocity = velocity;

			if (DebugMenu.menuEnabled)
			{
				entry.Clear();
				entry.AppendFormat("Velocity: {0:0.0}, Contact Point Count {1}, On Ground {2}", c.rb.velocity.magnitude, c.allCPs.Count, isGrounded);
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
			if (spell < c.spellTree.selectedNodes.Length && spell >= 0 && c.spellTree.selectedNodes[spell] != null)
				StartSpell(c.spellTree.selectedNodes[spell].BeginCast(c));
		}
		public void StartSpell(Spell spell)
		{
			if (currentCastingSpell != null) CancelSpellCast(true);
			currentCastingSpell = spell;

			forceForwardHeadingToCamera = true;
			currentCastingSpell.Begin();
		}
		public void CastSpell(CastType castType)
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

		public void SheathBow()
		{
			c.StartCoroutine(c.UnEquipAll());
			c.weaponSet = PlayerController.WeaponSet.MeleeSidearm;
		}

		public void OnAttack(InputActionPhase phase)
		{
			if (currentCastingSpell != null && currentCastingSpell.castType == CastType.PrimaryFire) CastSpell(CastType.PrimaryFire);
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

		public void OnAttackBow(InputActionPhase phase)
		{
			if (inControl && c.currentBow != -1 && c.currentAmmo != -1)
			{
				if (phase == InputActionPhase.Performed)
				{
					//Button down - start charge
					//Player bow is kinda spell
					//Start the spell at this point so that it can be canceled while items are sheathing
					StartSpell(new PlayerBowAttack(c, this));

					Debug.Log("Performed");

					if (c.weaponSet != PlayerController.WeaponSet.BowArrow)
					{
						c.StartCoroutine(c.UnEquipAll(() =>
						{
							c.weaponSet = PlayerController.WeaponSet.BowArrow;
							if (currentCastingSpell != null) //Change the bow is already cancelled
								(currentCastingSpell as PlayerBowAttack).BeginCharge();

						}));
					}
					else
					{
						(currentCastingSpell as PlayerBowAttack).BeginCharge();
					}



				}
				else if (phase == InputActionPhase.Canceled)
				{
					//button up - end charge
					if (currentCastingSpell.castType == CastType.ReleaseCharge)
						CastSpell(CastType.ReleaseCharge);

					Debug.Log("Released");
				}
			}
		}

		#region Bows



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
			float bestDirectionDot = 1;
			groundNormal = default;
			foreach (ContactPoint cp in allCPs)
			{
				//Pointing with some up direction
				if (Vector3.Dot(c.WorldUp, cp.normal) > c.m_maxGroundSlopeDot)
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
			if (new Vector2(currVelocity.x, currVelocity.z).sqrMagnitude < 0.0001f)
				return false;
			for (int i = 0; i < allCPs.Count; i++)// test if every point is suitable for a step up
				if (ResolveStepUp(out stepUpOffset, allCPs[i], groundCP, currVelocity))
					return true;
			return false;
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

			//FIXME: Only works in normal gravity

			//( 1 ) Check if the contact point normal matches that of a step (y close to 0)
			// if (Mathf.Abs(stepTestCP.normal.y) >= 0.01f)
			// {
			//     return false;
			// }

			//if the step and the ground are too close, do not count
			if (Vector3.Dot(stepTestCP.normal, c.WorldUp) > c.m_maxGroundSlopeDot)
			{
				if (debugStep) Debug.Log($"Contact too close to ground normal {Vector3.Dot(stepTestCP.normal, c.WorldUp)}");
				return false;
			}
			float height = stepTestCP.point.y - groundCP.point.y;
			//( 2 ) Make sure the contact point is low enough to be a step
			if (height < t.minStepHeight || height >= t.maxStepHeight)
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
			Vector3 direction = c.WorldDown;
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
			float dist = Vector3.Distance(point, start);

			while (t < 1)
			{
				t += Time.deltaTime * this.t.steppingSpeed / dist;
				//entry.values[2] = t;
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
			//entry.values[2] = 0;
			c.rb.isKinematic = false;
			c.rb.velocity = lastVelocity;
			inControl = true;
		}

		#endregion

		IEnumerator Coyote(float time)
		{
			yield return new WaitForSeconds(time);
			coyote = false;
		}

		public void OnJump(InputActionPhase phase)
		{
			if (inControl && phase == InputActionPhase.Started && (isGrounded || coyote))
			{
				//use acceleration to give constant upwards force regardless of mass
				// Vector3 v = c.rb.velocity;
				// v.y = c.jumpForce;
				// c.rb.velocity = v;

				Vector2 jump = t.GetSpeeds(walkingType).twoDJumpForce;
				var vel = Vector3.ProjectOnPlane(c.rb.velocity, c.WorldUp).normalized;
				vel *= jump.x;
				vel += c.WorldUp * jump.y;

				c.rb.velocity = vel;

				GetDesiredVelocity(transform.forward, t.GetSpeeds(walkingType).movementSpeed, 1, out Vector3 direction);

				c.ChangeToState(t.freefalling);
			}
		}


		public override void OnDrawGizmos()
		{
			currentCastingSpell?.OnDrawGizmos();
		}
	}
}