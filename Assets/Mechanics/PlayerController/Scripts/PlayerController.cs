using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Armere.Inventory;
using Armere.Inventory.UI;
using UnityEngine.AddressableAssets;

using Armere.UI;
namespace Armere.PlayerController
{
	[RequireComponent(typeof(Rigidbody), typeof(PlayerMachine))]
	public class PlayerController : Character, IGameDataSavable<PlayerController>, IInteractor
	{
		public enum WeaponSet { MeleeSidearm, BowArrow }

		public SpellUnlockTree spellTree;

		//Parallel state list:

		//Camera Controller - DONE
		//Weapons - DONE
		//Interaction with objects - DONE




		[Header("Cameras")]

		[NonSerialized] public Room currentRoom;
		public Transform cameraTrackingTransform;



		[Header("Movement")]


		public bool useGravity = true;

		public Vector3 m_gravityDirection { get; private set; } = Vector3.down;


		public Vector3 WorldUp => -m_gravityDirection;
		public Vector3 WorldDown => m_gravityDirection;







		[Header("States")]
		public MovementStateTemplate deadState;
		public KnockedOutTemplate knockoutState;
		public AutoWalkingTemplate autoWalk;


		[Header("Other")]

		public LayerMask m_groundLayerMask;
		public LayerMask m_waterLayerMask;
		[NonSerialized] public Rigidbody rb;
		[NonSerialized] new public CapsuleCollider collider;
		[NonSerialized] public Health health;



		private System.Text.StringBuilder _entry;

		public System.Text.StringBuilder entry
		{
			get
			{
				if (_entry != null)
				{
					_entry = DebugMenu.CreateEntry("Player");
				}
				return _entry;
			}
		}

		[Header("Animations")]
		public AnimationTransitionSet transitionSet;
		public Vector3Int armourSelections = new Vector3Int(-1, -1, -1);

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
		public InputReader inputReader;


		[NonSerialized] public EquipmentSet<bool> sheathing = new EquipmentSet<bool>(false, false, false);
		[NonSerialized] public PlayerMachine machine;

		// Start is called before the first frame update
		/// <summary>
		/// Awake is called when the script instance is being loaded.
		/// </summary>
		private void Awake()
		{
			if (playerCharacter != null)
			{
				Destroy(gameObject);
				return;
			}
			playerCharacter = this;
			animationController = GetComponent<AnimationController>();

			GameCameras.s.freeLookTarget = cameraTrackingTransform;
		}

		private void OnDestroy()
		{
			if (inventory == null) return;

			inventory.armour.onItemRemoved -= OnArmourRemoved;
			inventory.OnDropItemEvent -= OnDropItem;
			inventory.OnConsumeItemEvent -= OnConsumeItem;
			inventory.OnSelectItemEvent -= OnSelectItem;
			inventory.melee.onItemRemoved -= OnEquipableItemRemoved;
			inventory.bow.onItemRemoved -= OnEquipableItemRemoved;
			inventory.sideArm.onItemRemoved -= OnEquipableItemRemoved;


		}



