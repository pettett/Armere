using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using Yarn;
using UnityEngine.Assertions;

namespace Armere.Inventory
{

	public delegate void InventoryOptionDelegate(ItemType type, int itemIndex);

	[Flags]
	public enum ItemInteractionCommands
	{
		None = 0,
		Drop = 1 << 0,
		Equip = 1 << 1,
		Consume = 1 << 2
	}


	public class ItemStackBase : IGameDataSavableAsync<ItemStackBase>
	{
		public readonly ItemData item;

		public ItemStackBase()
		{
			item = null;//Bad
		}

		public ItemStackBase(ItemData item)
		{
			if (item == null)
				throw new System.ArgumentException("Item cannot be null");

			this.item = item;
		}

		public string title => item.displayName;

		public string description => this switch
		{
			PotionItemUnique pot => ((PotionItemData)pot.item).effect switch
			{
				PotionEffect.Health => string.Concat(Enumerable.Repeat('♥', Mathf.RoundToInt(pot.potency))),
				_ => string.Empty
			},
			_ => string.Empty
		};

		public ItemStackBase Init()
		{
			throw new NotImplementedException();
		}

		public void Read(in GameDataReader reader, System.Action<ItemStackBase> onDone)
		{
			reader.ReadAsync<ItemDataAsyncSerializer>(item =>
			{
				onDone?.Invoke(new ItemStackBase(item));
			});
		}

		public virtual void Write(in GameDataWriter writer)
		{
			writer.Write(item);
		}
	}
	public class ItemStackT<ItemDataT> : ItemStackBase, IGameDataSavableAsync<ItemStackT<ItemDataT>> where ItemDataT : ItemData
	{
		public ItemStackT()
		{
		}

		public ItemStackT(ItemDataT item) : base(item) { }
		public ItemDataT itemData
		{
			get => (ItemDataT)item;
		}

		public new ItemStackT<ItemDataT> Init()
		{
			return this;
		}

		public void Read(in GameDataReader reader, System.Action<ItemStackT<ItemDataT>> onDone)
		{
			ItemDatabase.ReadItemData(reader, item =>
			{
				onDone?.Invoke(new ItemStackT<ItemDataT>((ItemDataT)item));
			});
		}

	}
	[CreateAssetMenu(menuName = "Game/Inventory/Inventory Controller")]
	public class InventoryController : ScriptableObject, IGameDataSavableAsync<InventoryController>, IVariableAddon
	{

		public struct ItemHistroy : IGameDataSavable<ItemHistroy>
		{
			public bool hasPickedUp;

			public ItemHistroy Init()
			{
				return this;
			}

			public ItemHistroy Read(in GameDataReader reader)
			{
				hasPickedUp = reader.ReadBool();
				return this;
			}
			public void Write(in GameDataWriter writer)
			{
				writer.WritePrimitive(hasPickedUp);
			}
		}
		Dictionary<(ulong, ulong), ItemHistroy> itemsHistroy = new();

		public ItemAddedEventChannelSO onItemAdded;


		const string itemPrefix = "$Item_";

		public string prefix => itemPrefix;


		public uint weaponLimit = 7;
		public uint sidearmLimit = 7;
		public uint bowLimit = 7;


		public event System.Action<ItemData, System.Action> onTriggerReplaceItemDialogue;



		public Value this[string name]
		{
			get
			{
				return new Value(ItemCount(name));
			}
			set => throw new System.NotImplementedException("Cannot set inventory values");
		}

		public bool HasMeleeWeapon => melee.items.Count > 0;
		public bool HasBowWeapon => bow.items.Count > 0;

		public int BestMeleeWeapon
		{
			get
			{
				int best = -1;
				for (int i = 0; i < melee.items.Count; i++)
				{
					if (best == -1 || ((MeleeWeaponItemData)melee.items[best].item).damage < ((MeleeWeaponItemData)melee.items[i].item).damage)
					{
						best = i;
					}
				}

				return best;
			}
		}


		public MeleeWeaponItemData SelectedMeleeWeapon => (MeleeWeaponItemData)melee.items[selectedMelee].item;



		public InventoryPanel GetPanelFor(ItemType t) => panels[(int)t];

		//Save Order:
		/*
		common - List
		quest - List
		melee - List
		sideArm - List
		bow - List
		armour - List
		potions - List
		ammo - List

		currency - Uint
		*/

