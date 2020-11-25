using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{
    [
        Serializable,
        RequiresParallelState(typeof(ToggleMenus), typeof(Interact), typeof(ScanForNear<IAttackable>))
    ]

    public class Walking : MovementState, IInteractReceiver
    {
        public override string StateName => "Walking";

        public enum WeaponSet { MeleeSidearm, BowArrow }
        enum WalkingType { Walking, Sprinting, Crouching }

        public WeaponSet weaponSet;

        Vector3 currentGroundNormal = new Vector3();


        //used to continue momentum when the controller hits a stair
        Vector3 lastVelocity;
        Vector3 groundVelocity;


        //shooting variables for gizmos
        [NonSerialized] public DebugMenu.DebugEntry entry;

        bool forceForwardHeading = false;
        bool grounded;



        bool crouching { get => walkingType == WalkingType.Crouching; }
        bool sprinting { get => walkingType == WalkingType.Sprinting; }


        WalkingType walkingType = WalkingType.Walking;
        Dictionary<WalkingType, float> walkingSpeeds;
        bool inControl = true;
        [NonSerialized] Collider[] crouchTestColliders = new Collider[2];
        [NonSerialized] ContactPoint groundCP;



        MeleeWeaponItemData meleeWeapon => c.db[InventoryController.singleton.melee.items[currentMelee].name] as MeleeWeaponItemData;
        public int currentMelee => selections[ItemType.Melee];
        public int currentBow => selections[ItemType.Bow];
        public int currentAmmo => selections[ItemType.Ammo];
        public int currentSidearm => selections[ItemType.SideArm];

        public Dictionary<ItemType, int> selections = new Dictionary<ItemType, int>(new ItemTypeEqualityComparer()){
            {ItemType.Melee,-1},
            {ItemType.Bow,-1},
            {ItemType.Ammo,-1},
            {ItemType.SideArm,-1},
        };

        public bool movingHoldable;
        public bool holdingBody;

        //Save system does not work with game references, yet.
        //This should be done but is not super important
        [NonSerialized] public HoldableBody holding;

        //WEAPONS
        bool holdingSecondary = false;
        [NonSerialized] Coroutine bowChargingRoutine;
        float bowCharge = 0;
        float bowSpeed => Mathf.Lerp(10, 20, bowCharge);

        EquipmentSet<bool> sheathing = new EquipmentSet<bool>(false, false, false);
        EquipmentSet<bool> equipping = new EquipmentSet<bool>(false, false, false);
        EquipmentSet<bool> activated = new EquipmentSet<bool>(false, false, false);

        ShieldItemData SidearmAsShield => c.db[InventoryController.singleton.sideArm[currentSidearm].name] as ShieldItemData;
        bool SidearmIsShield => SidearmAsShield is ShieldItemData;

        bool holdingAltAttack = false;

        ScanForNear<IAttackable> nearAttackables;

        public bool Usable(ItemType t)
        {
            switch (t)
            {
                case ItemType.SideArm: return !sheathing.sidearm && !equipping.sidearm && !activated.sidearm && selections[ItemType.SideArm] != -1;
                case ItemType.Melee: return !sheathing.melee && !equipping.melee && !activated.melee && selections[ItemType.Melee] != -1;
                //Bow requires ammo and bow selection
                case ItemType.Bow: return !sheathing.bow && !equipping.bow && !activated.bow && selections[ItemType.Bow] != -1 && selections[ItemType.Ammo] != -1;
                default: return false;
            }
        }

        public bool Using(ItemType t)
        {
            switch (t)
            {
                case ItemType.SideArm: return !sheathing.sidearm && !equipping.sidearm && activated.sidearm && selections[ItemType.SideArm] != -1;
                case ItemType.Melee: return !sheathing.melee && !equipping.melee && activated.melee && selections[ItemType.Melee] != -1;
                //Bow requires ammo and bow selection
                case ItemType.Bow: return !sheathing.bow && !equipping.bow && activated.bow && selections[ItemType.Bow] != -1 && selections[ItemType.Ammo] != -1;
                default: return false;
            }
        }


        public override void Start()
        {
            entry = DebugMenu.CreateEntry("Player", "Velocity: {0:0.0} Contact Point Count {1} Stepping Progress {2} On Ground {3}", 0, 0, 0, false);

            walkingSpeeds = new Dictionary<WalkingType, float>(){
                {WalkingType.Walking , c.walkingSpeed},
                {WalkingType.Crouching , c.crouchingSpeed},
                {WalkingType.Sprinting , c.sprintingSpeed},
            };

            //c.controllingCamera = false; // debug for camera parallel state

            //c.transform.up = Vector3.up;
            c.animationController.enableFeetIK = true;
            c.rb.isKinematic = false;
            InventoryController.singleton.OnSelectItemEvent += OnSelectItem;
            animator.applyRootMotion = false;


            c.onPlayerInput += OnInput;
            InventoryController.singleton.onItemAdded += OnItemAdded;


            if (c.persistentStateData.TryGetValue("currentWeapon", out object o1)) selections[ItemType.Melee] = (int)o1;
            if (c.persistentStateData.TryGetValue("currentBow", out object o2)) selections[ItemType.Bow] = (int)o2;
            if (c.persistentStateData.TryGetValue("currentAmmo", out object o3)) selections[ItemType.Ammo] = (int)o3;
            if (c.persistentStateData.TryGetValue("currentSidearm", out object o4)) selections[ItemType.SideArm] = (int)o4;


            c.health.onTakeDamage += OnTakeDamage;
            c.health.onBlockDamage += OnBlockDamage;
            //Try to force any ammo type to be selected
            SelectAmmo(0);

            nearAttackables = (ScanForNear<IAttackable>)c.GetParallelState(typeof(ScanForNear<IAttackable>));
            nearAttackables.updateEveryFrame = false;
        }

        public void HoldHoldable(HoldableBody body)
        {
            UnEquipAll();

            holding = body;
            //Keep body attached to top of player;
            holding.transform.position = (transform.position + Vector3.up * (c.walkingHeight + holding.heightOffset));

            holding.joint.connectedBody = c.rb;

            holdingBody = true;

            UIKeyPromptGroup.singleton.ShowPrompts(
                c.playerInput,
                "Ground Action Map",
                new UIKeyPromptGroup.KeyPrompt("Drop", "Attack"),
                new UIKeyPromptGroup.KeyPrompt("Throw", "AltAttack")
                );

            (c.GetParallelState(typeof(Interact)) as Interact).End();

            c.StartCoroutine(PickupHoldable());
        }

        IEnumerator PickupHoldable()
        {
            movingHoldable = true;
            yield return new WaitForSeconds(0.1f);
            movingHoldable = false;
        }


        public void OnInteract(IInteractable interactable)
        {
            if (interactable is RestPoint restPoint)
            {
                print("Started rest");
                inControl = false;

                UIKeyPromptGroup.singleton.ShowPrompts(
                    c.playerInput,
                    "Ground Action Map",
                    new UIKeyPromptGroup.KeyPrompt("Rest", "Action"),
                    new UIKeyPromptGroup.KeyPrompt("Roast Marshmellow", "Attack")
                    );

                if (c.TryGetParallelState<Interact>(out var s))
                    s.End();




                c.StartCoroutine(UnEquipAll(() =>
                {
                    c.animationController.TriggerTransition(c.transitionSet.startSitting);

                    GameCameras.s.playerTrackingOffset = 0.5f;

                    bool inDialogue = false;

                    bool OnInput(InputAction.CallbackContext context)
                    {
                        switch (context.action.name)
                        {
                            case "Walk":
                                if (inDialogue) break;
                                //Return to walking
                                c.animationController.TriggerTransition(c.transitionSet.stopSitting);
                                inControl = true;
                                UIKeyPromptGroup.singleton.RemovePrompts();

                                GameCameras.s.playerTrackingOffset = GameCameras.s.defaultTrackingOffset;
                                c.onPlayerInput -= OnInput;


                                s?.Start();
                                break;
                            case "Action":
                                if (!inDialogue)
                                {
                                    //Run the dialogue
                                    c.runner.Add(restPoint.dialogue);
                                    c.runner.StartDialogue("Start");

                                    c.runner.AddCommandHandler("Morning", (_) => Debug.Log("Morning"));
                                    c.runner.AddCommandHandler("Noon", (_) => Debug.Log("Noon"));
                                    c.runner.AddCommandHandler("Night", (_) => Debug.Log("Night"));
                                    c.runner.AddCommandHandler("None", (_) => Debug.Log("None"));

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
                                break;
                        }


                        return false;
                    };

                    c.onPlayerInput += OnInput;

                }
                ));





            }
        }


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
            RemoveHoldable((transform.forward + Vector3.up).normalized * c.throwForce);
        }

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
                RemoveWeaponTrigger();
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

        public override void End()
        {
            transform.SetParent(null, true);
            DebugMenu.RemoveEntry(entry);

            c.health.onTakeDamage -= OnTakeDamage;
            c.health.onBlockDamage -= OnBlockDamage;

            //make sure the collider is left correctly
            c.collider.height = c.walkingHeight;
            c.collider.center = Vector3.up * c.collider.height * 0.5f;


            //Save state data
            c.persistentStateData["currentWeapon"] = currentMelee;
            c.persistentStateData["currentBow"] = currentBow;
            c.persistentStateData["currentAmmo"] = currentAmmo;
            c.persistentStateData["currentSidearm"] = currentSidearm;


            InventoryController.singleton.OnSelectItemEvent -= OnSelectItem;
            InventoryController.singleton.onItemAdded -= OnItemAdded;
        }

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
                        print("Too deep to walk");
                        c.ChangeToState<Swimming>();
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
                print("Too deep to walk");
                c.ChangeToState<Swimming>();
            }
        }

        public void GetDesiredVelocity(Vector3 playerDirection, float movementSpeed, float speedScalar, out Vector3 desiredVelocity)
        {
            if (forceForwardHeading)
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
                Quaternion walkingAngle = Quaternion.LookRotation(playerDirection);

                //If not forcing heading, rotate towards walking
                transform.rotation = Quaternion.RotateTowards(transform.rotation, walkingAngle, Time.deltaTime * 800);

                if (Quaternion.Angle(transform.rotation, walkingAngle) > 30f)
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

            }
            else
            {
                //No movement
                desiredVelocity = Vector3.zero;
            }
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
            Vector3 playerDirection = c.cameraController.TransformInput(c.input.horizontal);

            grounded = FindGround(out groundCP, out currentGroundNormal, playerDirection, c.allCPs);

            c.animationController.enableFeetIK = grounded;

            if (c.holdingSprintKey)
            {
                //Do not allow sprinting while a weapon is equipped, so wait until
                //The weapon for the set is sheathed before allowing sprint
                if (weaponSet == WeaponSet.MeleeSidearm)
                {
                    if (!sheathing[ItemType.Melee])
                    {
                        if (!c.weaponGraphicsController.holdables.melee.sheathed)
                        {
                            //Will only operate is sword exists
                            c.StartCoroutine(SheathItem(ItemType.Melee));
                        }
                        else
                        {
                            walkingType = WalkingType.Sprinting;
                        }
                    }

                    //Will only operate if sidearm exists
                    if (!sheathing[ItemType.SideArm] && !c.weaponGraphicsController.holdables[ItemType.SideArm].sheathed)
                    {
                        //Will only operate is sword exists
                        c.StartCoroutine(SheathItem(ItemType.SideArm));
                    }

                }
                else
                {

                    if (!sheathing[ItemType.Bow])
                    {
                        if (!c.weaponGraphicsController.holdables.bow.sheathed)
                        {
                            //Will only operate if bow exists
                            c.StartCoroutine(SheathItem(ItemType.Bow));
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

            if (grounded)
            {

                //step up onto the stair, reseting the velocity to what it was
                if (FindStep(out Vector3 stepUpOffset, c.allCPs, groundCP, playerDirection))
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
            float speedScalar = 1;

            //Test for water

            if (c.currentWater != null)
            {
                MoveThroughWater(ref speedScalar);
            }

            //c.transform.rotation = Quaternion.Euler(0, cc.camRotation.x, 0);
            if (c.holdingCrouchKey)
            {
                c.collider.height = c.crouchingHeight;
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

            float currentMovementSpeed = walkingSpeeds[walkingType];

            GetDesiredVelocity(playerDirection, currentMovementSpeed, speedScalar, out Vector3 desiredVelocity);

            Vector3 requiredForce = desiredVelocity - c.rb.velocity;
            requiredForce.y = 0;

            requiredForce = Vector3.ClampMagnitude(requiredForce, c.maxAcceleration * Time.fixedDeltaTime);

            //rotate the target based on the ground the player is standing on


            if (grounded)
                requiredForce -= currentGroundNormal * c.groundClamp;

            c.rb.AddForce(requiredForce, ForceMode.VelocityChange);

            lastVelocity = velocity;

            entry.values[0] = c.rb.velocity.magnitude;
            entry.values[1] = c.allCPs.Count;
            entry.values[3] = grounded;
        }

        public override void OnInteract(InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started)
                if (holdingBody && !movingHoldable) PlaceHoldable();
        }

        public void OnItemAdded(ItemName item, bool hiddenAddition)
        {
            //If this is ammo and none is equipped, equip it
            if (currentAmmo == -1 && InventoryController.singleton.db[item].type == ItemType.Ammo)
            {
                //Ammo is only allowed to be -1 if none is left
                SelectAmmo(0);
            }
        }

        bool OnInput(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                switch (context.action.name)
                {
                    case "SwitchWeaponSet":
                        c.StartCoroutine(SwitchWeaponSet());
                        return false; //Do not continue to process input
                }
            return true;
        }
        IEnumerator SwitchWeaponSet()
        {
            yield return UnEquipAll();
            if (weaponSet == WeaponSet.BowArrow) weaponSet = WeaponSet.MeleeSidearm;
            else weaponSet = WeaponSet.BowArrow;
        }


        public void OnSelectItem(ItemType type, int index)
        {
            Debug.Log($"Selected Item Type {type} - {index}");

            if (InventoryController.singleton.GetPanelFor(type).stackCount > index && index != selections[type])
            {
                //Draw or Sheath the selected type

                //If the user wishes to deselect this type:

                if (type == ItemType.Ammo)
                {
                    SelectAmmo(index);
                }
                else
                {
                    if (index == -1)
                    {
                        selections[type] = -1;

                        c.weaponGraphicsController.holdables[type].RemoveHeld();
                        //Do not trigger over time - remove immediately
                        c.StartCoroutine(SheathItem(type));
                    }
                    else
                    {
                        ItemName name = InventoryController.ItemAt(index, type);
                        if (c.db[name] is HoldableItemData holdableItemData)
                        {
                            selections[type] = index;

                            c.weaponGraphicsController.holdables[type].SetHeld(holdableItemData);

                        }
                    }
                }
                SelectedItemDisplayForType(type).ChangeItemIndex(selections[type]);
            }



            // switch (type)
            // {
            //     case ItemType.Weapon:
            //         SelectMelee(index);
            //         break;
            //     case ItemType.SideArm:
            //         SelectSidearm(index);
            //         break;
            //     case ItemType.Bow:
            //         SelectBow(index);
            //         break;
            //     case ItemType.Ammo:
            //         SelectAmmo(index);
            //         break;
            // }
        }

        //In this case, common means no value
        ItemType selectingSlot = ItemType.Common;
        ItemType ItemTypeFromID(int id)
        {
            switch (id)
            {
                case 0: return ItemType.Melee;
                case 1: return ItemType.SideArm;
                case 2: return ItemType.Bow;
                case 3: return ItemType.Ammo;
                default: return ItemType.Common;
            }
        }
        InventoryItemUI SelectedItemDisplayForType(ItemType t)
        {
            switch (t)
            {
                case ItemType.Melee: return UIController.singleton.selectedMeleeDisplay.GetComponent<InventoryItemUI>();
                case ItemType.SideArm: return UIController.singleton.selectedSidearmDisplay.GetComponent<InventoryItemUI>();
                case ItemType.Bow: return UIController.singleton.selectedBowDisplay.GetComponent<InventoryItemUI>();
                case ItemType.Ammo: return UIController.singleton.selectedAmmoDisplay.GetComponent<InventoryItemUI>();
                default: return null;
            }
        }


        public override void OnSelectWeapon(int index, InputActionPhase phase)
        {
            if (phase == InputActionPhase.Started && selectingSlot == ItemType.Common)
            {
                selectingSlot = ItemTypeFromID(index);

                var s = UIController.singleton.scrollingSelector.GetComponent<ScrollingSelectorUI>();
                s.selectingType = selectingSlot;
                s.selection = selections[selectingSlot];
                s.gameObject.SetActive(true);

                //Pause the game until the user has selected
                inControl = false;
                c.Pause();
            }
            else if (phase == InputActionPhase.Canceled)
            {
                var s = UIController.singleton.scrollingSelector.GetComponent<ScrollingSelectorUI>();
                //Select this item for the weapon controls
                OnSelectItem(selectingSlot, s.selection);

                UIController.singleton.scrollingSelector.gameObject.SetActive(false);

                selectingSlot = ItemType.Common;

                //Un pause the game
                inControl = true;
                c.Play();
            }
        }

        public static bool EnforceType<T>(object o) => o != null && o is T;


        public void SelectAmmo(int index)
        {
            if (InventoryController.singleton.ammo.items.Count > index && index >= 0)
            {
                selections[ItemType.Ammo] = index;

                NotchArrow();
            }
            else
            {
                selections[ItemType.Ammo] = -1;
                RemoveNotchedArrow();

            }
            UIController.singleton.selectedAmmoDisplay.GetComponent<InventoryItemUI>().ChangeItemIndex(currentAmmo);
        }


        public IEnumerator UnEquipAll(System.Action onComplete = null)
        {
            if (weaponSet == WeaponSet.MeleeSidearm)
            {
                if (!c.weaponGraphicsController.holdables[ItemType.Melee].sheathed)
                {
                    yield return SheathItem(ItemType.Melee);
                }
                if (!c.weaponGraphicsController.holdables[ItemType.SideArm].sheathed)
                {
                    yield return SheathItem(ItemType.SideArm);
                }
            }
            else //De quip all the bow and arrow stuff
            {
                if (!c.weaponGraphicsController.holdables[ItemType.Bow].sheathed)
                {
                    yield return SheathItem(ItemType.Bow);
                }
            }

            onComplete?.Invoke();
        }

        public override void OnAttack(InputActionPhase phase)
        {
            if (!inControl) return;

            if (holdingBody) PlaceHoldable();

            if (weaponSet == WeaponSet.MeleeSidearm && phase == InputActionPhase.Started && currentMelee != -1)
            {
                if (inControl)
                {
                    if (c.weaponGraphicsController.holdables.melee.sheathed == true)
                    {
                        c.StartCoroutine(DrawItem(ItemType.Melee));
                    }
                    else if (Usable(ItemType.Melee))
                    {
                        SwingSword();
                    }
                }
            }
            else if (weaponSet == WeaponSet.BowArrow && currentBow != -1 && currentAmmo != -1)
            {
                if (phase == InputActionPhase.Started)
                {
                    //Button down - start charge
                    bowChargingRoutine = c.StartCoroutine(ChargeBow());
                }
                else if (phase == InputActionPhase.Canceled)
                {
                    //button up - end charge
                    c.StopCoroutine(bowChargingRoutine);
                    ReleaseBow();
                    if (bowCharge > 0.2f)
                        FireBow();
                }
            }
        }

        IEnumerator ChargeBow()
        {

            if (c.weaponGraphicsController.holdables.bow.sheathed)
            {
                NotchArrow(); //TODO: Notch arrow should play animation, along with drawing bow
                yield return c.weaponGraphicsController.DrawItem(ItemType.Bow, c.transitionSet);
            }

            bowCharge = 0;

            forceForwardHeading = true;
            GameCameras.s.cameraTargetXOffset = c.shoulderViewXOffset;


            c.animator.SetBool("Holding Bow", true);

            c.animationController.lookAtPositionWeight = 1;
            c.animationController.headLookAtPositionWeight = 1;
            c.animationController.eyesLookAtPositionWeight = 1;
            c.animationController.bodyLookAtPositionWeight = 1;
            c.animationController.clampLookAtPositionWeight = 0.5f; //180 degrees


            var bowAC = c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Animator>();
            while (true)
            {
                yield return new WaitForEndOfFrame();

                c.animationController.lookAtPosition = GameCameras.s.cameraTransform.forward * 1000 + GameCameras.s.cameraTransform.position;

                c.weaponGraphicsController.holdables.bow.gameObject.transform.LookAt(c.animationController.lookAtPosition);

                //Update how zoomed on the shoulder the camera is
                GameCameras.s.shoulderViewStrength = Mathf.Sqrt(bowCharge);

                bowCharge += Time.deltaTime;
                bowCharge = Mathf.Clamp01(bowCharge);
                bowAC.SetFloat("Charge", bowCharge);
                //Update trajectory (in local space)
            }
        }

        void ReleaseBow()
        {
            GameCameras.s.cameraTargetXOffset = 0;

            GameCameras.s.shoulderViewStrength = 0;

            forceForwardHeading = false;
            c.animationController.lookAtPositionWeight = 0; // Dont need to do others - master switch
            c.animator.SetBool("Holding Bow", false);
            c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Animator>().SetFloat("Charge", 0);
        }

        void FireBow()
        {
            print("Charged bow to {0}", bowCharge);
            //Fire ammo

            // var ammo = (AmmoItemData)c.db[ammoName];

            // SpawnableBody ammoBody = await GameObjectSpawner.SpawnAsync(ammo.ammoGameObject, c.arrowSpawn.position, Quaternion.identity);

            // Arrow arrow = ammoBody.GetComponent<Arrow>();

            c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().ReleaseArrow(GameCameras.s.cameraTransform.forward * bowSpeed);

            //Initialize arrow
            //arrow.Initialize(ammoName, c.arrowSpawn.position, GameCameras.s.cameraTransform.forward * bowSpeed, InventoryController.singleton.db);


            //Remove one of ammo used
            InventoryController.TakeItem(currentAmmo, ItemType.Ammo);

            //Test if ammo left for shooting
            if (InventoryController.ItemCount(currentAmmo, ItemType.Ammo) == 0)
            {
                print("Run out of arrow type");
                //Keep current ammo within range of avalibles
                SelectAmmo(currentAmmo);
            }
            else
            {
                NotchArrow();
            }

        }


        public void NotchArrow()
        {
            if (selections[ItemType.Bow] != -1)
            {
                ItemName ammoName = InventoryController.ItemAt(currentAmmo, ItemType.Ammo);
                c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().NotchNextArrow(ammoName);
            }
        }
        public void RemoveNotchedArrow()
        {
            if (selections[ItemType.Bow] != -1)
            {
                c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().RemoveNotchedArrow();
            }
        }

        IEnumerator DrawItem(ItemType type, System.Action onComplete = null)
        {

            equipping[type] = true;

            if (sprinting)
            {
                walkingType = WalkingType.Walking;
                c.holdingSprintKey = false; //Stop the player from immediately sprinting again
            }

            yield return c.weaponGraphicsController.DrawItem(type, c.transitionSet);

            equipping[type] = false;
            onComplete?.Invoke();
        }


        IEnumerator SheathItem(ItemType type)
        {
            sheathing[type] = true;
            if (type == ItemType.Bow)
            {
                RemoveNotchedArrow();
            }
            yield return c.weaponGraphicsController.SheathItem(type, c.transitionSet);
            sheathing[type] = false;
        }


        void AddWeaponTrigger()
        {
            // var collider = c.weaponGraphicsController.holdables.melee.worldObject.gameObject.AddComponent<MeshCollider>();
            // collider.convex = true;
            // collider.isTrigger = true;
            var trigger = c.weaponGraphicsController.holdables.melee.gameObject.gameObject.GetComponent<WeaponTrigger>();
            trigger.enableTrigger = true;

            if (!trigger.inited)
            {
                trigger.Init(meleeWeapon.hitSparkEffect);
                trigger.weaponItem = meleeWeapon.itemName;
                trigger.controller = gameObject;
            }
        }

        void RemoveWeaponTrigger()
        {
            //Clean up the trigger detection of the sword
            var trigger = c.weaponGraphicsController.holdables.melee.gameObject.gameObject.GetComponent<WeaponTrigger>();
            trigger.enableTrigger = false;

            c.onSwingStateChanged = null;
            inControl = true;
            activated.melee = false;

            float time = 0.07f;
            c.StartCoroutine(LerpNumber((x) => c.animationController.bodyLookAtPositionWeight = x, c.animationController.bodyLookAtPositionWeight, 0, time));
            c.StartCoroutine(LerpNumber((x) => c.animationController.lookAtPositionWeight = x, c.animationController.lookAtPositionWeight, 0, time));
        }

        IEnumerator LerpNumber(Action<float> update, float from, float to, float time)
        {
            float t = 0;
            float invTime = 1 / time;
            while (t < 1)
            {
                t += Time.deltaTime * invTime;
                update(Mathf.Lerp(from, to, t));
                yield return new WaitForEndOfFrame();
            }
            update(to);
        }

        //Play the animation and use triggers to swing the player's sword
        void SwingSword()
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
            for (int i = 1; i < nearAttackables.nearObjects.Count; i++)
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
            c.animationController.TriggerTransition(c.transitionSet.swingSword);


            c.onSwingStateChanged = (bool on) =>
            {
                if (on) AddWeaponTrigger();
                else RemoveWeaponTrigger();
            };

        }

        public override void OnAltAttack(InputActionPhase phase)
        {
            if (!inControl) return;
            if (holdingBody)
            {
                ThrowHoldable();
            }

            else if (phase == InputActionPhase.Started)
            {
                holdingAltAttack = true;

                if (currentSidearm != -1)
                {
                    //Equip the sidearm if it wasnt
                    if (!sheathing.sidearm && c.weaponGraphicsController.holdables.sidearm.sheathed)
                    {
                        print("Drawing sidearm");
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

        public void RaiseShield()
        {
            //Only raise shield if alt attack is still being held
            if (!activated.sidearm && holdingAltAttack)
            {
                print("Raising shield");
                activated.sidearm = true;
                c.animationController.TriggerTransition(c.transitionSet.shieldRaise);

                c.health.blockingDamage = true;
                c.health.minBlockingDot = SidearmAsShield.minBlockingDot;
            }
        }
        public void LowerShield()
        {
            if (currentSidearm != -1 && activated.sidearm && SidearmIsShield)
            {
                activated.sidearm = false;
                c.animationController.TriggerTransition(c.transitionSet.shieldLower);

                c.health.blockingDamage = false;
            }
        }


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
                if (debugStep) print("Contact too close to ground normal {0}", Vector3.Dot(stepTestCP.normal, Vector3.up));
                return false;
            }

            //( 2 ) Make sure the contact point is low enough to be a step
            if (!(stepTestCP.point.y - groundCP.point.y < c.maxStepHeight))
            {
                if (debugStep) print("Contact to high");
                return false;
            }


            //( 2.5 ) Make sure the step is in the direction the player is moving
            if (Vector3.Dot(-stepTestCP.normal, velocity.normalized) < 0.01f)
            {
                if (debugStep) print(Vector3.Dot(-stepTestCP.normal, velocity.normalized).ToString());
                //not pointing in the general direction of movement - fail
                return false;
            }

            //( 3 ) Check to see if there's actually a place to step in front of us
            //Fires one Raycast
            RaycastHit hitInfo;
            float stepHeight = groundCP.point.y + c.maxStepHeight + 0.0001f;

            Vector3 stepTestInvDir = velocity.normalized; // new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;

            //check forward based off the direction the player is walking

            Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * c.stepSearchOvershoot);
            Vector3 direction = Vector3.down;
            if (!stepCol.Raycast(new Ray(origin, direction), out hitInfo, c.maxStepHeight + c.maxStepDown))
            {
                if (debugStep) print("Nothing to step to");
                return false;
            }

            //We have enough info to calculate the points
            Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * c.stepSearchOvershoot);
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
                t += Time.deltaTime / c.steppingTime;
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
                yield return new WaitForEndOfFrame();
            }
            entry.values[2] = 0;
            c.rb.isKinematic = false;
            c.rb.velocity = lastVelocity;
            inControl = true;
        }


        public override void Animate(AnimatorVariables vars)
        {
            animator.SetBool(vars.surfing.id, false);

            float speed = c.input.horizontal.magnitude * (sprinting ? 1.5f : 1);
            if (!inControl) speed = 0;

            animator.SetBool("Idle", speed == 0);

            const float dampTime = 0.1f;

            if (forceForwardHeading)
            {
                animator.SetFloat(vars.vertical.id, c.input.horizontal.y, dampTime, Time.deltaTime);
                animator.SetFloat(vars.horizontal.id, c.input.horizontal.x, dampTime, Time.deltaTime);
            }
            else
            {
                animator.SetFloat(vars.vertical.id, speed, dampTime, Time.deltaTime);
                animator.SetFloat(vars.horizontal.id, 0, dampTime, Time.deltaTime);
            }



            //c.animator.SetFloat("InputHorizontal", c.input.inputWalk.x);

            animator.SetFloat("WalkingSpeed", 1);
            animator.SetBool("IsGrounded", true);


            animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
            animator.SetFloat("GroundDistance", c.currentHeight);
            animator.SetBool("Crouching", crouching);
            animator.SetBool("IsStrafing", true);
        }

        public override void OnJump(InputActionPhase phase)
        {
            if (inControl && phase == InputActionPhase.Started && grounded)
            {
                //use acceleration to give constant upwards force regardless of mass
                // Vector3 v = c.rb.velocity;
                // v.y = c.jumpForce;
                // c.rb.velocity = v;
                c.rb.AddForce(Vector3.up * c.jumpForce);

                //ChangeToState<Freefalling>();
            }
        }


        // public override void OnCollideGround(RaycastHit hit)
        // {
        //     //currentGroundNormal = hit.normal;
        //     //Make the player stand on a platform if it is kinematic
        //     if (hit.rigidbody != null && hit.rigidbody.isKinematic)
        //     {
        //         groundVelocity = hit.rigidbody.velocity;
        //         transform.SetParent(hit.transform, true);
        //     }
        //     else
        //     {
        //         transform.SetParent(null, true);
        //     }


        //     //attempt to lock the player to the ground while walking

        // }


        // public override void OnCollideCliff(RaycastHit hit)
        // {
        //     if (
        //         hit.rigidbody != null &&
        //         hit.rigidbody.isKinematic == true &&
        //         Vector3.Dot(-hit.normal, c.cameraController.TransformInput(c.input.inputWalk)) > 0.5f)
        //     {
        //         if (Vector3.Angle(Vector3.up, hit.normal) > c.minAngleForCliff)
        //         {
        //             ChangeToState<Climbing>();
        //         }
        //         else
        //         {
        //             print("did not engage climb as {0} is too shallow", Vector3.Angle(Vector3.up, hit.normal));
        //         }
        //     }
        // }


        // public override void OnDrawGizmos()
        // {
        //     FindGround(out groundCP, out currentGroundNormal, c.allCPs);
        //     for (int i = 0; i < c.allCPs.Count; i++)
        //     {
        //         //draw positions the ground is touching
        //         if (c.allCPs[i].point == groundCP.point)
        //             Gizmos.color = Color.red;
        //         else //Change color of cps depending on importance
        //             Gizmos.color = Color.white;
        //         Gizmos.DrawWireSphere(c.allCPs[i].point, 0.05f);
        //         Gizmos.DrawLine(c.allCPs[i].point, c.allCPs[i].point + c.allCPs[i].normal * 0.1f);
        //     }
        //     Gizmos.DrawLine(transform.position, transform.position + currentGroundNormal.normalized * 0.5f);

        //     Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * c.maxStepHeight, Quaternion.identity, new Vector3(1, 0, 1));
        //     Gizmos.color = Color.yellow;
        //     //draw a place to reprosent max step height
        //     Gizmos.DrawWireSphere(Vector3.zero, c.stepSearchOvershoot + 0.25f);
        // }
    }
}