		public override void Start()
		{
			base.Start();


			print("Starting player controller");

			rb = GetComponent<Rigidbody>();
			collider = GetComponent<CapsuleCollider>();
			if (TryGetComponent<Health>(out health))
				health.onDeathEvent.AddListener(OnDeath);

			inventoryHolder = GetComponent<GameObjectInventory>();

			weaponGraphics = GetComponent<WeaponGraphicsController>();

			machine = GetComponent<PlayerMachine>();


			collider.material.dynamicFriction = profile.dynamicFriction;

			GetComponent<Ragdoller>().RagdollEnabled = false;

			//Allow for custom gravity
			rb.useGravity = false;



			inventory.armour.onItemRemoved += OnArmourRemoved;
			inventory.OnDropItemEvent += OnDropItem;
			inventory.OnConsumeItemEvent += OnConsumeItem;
			inventory.OnSelectItemEvent += OnSelectItem;

			inventory.melee.onItemRemoved += OnEquipableItemRemoved;
			inventory.bow.onItemRemoved += OnEquipableItemRemoved;
			inventory.sideArm.onItemRemoved += OnEquipableItemRemoved;



			//start a fresh state
			machine.ChangeToState(machine.defaultState);

			inputReader.SwitchToGameplayInput();
			enabled = true;





			for (int i = 0; i < 3; i++)
			{
				//Apply the chosen armour to the player on load
				if (armourSelections[i] != -1)
				{
					var selected = (ArmourItemData)inventory.armour.ItemAt(armourSelections[i]).item;
					//Will automatically remove the old armour piece
					weaponGraphics.characterMesh.SetClothing((CharacterMeshController.ClothPosition)selected.armourPosition, selected.hideBody, true, selected.armaturePrefab);
				}
			}

			for (int i = 1; i < 5; i++)
			{
				ItemType t = (ItemType)i;
				if (itemSelections[t] >= inventory.StackCount(t))
				{
					Debug.LogWarning($"Selected {t}, {itemSelections[t]} is out of range");
					itemSelections[t] = inventory.StackCount(t) - 1;
				}

				if (itemSelections[t] >= 0)
				{
					ItemData item = inventory.ItemAt(itemSelections[t], t).item;
					if (item is HoldableItemData holdableItemData)
					{
						var x = weaponGraphics.holdables[t].SetHeld(holdableItemData);
						weaponGraphics.holdables[t].sheathed = sheathing[t];

						//OnSelectItem(t, selection);
					}
				}
			}
			//reinit sheathing to false after being used as cache for save data

			sheathing = new EquipmentSet<bool>(false, false, false);

		}


		public override void OnEnable()
		{
			inputReader.changeSelection += OnChangeSelection;
		}
		public override void OnDisable()
		{
			inputReader.changeSelection -= OnChangeSelection;
		}
		public void SetGravityDirection(Vector3 direction)
		{
			m_gravityDirection = direction;

			transform.up = WorldUp;
		}

		public void AfterLoaded()
		{

		}


		public void TriggerFallingDeath()
		{
			health.Die();
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
					weaponGraphics.characterMesh.RemoveClothing(i);
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
			machine.ChangeToState(deadState);

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

		public override Bounds bounds => collider.bounds;

		public override Vector3 velocity => rb.velocity;

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
			GameCameras.s.lockingMouse = false;
			Pause();
		}
		public void ResumeControl()
		{
			GameCameras.s.EnableControl();
			GameCameras.s.lockingMouse = true;
			Play();
		}




		public void OnDropItem(ItemType type, int itemIndex)
		{
			var item = (PhysicsItemData)inventory.GetPanelFor(type)[itemIndex].item;
			//TODO - Add way to drop multiple items
			if (inventory.TakeItem(itemIndex, type))
				ItemSpawner.SpawnItem(item, transform.position + Vector3.up * 0.1f + transform.forward, transform.rotation);
		}
		public void OnConsumeItem(ItemType type, int itemIndex)
		{
			if (type == ItemType.Potion)
			{
				//Consume the potion
				PotionItemUnique pot = inventory.potions.items[itemIndex];

				if (((PotionItemData)pot.item).effect == PotionEffect.Health)
				{
					health.SetHealth(health.health + pot.potency);
				}

				inventory.potions.TakeItem(itemIndex, 1);

			}
		}