		public void CreatePanels()
		{
			common = new StackPanel<ItemStack>("Items", int.MaxValue, ItemType.Common, ItemInteractionCommands.None);
			quest = new UniquesPanel<ItemStackBase>("Quest Items", int.MaxValue, ItemType.Quest, ItemInteractionCommands.None);
			melee = new UniquesPanel<ItemStackBase>("Weapons", weaponLimit, ItemType.Melee, ItemInteractionCommands.Equip | ItemInteractionCommands.Drop);
			sideArm = new UniquesPanel<ItemStackBase>("Side Arms", sidearmLimit, ItemType.SideArm, ItemInteractionCommands.Equip | ItemInteractionCommands.Drop);
			bow = new UniquesPanel<ItemStackT<BowItemData>>("Bows", bowLimit, ItemType.Bow, ItemInteractionCommands.Equip | ItemInteractionCommands.Drop);
			armour = new UniquesPanel<ItemStackBase>("Armour", int.MaxValue, ItemType.Armour, ItemInteractionCommands.Equip);
			potions = new UniquesPanel<PotionItemUnique>("Potions", int.MaxValue, ItemType.Potion, ItemInteractionCommands.Consume);
			ammo = new StackPanel<ItemStack>("Ammo", int.MaxValue, ItemType.Ammo, ItemInteractionCommands.Equip);
			currency = new ValuePanel("Currency", int.MaxValue, ItemType.Currency);

			panels[(int)ItemType.Common] = common;
			panels[(int)ItemType.Quest] = quest;
			panels[(int)ItemType.Melee] = melee;
			panels[(int)ItemType.SideArm] = sideArm;
			panels[(int)ItemType.Bow] = bow;
			panels[(int)ItemType.Armour] = armour;
			panels[(int)ItemType.Potion] = potions;
			panels[(int)ItemType.Ammo] = ammo;
			panels[(int)ItemType.Currency] = currency;
		}

		private void OnEnable()
		{
			CreatePanels();
		}

		private void Start()
		{
			DialogueInstances.singleton.variableStorage.addons.Add(this);
		}


		public StackPanel<ItemStack> common;
		public UniquesPanel<ItemStackBase> quest;
		public UniquesPanel<ItemStackBase> melee;
		public UniquesPanel<ItemStackT<BowItemData>> bow;
		public UniquesPanel<ItemStackBase> armour;
		public StackPanel<ItemStack> ammo;
		public UniquesPanel<ItemStackBase> sideArm;
		public UniquesPanel<PotionItemUnique> potions;

		public ValuePanel currency;



		public readonly InventoryPanel[] panels = new InventoryPanel[9];

		public event InventoryOptionDelegate OnSelectItemEvent;
		public event InventoryOptionDelegate OnDropItemEvent;
		public event InventoryOptionDelegate OnConsumeItemEvent;


		public int selectedHelmet;
		public int selectedChest;
		public int selectedBoots;
		public int selectedMelee;
		public int selectedBow;
		public int selectedSidearm;
		public int selectedArrow;


		public bool TryAddItem(ItemData item, uint count, bool hiddenAddition, int desiredPosition = -1)
		{
			Assert.IsNotNull(item, "Item cannot be none");

			InventoryPanel p = GetPanelFor(item.type);
			var addedIndex = p.AddItem(item, count, desiredPosition);

			if (!hiddenAddition && addedIndex != -1 && p.type != ItemType.Currency)
			{
				onItemAdded.OnItemAdded(p[addedIndex], item.type, addedIndex, hiddenAddition);
				IncreaseItemHistroy(item);
			}

			return addedIndex != -1;
		}

		public bool TryAddItem(int index, ItemType type, uint count, bool hiddenAddition)
		{
			var b = GetPanelFor(type).AddItem(index, count);
			if (b) //Use itemat command to find the type of item that was added
			{
				onItemAdded.OnItemAdded(ItemAt(index, type), type, index, hiddenAddition);
				IncreaseItemHistroy(ItemAt(index, type).item);
			}
			return b;
		}

