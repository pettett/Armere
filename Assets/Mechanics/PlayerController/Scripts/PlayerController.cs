using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Armere.Inventory;
using Armere.Inventory.UI;

namespace Armere.PlayerController
{



    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoSaveable<PlayerController.PlayerSaveData>, IAITarget, IWaterObject, IInteractor
    {



        [Serializable]
        public readonly struct PlayerSaveData
        {
            public readonly MovementState currentState;
            public readonly MovementState[] parallels;
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly int[] armourSelections;
            public readonly Dictionary<ItemType, int> itemSelections;
            public readonly EquipmentSet<bool> sheathedItems;

            public PlayerSaveData(PlayerController c)
            {
                currentState = c.currentState;
                parallels = c.parallelStates;
                position = c.transform.position;
                rotation = c.transform.rotation;
                armourSelections = c.armourSelections;
                itemSelections = c.itemSelections;
                sheathedItems = new EquipmentSet<bool>(
                    c.weaponGraphicsController.holdables.melee.sheathed,
                    c.weaponGraphicsController.holdables.sidearm.sheathed,
                    c.weaponGraphicsController.holdables.bow.sheathed);
            }
        }

        public override PlayerSaveData SaveData()
        {
            return new PlayerSaveData(this);
        }
        public override void LoadData(PlayerSaveData data)
        {
            print("Restored state");

            currentState = data.currentState;
            print(currentState);
            parallelStates = data.parallels;
            transform.SetPositionAndRotation(data.position, data.rotation);
            loadedStates = true;

            if (data.armourSelections != null)
            {
                print("Updating armour data");

                for (int i = 0; i < 3; i++)
                {
                    armourSelections[i] = data.armourSelections[i];

                    //Apply the chosen armour to the player on load
                    if (armourSelections[i] != -1)
                    {
                        var selected = ((ArmourItemData)InventoryController.singleton.db[InventoryController.singleton.armour.ItemAt(armourSelections[i]).name]);
                        //Will automatically remove the old armour piece
                        weaponGraphicsController.characterMesh.SetClothing((int)selected.armourPosition, selected.hideBody, selected.armaturePrefab);
                    }
                }
            }
            if (data.itemSelections != null)
            {

                //Melee=1, Bow=2, Ammo=3, Sidearm = 4

                for (int i = 1; i < 5; i++)
                {
                    ItemType t = (ItemType)i;
                    int selection = data.itemSelections[t];


                    itemSelections[t] = selection;
                    if (selection != -1)
                    {
                        ItemName name = InventoryController.ItemAt(selection, t).name;
                        if (db[name] is HoldableItemData holdableItemData)
                        {
                            var x = weaponGraphicsController.holdables[t].SetHeld(holdableItemData);
                            weaponGraphicsController.holdables[t].sheathed = data.sheathedItems[t];

                            //OnSelectItem(t, selection);
                        }
                    }
                }
            }
        }

        public override void LoadBlank()
        {
            loadedStates = false;
        }

        // public event Action<Player_CharacterController> onDeath;
        // public event Action<Player_CharacterController> onRepawn;
        public delegate bool ProcessPlayerInputDelegate(InputAction.CallbackContext input);
        ///<summary> Returns wether the input should still be processed</summary>
        public event ProcessPlayerInputDelegate onPlayerInput;

        public struct InputStatus
        {
            public Vector2 horizontal;
            public Vector2 camera;
            public float vertical;
        }

        public bool holdingSprintKey;
        public bool holdingCrouchKey;

        public ItemDatabase db;

        [HideInInspector] public PlayerInput playerInput;

        [NonSerialized] public MovementState currentState;

        //Parallel state list:

        //Camera Controller - DONE
        //Weapons - DONE
        //Interaction with objects - DONE

        [NonSerialized] private MovementState[] parallelStates = new MovementState[0];


        public Yarn.Unity.DialogueRunner runner;

        [NonSerialized] MovementState[] allStates;


        [Header("Cameras")]

        public Room currentRoom;
        public float shoulderViewXOffset = 0.6f;


        [Header("Ground detection")]
        [Range(0, 90)] public float m_maxGroundAngle = 70;
        [HideInInspector] public float m_maxGroundDot = 0.3f;
        public bool onGround;
        [Header("Movement")]
        public float walkingSpeed = 2f;
        public float sprintingSpeed = 5f;
        public float crouchingSpeed = 1f;
        public float walkingHeight = 1.8f;
        public float crouchingHeight = 0.9f;
        public float groundClamp = 1f;
        public float maxAcceleration = 20f;
        public float maxStepHeight = 1f;
        public float maxStepDown = 0.25f;
        public float stepSearchOvershoot = 0.3f;
        public float steppingTime = 0.1f;
        public float jumpForce = 1000f;

        [Range(0, 1)]
        public float dynamicFriction = 0.2f;

        [Header("Weapons")]
        public float swordUseDelay = 0.4f;

        [Header("Water")]
        public float maxWaterStrideDepth = 1;
        [Range(0, 1), Tooltip("0 means no movement in water, 1 means full speed at full depth")]
        public float maxStridingDepthSpeedScalar = 0.6f;
        public float waterDrag = 1;
        public float waterMovementForce = 1;

        public float waterMovementSpeed = 1;
        public float waterSittingDepth = 1;

        public GameObject waterTrailPrefab;
        [Header("Holding")]
        public float throwForce = 100;
        [Header("Climbing")]
        public float climbingColliderHeight = 1.6f;
        public float climbingSpeed = 4f;
        public float transitionTime = 4f;
        [Range(0, 180)] public float maxHeadBodyRotationDifference = 5f;
        [Header("Shield Surfing")]
        public float turningTorqueForce;
        public float minSurfingSpeed;
        public float turningAngle;

        public PhysicMaterial surfPhysicMat;


        [Header("Other")]

        public LayerMask m_groundLayerMask;
        public LayerMask m_waterLayerMask;
        public Rigidbody rb;
        [HideInInspector] public new CapsuleCollider collider;

        [HideInInspector]
        public Animator animator;

        [System.NonSerialized] public InputStatus input = new InputStatus(); //current player input

        public Health health;

        [HideInInspector] public float currentHeight = 0;

        public WeaponGraphicsController weaponGraphicsController;



        public static PlayerController activePlayerController;

        [HideInInspector]
        public AnimationController animationController;

        //set capacity to 1 as it is common for the player to be touching the ground in at least one point
        [HideInInspector] public List<ContactPoint> allCPs = new List<ContactPoint>(1);

        DebugMenu.DebugEntry<string> entry;



        Collider IAITarget.collider => collider;
        public bool canBeTargeted => currentState != null && currentState.canBeTargeted;
        public Vector3 velocity => rb.velocity;

        public Dictionary<string, object> persistentStateData = new Dictionary<string, object>();


        public AnimatorVariables animatorVariables;

        [HideInInspector] public WaterController currentWater;


        [Header("Animations")]
        public AnimationTransitionSet transitionSet;

        public readonly int[] armourSelections = new int[3] { -1, -1, -1 };

        public readonly Dictionary<ItemType, int> itemSelections = new Dictionary<ItemType, int>(new ItemTypeEqualityComparer()){
            {ItemType.Melee,-1},
            {ItemType.Bow,-1},
            {ItemType.Ammo,-1},
            {ItemType.SideArm,-1},
        };
        public int currentMelee { get => itemSelections[ItemType.Melee]; set => itemSelections[ItemType.Melee] = value; }
        public int currentBow { get => itemSelections[ItemType.Bow]; set => itemSelections[ItemType.Bow] = value; }
        public int currentAmmo { get => itemSelections[ItemType.Ammo]; set => itemSelections[ItemType.Ammo] = value; }
        public int currentSidearm { get => itemSelections[ItemType.SideArm]; set => itemSelections[ItemType.SideArm] = value; }




        bool loadedStates = false;
        // Start is called before the first frame update
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            activePlayerController = this;
            animationController = GetComponent<AnimationController>();
        }
        void OnCollisionEnter(Collision col) => allCPs.AddRange(col.contacts);
        void OnCollisionStay(Collision col) => allCPs.AddRange(col.contacts);