		private float sqrMagTemp;
		// Update is called once per frame
		protected override void Update()
		{
			base.Update();


			cameraTrackingTransform.rotation = Quaternion.identity;
			cameraTrackingTransform.up = WorldUp;

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

		public List<ContactPoint> allCPs => machine.allCPs;
		private ItemType[] selectingSlot = null;


		private void FixedUpdate()
		{
			if (useGravity)
				rb.AddForce(m_gravityDirection * Physics.gravity.magnitude);
		}

		private void OnChangeSelection(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started && selectingSlot == null && !paused)
			{
				if (inputReader.GetActionState<float>(
					InputReader.groundActionMap, InputReader.GroundActionMapActions.EquipBow.ToString()
					) > 0.5f)
				{
					selectingSlot = new ItemType[] { ItemType.Bow, ItemType.Ammo };
				}
				else
				{
					selectingSlot = new ItemType[] { ItemType.Melee, ItemType.SideArm };
				}

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

		public void Warp(Vector3 newPosition)
		{
			rb.velocity = Vector3.zero;

			GameCameras.s.currentCamera.OnTargetObjectWarped(transform, newPosition - transform.position);
			transform.position = newPosition;

		}

		public IEnumerator FadeToActions(System.Action onFullFade = null, System.Action onFadeEnd = null)
		{
			float time = 1f;
			float fadeTime = 0.25f;

			fadeTime = Mathf.Clamp(fadeTime, 0, time / 2);

			// fadeoutImage.color = Color.clear; //Black with full transparency
			UIController.singleton.fadeoutImage.gameObject.SetActive(true);

			yield return UIController.singleton.Fade(0, 1, fadeTime, false);

			float fullyBlackTime = time - fadeTime * 2;
			var wait = new WaitForSecondsRealtime(fullyBlackTime * 0.5f);

			yield return wait;
			onFullFade?.Invoke();
			yield return wait;

			yield return UIController.singleton.Fade(1, 0, fadeTime, false);

			UIController.singleton.DisableFadeout();
			onFadeEnd?.Invoke();

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
					weaponGraphics.characterMesh.SetClothing((CharacterMeshController.ClothPosition)selected.armourPosition, selected.hideBody, true, selected.armaturePrefab);
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

						weaponGraphics.holdables[type].RemoveHeld();

						if (!weaponGraphics.holdables[type].sheathed)
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

							var x = weaponGraphics.holdables[type].SetHeld(holdableItemData);
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
			if (inventory.ammo.items.Count > index && index >= 0)
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
				StartCoroutine(weaponGraphics.holdables.bow.gameObject.GetComponent<Bow>().NotchNextArrow(ammo));
			}
		}
		public void RemoveNotchedArrow()
		{
			if (currentBow != -1)
			{
				weaponGraphics.holdables.bow.gameObject.GetComponent<Bow>().RemoveNotchedArrow();
			}
		}
		#region Holdables


		public IEnumerator SheathItem(ItemType type)
		{
			sheathing[type] = true;
			if (type == ItemType.Bow)
			{
				RemoveNotchedArrow();
			}
			yield return weaponGraphics.SheathItem(type, transitionSet);
			sheathing[type] = false;
		}

		public IEnumerator UnEquipAll(System.Action onComplete = null)
		{


			if (!weaponGraphics.holdables.melee.sheathed)
			{
				yield return SheathItem(ItemType.Melee);
			}
			if (!weaponGraphics.holdables.sidearm.sheathed)
			{
				yield return SheathItem(ItemType.SideArm);
			}
			if (!weaponGraphics.holdables.bow.sheathed)
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

		//show the sound on the minimap?


		public override void Knockout(float time)
		{
			knockoutState.time = time;
			machine.ChangeToState(knockoutState);
		}




		public PlayerController Read(in GameDataReader reader)
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

			sheathing = new EquipmentSet<bool>(reader.ReadBool(), reader.ReadBool(), reader.ReadBool());

			//	startingState = SymbolToType(reader.ReadChar());




			return this;
		}

		public void Write(in GameDataWriter writer)
		{
			writer.WritePrimitive(transform.position);
			Debug.Log(transform.position);
			writer.WritePrimitive(transform.rotation);

			writer.WritePrimitive(armourSelections[0]);
			writer.WritePrimitive(armourSelections[1]);
			writer.WritePrimitive(armourSelections[2]);

			writer.WritePrimitive(itemSelections[ItemType.Melee]);
			writer.WritePrimitive(itemSelections[ItemType.SideArm]);
			writer.WritePrimitive(itemSelections[ItemType.Bow]);
			writer.WritePrimitive(itemSelections[ItemType.Ammo]);

			writer.WritePrimitive(weaponGraphics.holdables.melee.sheathed);
			writer.WritePrimitive(weaponGraphics.holdables.sidearm.sheathed);
			writer.WritePrimitive(weaponGraphics.holdables.bow.sheathed);

			//Save the current state
			writer.WritePrimitive(machine.mainState.stateSymbol);
		}

		public PlayerController Init()
		{
			return this;
		}
	}
}