		public bool TryAddItem(ItemStackBase stack, bool hiddenAddition = false)
		{
			ItemType type = stack.item.type;
			var b = GetPanelFor(type).AddItem(stack);
			if (b) //Use itemat command to find the type of item that was added
			{
				onItemAdded.OnItemAdded(stack, type, GetPanelFor(type).stackCount - 1, hiddenAddition);
				IncreaseItemHistroy(stack.item);
			}
			return b;
		}

		void IncreaseItemHistroy(ItemData name)
		{
			ItemHistroy hist;
			if (!itemsHistroy.TryGetValue(ItemDatabase.itemDataPrimaryKeys[name], out hist))
				hist = new ItemHistroy();


			if (!hist.hasPickedUp)
			{
				UI.NewItemPrompt.singleton.ShowPrompt(name, 1, null, addItemsToInventory: false);
				hist.hasPickedUp = true;
				itemsHistroy[ItemDatabase.itemDataPrimaryKeys[name]] = hist;
			}
		}



		public void OnSelectItem(ItemType type, int itemIndex)
		{
			//print("Selected " + itemIndex.ToString());
			OnSelectItemEvent?.Invoke(type, itemIndex);
		}
		public void OnConsumeItem(ItemType type, int itemIndex)
		{
			//print("Consumed " + itemIndex.ToString());
			OnConsumeItemEvent?.Invoke(type, itemIndex);
		}
		public void OnDropItem(ItemType type, int itemIndex)
		{
			//print("Dropped " + itemIndex.ToString());
			OnDropItemEvent?.Invoke(type, itemIndex);
		}








		public void ReplaceItemDialogue(ItemData item, System.Action onReplaced)
		{
			//Tell the UI to start a replacement

			onTriggerReplaceItemDialogue?.Invoke(item, onReplaced);
		}


		public bool TakeItem(ItemData item, uint count = 1) => GetPanelFor(item.type).TakeItem(item, count);


		public void ReplaceItem(int index, ItemData item)
		{
			//TODO: If the player has this item equipped, it will be unequipped.
			// Need a different event for item replaced
			TakeItem(index, item.type);
			TryAddItem(item, 1, false, index);
		}





		public bool TakeItem(int index, ItemType type, uint count = 1) => GetPanelFor(type).TakeItem(index, count);
		public uint ItemCount(ItemData item) => GetPanelFor(item.type).ItemCount(item);
		public uint ItemCount(int index, ItemType type) => GetPanelFor(type).ItemCount(index);
		public int StackCount(ItemType type) => GetPanelFor(type).stackCount;
		public ItemStackBase ItemAt(int index, ItemType type) => GetPanelFor(type)[index];

		public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
		{
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Read(in GameDataReader reader, Action<InventoryController> data)
		{
			uint loaded = 0;
			const uint asyncLoads = 8;
			void OnLoaded<T>(T inp) where T : InventoryPanel
			{
				loaded++;

				if (loaded == asyncLoads)
				{
					data.Invoke(this);
				}
			}
			Debug.Log("Loading inv");

			reader.ReadAsyncInto(common, OnLoaded);
			reader.ReadAsyncInto(quest, OnLoaded);
			reader.ReadAsyncInto(melee, OnLoaded);
			reader.ReadAsyncInto(sideArm, OnLoaded);
			reader.ReadAsyncInto(bow, OnLoaded);
			reader.ReadAsyncInto(armour, OnLoaded);
			reader.ReadAsyncInto(potions, OnLoaded);
			reader.ReadAsyncInto(ammo, OnLoaded);

			reader.ReadInto(currency);

		}


		public void Write(in GameDataWriter writer)
		{
			writer.Write(common);
			writer.Write(quest);
			writer.Write(melee);
			writer.Write(sideArm);
			writer.Write(bow);
			writer.Write(armour);
			writer.Write(potions);
			writer.Write(ammo);
			writer.Write(currency);
		}

		public InventoryController Init()
		{
			return this;
		}


		public void RegisterForCommands()
		{
			string prefix = name.Split(' ')[0].ToLower();

			//ItemData item, uint count, bool hiddenAddition
			Armere.Console.Console.RegisterCommand(prefix + ".additem", (object[] args) =>
			{
				uint count = (args[1] as uint?).GetValueOrDefault(1);

				ItemDatabase.LoadItemDataAsync<ItemData>(args[0], x => TryAddItem(x, count, true));
			}, "itemdata", "u32");
		}

	}
}