        public System.Action<bool> onSwingStateChanged;

        public void SwingStart() => onSwingStateChanged?.Invoke(true);

        public void SwingEnd() => onSwingStateChanged?.Invoke(false);


        private void Start()
        {

            animatorVariables.UpdateIDs();

            playerInput = GetComponent<PlayerInput>();

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            collider = GetComponent<CapsuleCollider>();
            if (TryGetComponent<Health>(out health))
                health.onDeath += OnDeath;

            weaponGraphicsController = GetComponent<WeaponGraphicsController>();

            m_maxGroundDot = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad);

            entry = DebugMenu.CreateEntry("Player", "State(s): {0}", "");


            if (loadedStates)
            {
                StartAllStates();
                FillAllStates();
            }
            else
            {
                //start a fresh state
                ChangeToState<Walking>();
            }

            collider.material.dynamicFriction = dynamicFriction;




            GetComponent<PlayerInput>().onActionTriggered += OnActionTriggered;

            GetComponent<Ragdoller>().RagdollEnabled = false;


            InventoryController.singleton.armour.onItemRemoved += OnArmourRemoved;
            InventoryController.singleton.OnDropItemEvent += OnDropItem;
            InventoryController.singleton.OnConsumeItemEvent += OnConsumeItem;
            InventoryController.singleton.OnSelectItemEvent += OnSelectItem;
            InventoryController.singleton.melee.onItemRemoved += OnEquipableItemRemoved;
            InventoryController.singleton.bow.onItemRemoved += OnEquipableItemRemoved;
            InventoryController.singleton.sideArm.onItemRemoved += OnEquipableItemRemoved;


        }

        public void EnterRoom(Room room)
        {
            currentRoom = room;

            StartCoroutine(CameraVolumeController.s.ApplyOverrideProfile(room == null ? null : room.overrideProfile, 3f));
        }

        private void OnDestroy()
        {
            InventoryController.singleton.armour.onItemRemoved -= OnArmourRemoved;
            InventoryController.singleton.OnDropItemEvent -= OnDropItem;
            InventoryController.singleton.OnConsumeItemEvent -= OnConsumeItem;
            InventoryController.singleton.OnSelectItemEvent -= OnSelectItem;
            InventoryController.singleton.melee.onItemRemoved -= OnEquipableItemRemoved;
            InventoryController.singleton.bow.onItemRemoved -= OnEquipableItemRemoved;
            InventoryController.singleton.sideArm.onItemRemoved -= OnEquipableItemRemoved;

            foreach (var s in allStates) s.End();
        }


        void OnArmourRemoved(Inventory.InventoryPanel panel, int index)
        {

            //Armour has multiple selections, so references may need to be adjusted
            for (int i = 0; i < 3; i++)
            {
                if (armourSelections[i] == index)
                {
                    //this armour piece was removed, kill
                    weaponGraphicsController.characterMesh.RemoveClothing(i);
                }
                else if (armourSelections[i] > index)
                {
                    //Same armour still selected, but it is at a different index now
                    armourSelections[i]--;
                }
            }
        }



        public void OnDeath(GameObject attacker, GameObject victim)
        {
            // The player has died.
            ChangeToState<Dead>();

        }
        bool _paused = false;
        public bool paused
        {
            get => _paused;
            set
            {
                if (_paused != value)
                {
                    //value has changed
                    if (value)
                        PauseControl();
                    else
                        Play();
                }
            }
        }

        public int engagementCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Pause()
        {
            Time.timeScale = 0;
            _paused = true;
        }
        public void Play()
        {
            Time.timeScale = 1;
            _paused = false;
        }
        public void PauseControl()
        {
            GameCameras.s.DisableControl();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GameCameras.s.lockingMouse = false;
            Pause();
        }
        public void ResumeControl()
        {
            GameCameras.s.EnableControl();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameCameras.s.lockingMouse = true;
            Play();
        }


        public bool StateActive(int i) => !paused || paused && allStates[i].updateWhilePaused;

        public void OnActionTriggered(InputAction.CallbackContext action)
        {

            //Convert nullable bool into bool - defaults to true
            if (onPlayerInput?.Invoke(action) ?? true)
            {

                string actionName = action.action.name;

                switch (actionName)
                {
                    case "SelectWeapon":
                        OnSelectWeapon((int)action.ReadValue<float>(), action.phase);
                        break;
                    case "Walk":
                        input.horizontal = action.ReadValue<Vector2>();
                        break;
                    case "Look":
                        input.camera = action.ReadValue<Vector2>();
                        break;
                    case "VerticalMovement":
                        input.vertical = action.ReadValue<float>();
                        break;

                    case "Action":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnInteract(action.phase);
                        break;
                    case "Crouch":
                        holdingCrouchKey = action.ReadValue<float>() == 1;
                        break;
                    case "Sprint":
                        holdingSprintKey = action.ReadValue<float>() == 1;
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnSprint(action.phase);
                        break;
                    case "Jump":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnJump(action.phase);
                        break;
                    case "Attack":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnAttack(action.phase);
                        break;
                    case "AltAttack":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnAltAttack(action.phase);
                        break;

                    case "KO":
                        ChangeToState<KnockedOut>();
                        break;

                    default:
                        currentState.OnCustomAction(action);
                        break;
                }
            }
        }


        public async void OnDropItem(ItemType type, int itemIndex)
        {
            ItemName name = InventoryController.singleton.GetPanelFor(type)[itemIndex].name;
            //TODO - Add way to drop multiple items
            if (InventoryController.TakeItem(itemIndex, type))
                await ItemSpawner.SpawnItemAsync(name, transform.position + Vector3.up * 0.1f + transform.forward, transform.rotation);
        }
        public void OnConsumeItem(ItemType type, int itemIndex)
        {
            if (type == ItemType.Potion)
            {
                //Consume the potion
                PotionItemUnique pot = InventoryController.singleton.potions.items[itemIndex];
                if (pot.name == ItemName.HealingPotion)
                {
                    health.SetHealth(health.health + pot.potency);
                }
                InventoryController.singleton.potions.TakeItem(itemIndex, 1);

            }
        }


        float sqrMagTemp;
        // Update is called once per frame
        private void Update()
        {

            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].Update();

            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].Animate(animatorVariables);


            for (int i = 0; i < PlayerRelativeObject.relativeObjects.Count; i++)
            {
                sqrMagTemp = (transform.position - PlayerRelativeObject.relativeObjects[i].transform.position).sqrMagnitude;
                if (PlayerRelativeObject.relativeObjects[i].enabled && sqrMagTemp > PlayerRelativeObject.relativeObjects[i].disableRange * PlayerRelativeObject.relativeObjects[i].disableRange)
                {
                    //Disable the object - disable range should be larger then enable range to stop object flickering
                    PlayerRelativeObject.relativeObjects[i].OnPlayerOutRange();
                }
                //else, check if it is close enough to enable
                else if (!PlayerRelativeObject.relativeObjects[i].enabled && sqrMagTemp < PlayerRelativeObject.relativeObjects[i].enableRange * PlayerRelativeObject.relativeObjects[i].enableRange)
                {
                    //Enable the object
                    PlayerRelativeObject.relativeObjects[i].OnPlayerInRange();
                }
            }
        }


        private void LateUpdate()
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].LateUpdate();
        }

        void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].OnTriggerEnter(other);
        }
        void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].OnTriggerExit(other);
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].FixedUpdate();

            allCPs.Clear();
        }
        private void OnAnimatorIK(int layerIndex)
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].OnAnimatorIK(layerIndex);
        }


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


        private void OnSelectWeapon(int index, InputActionPhase phase)
        {
            //print(String.Format("Switched to weapon {0}", index));

            if (phase == InputActionPhase.Started && selectingSlot == ItemType.Common && !paused)
            {
                selectingSlot = ItemTypeFromID(index);

                var s = UIController.singleton.scrollingSelector.GetComponent<ScrollingSelectorUI>();
                s.selectingType = selectingSlot;
                s.selection = itemSelections[selectingSlot];
                s.gameObject.SetActive(true);

                //Pause the game until the user has selected
                //inControl = false;
                Pause();
            }
            else if (phase == InputActionPhase.Canceled && selectingSlot != ItemType.Common)
            {
                var s = UIController.singleton.scrollingSelector.GetComponent<ScrollingSelectorUI>();
                //Select this item for the weapon controls
                OnSelectItem(selectingSlot, s.selection);

                UIController.singleton.scrollingSelector.gameObject.SetActive(false);

                selectingSlot = ItemType.Common;

                //Un pause the game
                //inControl = true;
                Play();
            }
        }


        void OnEquipableItemRemoved(Inventory.InventoryPanel panel, int index)
        {
            if (itemSelections[panel.type] == index)
            {
                //Only one selection for each category, can safely remove it
                OnSelectItem(panel.type, -1);
            }
        }


        public void OnSelectItem(ItemType type, int index)
        {
            //Draw or Sheath the selected type
            if (type == ItemType.Armour)
            {
                var selected = ((ArmourItemData)InventoryController.singleton.db[InventoryController.singleton.armour.ItemAt(index).name]);
                // if (armourSelections[(int)selected.armourPosition] != -1)
                // {
                // }
                armourSelections[(int)selected.armourPosition] = index;

                if (index != -1)
                {
                    //Will automatically remove the old armour piece
                    weaponGraphicsController.characterMesh.SetClothing((int)selected.armourPosition, selected.hideBody, selected.armaturePrefab);
                }
            }
            else if (InventoryController.singleton.GetPanelFor(type).stackCount > index && index != itemSelections[type])
            {

                //If the user wishes to deselect this type:
                SelectedItemDisplayForType(type).ChangeItemIndex(itemSelections[type]);


                if (type == ItemType.Ammo)
                {
                    SelectAmmo(index);
                }
                else
                {
                    if (index == -1)
                    {
                        itemSelections[type] = -1;

                        weaponGraphicsController.holdables[type].RemoveHeld();

                        if (!weaponGraphicsController.holdables[type].sheathed)
                        {
                            //No need to double sheath
                            //Do not trigger over time - remove immediately
                            StartCoroutine(SheathItem(type));
                        }
                    }
                    else
                    {
                        ItemName name = InventoryController.ItemAt(index, type).name;
                        if (db[name] is HoldableItemData holdableItemData)
                        {
                            itemSelections[type] = index;

                            weaponGraphicsController.holdables[type].SetHeld(holdableItemData);
                        }
                    }

                }

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


        public void SelectAmmo(int index)
        {
            if (InventoryController.singleton.ammo.items.Count > index && index >= 0)
            {
                currentAmmo = index;

                NotchArrow();
            }
            else
            {
                currentAmmo = -1;
                RemoveNotchedArrow();

            }
            UIController.singleton.selectedAmmoDisplay.GetComponent<InventoryItemUI>().ChangeItemIndex(currentAmmo);
        }
        public void NotchArrow()
        {
            if (currentBow != -1)
            {
                ItemName ammoName = InventoryController.ItemAt(currentAmmo, ItemType.Ammo).name;
                weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().NotchNextArrow(ammoName);
            }
        }
        public void RemoveNotchedArrow()
        {
            if (currentBow != -1)
            {
                weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().RemoveNotchedArrow();
            }
        }


        public EquipmentSet<bool> sheathing = new EquipmentSet<bool>(false, false, false);

        public IEnumerator SheathItem(ItemType type)
        {
            sheathing[type] = true;
            if (type == ItemType.Bow)
            {
                RemoveNotchedArrow();
            }
            yield return weaponGraphicsController.SheathItem(type, transitionSet);
            sheathing[type] = false;
        }




        private void StartControllerRumble(float duration)
        {
            StartCoroutine(RumbleController(duration));
        }

        private IEnumerator RumbleController(float duration)
        {
            SetRumbleFrequency(0.5f, 0.5f);
            yield return new WaitForSeconds(duration);
            SetRumbleFrequency(0, 0);
        }

        private void SetRumbleFrequency(float low, float high)
        {
            foreach (var gamepad in Gamepad.all)
            {
                gamepad.SetMotorSpeeds(low, high);
            }
        }

        public T ChangeToState<T>(params object[] parameters) where T : MovementState, new()
        {
            return ChangeToState(typeof(T), parameters) as T;
        }
        public MovementState ChangeToState(Type type, params object[] parameters)
        {
            currentState?.End(); // state specific end method
            currentState = Activator.CreateInstance(type) as MovementState;


            //test to see if this state requires any parallel states to be started
            RequiresParallelState requiresParallelStates = (RequiresParallelState)type.GetCustomAttributes(typeof(RequiresParallelState), true).FirstOrDefault();

            //go through and end all the parallel states not used by this state
            for (int i = 0; i < parallelStates.Length; i++)
            {
                if (requiresParallelStates != null)
                {
                    Type stateType = parallelStates[i].GetType();
                    for (int j = 0; j < requiresParallelStates.states.Count; j++)
                        if (stateType == requiresParallelStates.states[j])
                            goto DoNotEndState;
                }
                //Goto saves time here - ending state will only be skipped if it is required
                parallelStates[i].End();
            DoNotEndState:
                continue;
            }

            if (requiresParallelStates != null)
            {
                MovementState[] newStates = new MovementState[requiresParallelStates.states.Count];

                for (int i = 0; i < requiresParallelStates.states.Count; i++)
                {
                    //test if the desired parallel state is currently active
                    newStates[i] = GetParallelState(requiresParallelStates.states[i]);
                    if (newStates[i] == null)
                    {
                        newStates[i] = Activator.CreateInstance(requiresParallelStates.states[i]) as MovementState;
                    }
                }
                parallelStates = newStates;
            }
            else
            {
                parallelStates = new MovementState[0];
            }




            //Show all the currentley active states
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(currentState.StateName);
            if (parallelStates.Length != 0)
            {
                stringBuilder.Append('{');
                foreach (var s in parallelStates)
                {
                    stringBuilder.Append(s.StateName);
                    stringBuilder.Append(',');
                }
                stringBuilder.Append('}');
            }
            //update f3 information
            entry.value0 = stringBuilder.ToString();



            StartAllStates(parameters);
            FillAllStates();

            return currentState;
        }

        public void FillAllStates()
        {
            allStates = new MovementState[parallelStates.Length + 1];
            for (int i = 0; i < parallelStates.Length; i++)
            {
                allStates[i] = parallelStates[i];
            }
            allStates[parallelStates.Length] = currentState;
        }
        public void StartAllStates(params object[] parameters)
        {
            for (int i = 0; i < parallelStates.Length; i++)
            {
                parallelStates[i].Init(this);
            }
            currentState.Init(this); //non - overridable init method for reference to controller

            for (int i = 0; i < parallelStates.Length; i++)
            {
                parallelStates[i].Start();
            }
            // start all the states after everything has been constructed
            currentState.Start(parameters);
        }

        public MovementState GetParallelState(Type t)
        {
            for (int i = 0; i < parallelStates.Length; i++)
            {
                if (parallelStates[i].GetType() == t)
                {
                    return parallelStates[i];
                }
            }
            return null;
        }
        public bool TryGetParallelState<T>(out T state) where T : MovementState
        {
            for (int i = 0; i < parallelStates.Length; i++)
            {
                if (parallelStates[i].GetType() == typeof(T))
                {
                    state = parallelStates[i] as T;
                    return true;
                }
            }
            state = default;
            return false;
        }



        void OnDrawGizmos()
        {
            if (allStates != null)
                for (int i = 0; i < allStates.Length; i++)
                    if (StateActive(i))
                        allStates[i].OnDrawGizmos();
        }

        //show the sound on the minimap?
        void IAITarget.HearSound(IAITarget source, float volume, ref bool responded) { }



        public void OnWaterEnter(WaterController waterController)
        {
            currentWater = waterController;
        }

        public void OnWaterExit(WaterController waterController)
        {
            currentWater = null;
        }


    }
}