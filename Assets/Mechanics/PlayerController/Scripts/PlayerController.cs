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
	[RequireComponent(typeof(Rigidbody))]
	public class PlayerController : Character, IInteractor
	{
		public enum WeaponSet { MeleeSidearm, BowArrow }

		public SpellUnlockTree spellTree;

		[NonSerialized] public MovementState currentState;

		//Parallel state list:

		//Camera Controller - DONE
		//Weapons - DONE
		//Interaction with objects - DONE

		[NonSerialized] private MovementState[] parallelStates = new MovementState[0];



		[NonSerialized] private MovementState[] allStates;


		[Header("Cameras")]

		[NonSerialized] public Room currentRoom;
		public Transform cameraTrackingTransform;



		[Header("Movement")]


		public bool useGravity = true;

		public Vector3 m_gravityDirection { get; private set; } = Vector3.down;


		public Vector3 WorldUp => -m_gravityDirection;
		public Vector3 WorldDown => m_gravityDirection;







		[Header("States")]
		public MovementStateTemplate defaultState;
		public MovementStateTemplate deadState;
		public KnockedOutTemplate knockoutState;
		public AutoWalkingTemplate autoWalk;


		[Header("Other")]

		public LayerMask m_groundLayerMask;
		public LayerMask m_waterLayerMask;
		[NonSerialized] public Rigidbody rb;
		[NonSerialized] new public CapsuleCollider collider;
		[NonSerialized] public Health health;

		//set capacity to 1 as it is common for the player to be touching the ground in at least one point
		[NonSerialized] public List<ContactPoint> allCPs = new List<ContactPoint>(1);

		private System.Text.StringBuilder _entry;

		private System.Text.StringBuilder entry
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

		public int[] armourSelections => playerSaveData.armourSelections;
		public Dictionary<ItemType, int> itemSelections => playerSaveData.itemSelections;

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
		public PlayerSaveData playerSaveData;


		[NonSerialized] public EquipmentSet<bool> sheathing = new EquipmentSet<bool>(false, false, false);


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

			inventory.armour.onItemRemoved -= OnArmourRemoved;
			inventory.OnDropItemEvent -= OnDropItem;
			inventory.OnConsumeItemEvent -= OnConsumeItem;
			inventory.OnSelectItemEvent -= OnSelectItem;
			inventory.melee.onItemRemoved -= OnEquipableItemRemoved;
			inventory.bow.onItemRemoved -= OnEquipableItemRemoved;
			inventory.sideArm.onItemRemoved -= OnEquipableItemRemoved;

			if (allStates != null)
				foreach (var s in allStates) s.End();
		}


		protected override void OnCollisionEnter(Collision col)
		{
			allCPs.Capacity += col.contactCount;
			for (int i = 0; i < col.contactCount; i++)
			{
				allCPs.Add(col.GetContact(i));
			}
			base.OnCollisionEnter(col);
		}
		private void OnCollisionStay(Collision col)
		{
			allCPs.Capacity += col.contactCount;
			for (int i = 0; i < col.contactCount; i++)
			{
				allCPs.Add(col.GetContact(i));
			}
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

			playerSaveData.c = this;

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
						weaponGraphics.holdables[t].sheathed = playerSaveData.sheathedItems[t];

						//OnSelectItem(t, selection);
					}
				}
			}

			//start a fresh state
			ChangeToState(playerSaveData.startingState ?? defaultState);

			inputReader.SwitchToGameplayInput();
			enabled = true;

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
			ChangeToState(deadState);

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


		public bool StateActive(int i) => !paused || paused && allStates[i].updateWhilePaused;


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

			for (int i = 0; i < allStates.Length; i++)
				if (StateActive(i))
					allStates[i].Update();

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
			if (useGravity)
				rb.AddForce(m_gravityDirection * Physics.gravity.magnitude);


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
		public void ChangeToStateTimed(MovementStateTemplate timedState, float time, MovementStateTemplate returnedState = null) =>
			StartCoroutine(ChangeToStateTimedRoutine(timedState, time, returnedState));

		IEnumerator ChangeToStateTimedRoutine(MovementStateTemplate timedState, float time, MovementStateTemplate returnedState = null)
		{
			ChangeToState(timedState);
			yield return new WaitForSeconds(time);
			//Returned state or default state if it is null
			ChangeToState(returnedState ?? defaultState);
		}


		public MovementState ChangeToState(MovementStateTemplate t)
		{
			currentState?.End(); // state specific end method
			currentState = t.StartState(this);


			//go through and end all the parallel states not used by this state
			for (int i = 0; i < parallelStates.Length; i++)
			{

				Type stateType = parallelStates[i].GetType();
				for (int j = 0; j < t.parallelStates.Length; j++)
					if (stateType == t.parallelStates[j].StateType())
						goto DoNotEndState;

				//Goto saves time here - ending state will only be skipped if it is required
				parallelStates[i].End();
			DoNotEndState:
				continue;
			}


			MovementState[] newStates = new MovementState[t.parallelStates.Length];

			for (int i = 0; i < t.parallelStates.Length; i++)
			{
				//test if the desired parallel state is currently active
				newStates[i] = GetParallelState(t.parallelStates[i].StateType());
				if (newStates[i] == null)
				{
					newStates[i] = t.parallelStates[i].StartState(this);
				}
			}
			parallelStates = newStates;




			if (DebugMenu.menuEnabled && entry != null)
			{
				//Show all the currentley active states

				//update f3 information
				entry.Clear();
				entry.Append("Current State: ");
				entry.Append(currentState.StateName);
				if (parallelStates.Length != 0)
				{
					entry.Append('{');
					foreach (var s in parallelStates)
					{
						entry.Append(s.StateName);
						entry.Append(',');
					}
					entry.Append('}');
				}
			}
			FillAllStates();

			StartAllStates();

			StateChanged();
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
		public void StartAllStates()
		{

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


		public override void Knockout(float time)
		{
			knockoutState.time = time;
			ChangeToState(knockoutState);
		}
	}
}