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
	public class PlayerController : MonoBehaviour, IAITarget, IWaterObject, IInteractor
	{
		public enum WeaponSet { MeleeSidearm, BowArrow }

		public InventoryController inventory;
		public SpellUnlockTree spellTree;

		[NonSerialized] public MovementState currentState;

		//Parallel state list:

		//Camera Controller - DONE
		//Weapons - DONE
		//Interaction with objects - DONE

		[NonSerialized] private MovementState[] parallelStates = new MovementState[0];


		public Yarn.Unity.DialogueRunner runner;

		[NonSerialized] private MovementState[] allStates;


		[Header("Cameras")]

		[NonSerialized] public Room currentRoom;
		public float shoulderViewXOffset = 0.6f;


		[Header("Ground detection")]
		[Range(0, 90)] public float m_maxGroundAngle = 70;
		[NonSerialized] public float m_maxGroundDot = 0.3f;
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
		public Vector2 arrowSpeedRange = new Vector2(70, 100);

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
		[NonSerialized] public Rigidbody rb;
		[NonSerialized] new public CapsuleCollider collider;
		[NonSerialized] public Animator animator;
		[NonSerialized] public Health health;

		[NonSerialized] public WeaponGraphicsController weaponGraphicsController;



		public static PlayerController activePlayerController;

		[NonSerialized] public AnimationController animationController;

		//set capacity to 1 as it is common for the player to be touching the ground in at least one point
		[NonSerialized] public List<ContactPoint> allCPs = new List<ContactPoint>(1);

		private DebugMenu.DebugEntry<string> _entry;

		private DebugMenu.DebugEntry<string> entry
		{
			get
			{
				if (_entry != null)
				{
					_entry = DebugMenu.CreateEntry("Player", "State(s): {0}", "");
				}
				return _entry;
			}
		}

		Collider IAITarget.collider => collider;
		public bool canBeTargeted => currentState != null && currentState.canBeTargeted;
		public Vector3 velocity => rb.velocity;


		public AnimatorVariables animatorVariables;

		[NonSerialized] public WaterController currentWater;


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
		[NonSerialized] public WeaponSet weaponSet;

		[Header("Channels")]
		public IntEventChannelSO onChangeSelectedMelee;
		public IntEventChannelSO onChangeSelectedSidearm;
		public IntEventChannelSO onChangeSelectedBow;
		public IntEventChannelSO onChangeSelectedAmmo;
		public ItemAddedEventChannelSO onPlayerInventoryItemAdded;
		public VoidEventChannelSO onAimModeEnable;
		public VoidEventChannelSO onAimModeDisable;
		public FloatEventChannelSO changeTimeEventChannel;
		public BoolEventChannelSO setTabMenuEventChannel;
		public IntEventChannelSO setTabMenuPanelEventChannel;
		public InputReader inputReader;

		public SaveLoadEventChannel playerSaveLoadChannel;

		// Start is called before the first frame update
		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		private void Awake()
		{
			activePlayerController = this;
			animationController = GetComponent<AnimationController>();

			playerSaveLoadChannel.onSaveBinEvent += SaveBin;
			playerSaveLoadChannel.onLoadBinEvent += LoadBin;
			playerSaveLoadChannel.onLoadBlankEvent += LoadBlank;
		}

		private void OnDestroy()
		{
			playerSaveLoadChannel.onSaveBinEvent -= SaveBin;
			playerSaveLoadChannel.onLoadBinEvent -= LoadBin;
			playerSaveLoadChannel.onLoadBlankEvent -= LoadBlank;

			inventory.armour.onItemRemoved -= OnArmourRemoved;
			inventory.OnDropItemEvent -= OnDropItem;
			inventory.OnConsumeItemEvent -= OnConsumeItem;
			inventory.OnSelectItemEvent -= OnSelectItem;
			inventory.melee.onItemRemoved -= OnEquipableItemRemoved;
			inventory.bow.onItemRemoved -= OnEquipableItemRemoved;
			inventory.sideArm.onItemRemoved -= OnEquipableItemRemoved;

			foreach (var s in allStates) s.End();
		}


		private void OnCollisionEnter(Collision col) => allCPs.AddRange(col.contacts);
		private void OnCollisionStay(Collision col) => allCPs.AddRange(col.contacts);


		public System.Action<bool> onSwingStateChanged;

		public void SwingStart() => onSwingStateChanged?.Invoke(true);

		public void SwingEnd() => onSwingStateChanged?.Invoke(false);


		private void Start()
		{
			Debug.Log("Starting player controller");

			animatorVariables.UpdateIDs();

			rb = GetComponent<Rigidbody>();
			animator = GetComponent<Animator>();
			collider = GetComponent<CapsuleCollider>();
			if (TryGetComponent<Health>(out health))
				health.onDeathEvent.AddListener(OnDeath);

			weaponGraphicsController = GetComponent<WeaponGraphicsController>();

			m_maxGroundDot = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad);



			collider.material.dynamicFriction = dynamicFriction;

			GetComponent<Ragdoller>().RagdollEnabled = false;

			inventory.armour.onItemRemoved += OnArmourRemoved;
			inventory.OnDropItemEvent += OnDropItem;
			inventory.OnConsumeItemEvent += OnConsumeItem;
			inventory.OnSelectItemEvent += OnSelectItem;
			inventory.melee.onItemRemoved += OnEquipableItemRemoved;
			inventory.bow.onItemRemoved += OnEquipableItemRemoved;
			inventory.sideArm.onItemRemoved += OnEquipableItemRemoved;

			enabled = false;
		}

		public void AfterLoaded()
		{
			enabled = true;
		}

		private void OnEnable()
		{
			inputReader.equipBowEvent += OnEquipBow;
			inputReader.switchWeaponSetEvent += OnSelectWeapon;
		}
		private void OnDisable()
		{
			inputReader.equipBowEvent -= OnEquipBow;
			inputReader.switchWeaponSetEvent -= OnSelectWeapon;
		}


		private static Type SearchForType(char symbol)
		{
			//Search assembly for the symbol by creating an instance of every class and comparing - slow
			foreach (var t in typeof(MovementState).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(MovementState))))
				//Create an instance of the type and check it's symbol
				if (((MovementState)Activator.CreateInstance(t)).StateSymbol == symbol)
					return t;
			throw new ArgumentException("Symbol not mapped to state");
		}

		public static Type SymbolToType(char symbol) => symbol switch
		{
			'W' => typeof(Walking),
			'T' => typeof(TransitionState<Walking>),
			's' => typeof(Shieldsurfing),
			'S' => typeof(Swimming),
			'L' => typeof(LadderClimb),
			'K' => typeof(KnockedOut),
			'I' => typeof(Interact),
			'E' => typeof(Dead),
			'A' => typeof(AutoWalking),
			'D' => typeof(Dialogue),
			'c' => typeof(Conversation),
			_ => SearchForType(symbol)
		};



		public void LoadBin(in GameDataReader reader)
		{
			transform.position = reader.ReadVector3();
			transform.rotation = reader.ReadQuaternion();

			armourSelections[0] = reader.ReadInt();
			armourSelections[1] = reader.ReadInt();
			armourSelections[2] = reader.ReadInt();

			itemSelections[ItemType.Melee] = reader.ReadInt();
			itemSelections[ItemType.SideArm] = reader.ReadInt();
			itemSelections[ItemType.Bow] = reader.ReadInt();
			itemSelections[ItemType.Ammo] = reader.ReadInt();

			EquipmentSet<bool> sheathedItems = new EquipmentSet<bool>(reader.ReadBool(), reader.ReadBool(), reader.ReadBool());

			var startingState = SymbolToType(reader.ReadChar());

			//currentState.LoadBin(saveVersion, reader);

			for (int i = 0; i < 3; i++)
			{
				//Apply the chosen armour to the player on load
				if (armourSelections[i] != -1)
				{
					var selected = (ArmourItemData)inventory.armour.ItemAt(armourSelections[i]).item;
					//Will automatically remove the old armour piece
					weaponGraphicsController.characterMesh.SetClothing((int)selected.armourPosition, selected.hideBody, selected.armaturePrefab);
				}
			}

			for (int i = 1; i < 5; i++)
			{
				ItemType t = (ItemType)i;
				int selection = itemSelections[t];


				itemSelections[t] = selection;
				if (selection != -1)
				{
					ItemData item = inventory.ItemAt(selection, t).item;
					if (item is HoldableItemData holdableItemData)
					{
						var x = weaponGraphicsController.holdables[t].SetHeld(holdableItemData);
						weaponGraphicsController.holdables[t].sheathed = sheathedItems[t];

						//OnSelectItem(t, selection);
					}
				}
			}

			AfterLoaded();
			//start a fresh state
			ChangeToState(startingState);
		}
		public void SaveBin(in GameDataWriter writer)
		{
			writer.Write(transform.position);
			writer.Write(transform.rotation);

			writer.Write(armourSelections[0]);
			writer.Write(armourSelections[1]);
			writer.Write(armourSelections[2]);

			writer.Write(itemSelections[ItemType.Melee]);
			writer.Write(itemSelections[ItemType.SideArm]);
			writer.Write(itemSelections[ItemType.Bow]);
			writer.Write(itemSelections[ItemType.Ammo]);

			writer.Write(weaponGraphicsController.holdables.melee.sheathed);
			writer.Write(weaponGraphicsController.holdables.sidearm.sheathed);
			writer.Write(weaponGraphicsController.holdables.bow.sheathed);

			//Save the current state
			writer.Write(currentState.StateSymbol);
			//currentState.SaveBin(writer);
		}

		public void LoadBlank()
		{
			AfterLoaded();
			//start a fresh state
			ChangeToState<Walking>();
		}

		public void EnterRoom(Room room)
		{
			currentRoom = room;

			StartCoroutine(CameraVolumeController.s.ApplyOverrideProfile(room == null ? null : room.overrideProfile, 3f));
		}


		private void OnArmourRemoved(Inventory.InventoryPanel panel, int index)
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



		public void OnDeath()
		{
			// The player has died.
			ChangeToState<Dead>();

		}

		private bool _paused = false;
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


		public async void OnDropItem(ItemType type, int itemIndex)
		{
			var item = (PhysicsItemData)inventory.GetPanelFor(type)[itemIndex].item;
			//TODO - Add way to drop multiple items
			if (inventory.TakeItem(itemIndex, type))
				await ItemSpawner.SpawnItemAsync(item, transform.position + Vector3.up * 0.1f + transform.forward, transform.rotation);
		}
		public void OnConsumeItem(ItemType type, int itemIndex)
		{
			if (type == ItemType.Potion)
			{
				//Consume the potion
				PotionItemUnique pot = inventory.potions.items[itemIndex];
				if (pot.item.itemName == ItemName.HealingPotion)
				{
					health.SetHealth(health.health + pot.potency);
				}
				inventory.potions.TakeItem(itemIndex, 1);

			}
		}


		private float sqrMagTemp;
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

		private void OnTriggerEnter(Collider other)
		{
			if (enabled)
				for (int i = 0; i < allStates.Length; i++)
					if (StateActive(i))
						allStates[i].OnTriggerEnter(other);
		}

		private void OnTriggerExit(Collider other)
		{
			if (enabled)
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


		private ItemType[] selectingSlot = null;

		private void OnEquipBow(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Performed)
			{
				if (weaponSet != WeaponSet.BowArrow)
				{
					StartCoroutine(UnEquipAll());
					weaponSet = WeaponSet.BowArrow;
				}

			}

		}

		private void OnSelectWeapon(InputActionPhase phase)
		{
			//print(String.Format("Switching on slot {0}", index));

			if (phase == InputActionPhase.Started && selectingSlot == null && !paused)
			{
				selectingSlot = weaponSet switch
				{
					WeaponSet.MeleeSidearm => new ItemType[] { ItemType.Melee, ItemType.SideArm },
					WeaponSet.BowArrow => new ItemType[] { ItemType.Bow, ItemType.Ammo },
					_ => null,
				};

				var s = UIController.singleton.scrollingSelector.GetComponent<ScrollingSelectorUI>();
				s.layers[0].selecting = selectingSlot[0];
				s.layers[1].selecting = selectingSlot[1];
				s.layers[0].selection = itemSelections[selectingSlot[0]];
				s.layers[1].selection = itemSelections[selectingSlot[1]];

				s.gameObject.SetActive(true);

				//Pause the game until the user has selected
				//inControl = false;
				Pause();
			}
			else if (phase == InputActionPhase.Canceled && selectingSlot != null)
			{
				var s = UIController.singleton.scrollingSelector.GetComponent<ScrollingSelectorUI>();
				//Select this item for the weapon controls
				OnSelectItem(selectingSlot[0], s.layers[0].selection);
				OnSelectItem(selectingSlot[1], s.layers[1].selection);

				UIController.singleton.scrollingSelector.gameObject.SetActive(false);

				selectingSlot = null;

				//Un pause the game
				//inControl = true;
				Play();
			}
		}


		private void OnEquipableItemRemoved(Inventory.InventoryPanel panel, int index)
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
				var selected = (ArmourItemData)inventory.armour.ItemAt(index).item;
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
			else if (inventory.GetPanelFor(type).stackCount > index && index != itemSelections[type])
			{

				//If the user wishes to deselect this type:



				if (type == ItemType.Ammo)
				{
					SelectAmmo(index);
				}
				else //Select an ammo with a holdable graphic reprosentation
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
						ItemData item = inventory.ItemAt(index, type).item;
						if (item is HoldableItemData holdableItemData)
						{
							itemSelections[type] = index;

							var x = weaponGraphicsController.holdables[type].SetHeld(holdableItemData);
						}
					}

				}
				//Send events to listeners (UI) about changed weapons
				switch (type)
				{
					case ItemType.Melee:
						onChangeSelectedMelee.RaiseEvent(index);
						break;
					case ItemType.SideArm:
						onChangeSelectedSidearm.RaiseEvent(index);
						break;
					case ItemType.Bow:
						onChangeSelectedBow.RaiseEvent(index);
						break;
					case ItemType.Ammo:
						onChangeSelectedAmmo.RaiseEvent(index);
						break;
				}
			}
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
		}

		public void NotchArrow()
		{
			if (currentBow != -1)
			{
				var ammo = (AmmoItemData)inventory.ItemAt(currentAmmo, ItemType.Ammo).item;
				weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().NotchNextArrow(ammo);
			}
		}
		public void RemoveNotchedArrow()
		{
			if (currentBow != -1)
			{
				weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().RemoveNotchedArrow();
			}
		}
		#region Holdables

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

		public IEnumerator UnEquipAll(System.Action onComplete = null)
		{


			if (!weaponGraphicsController.holdables.melee.sheathed)
			{
				yield return SheathItem(ItemType.Melee);
			}
			if (!weaponGraphicsController.holdables.sidearm.sheathed)
			{
				yield return SheathItem(ItemType.SideArm);
			}
			if (!weaponGraphicsController.holdables.bow.sheathed)
			{
				yield return SheathItem(ItemType.Bow);
			}

			onComplete?.Invoke();
		}


		#endregion
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

		public T ChangeToState<T>(params object[] parameters) where T : MovementState, new() => (T)ChangeToState(typeof(T), parameters);
		public T ChangeToState<T>() where T : MovementState, new() => (T)ChangeToState(typeof(T));

		private void InsatiateState(Type type)
		{
			currentState?.End(); // state specific end method
			currentState = (MovementState)Activator.CreateInstance(type);


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



			if (DebugMenu.menuEnabled)
			{
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
			}
			FillAllStates();
		}
		public MovementState ChangeToState(Type type)
		{
			InsatiateState(type);
			StartAllStates();
			return currentState;
		}

		public MovementState ChangeToState(Type type, params object[] parameters)
		{
			InsatiateState(type);
			StartAllStates(parameters);
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
			StartAllStates();
			// start all the states after everything has been constructed
			currentState.Start(parameters);
		}
		public void StartAllStates()
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
			currentState.Start();
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


		private void OnDrawGizmos()
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