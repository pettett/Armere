using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using Yarn;



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


	public class ItemStackBase : IWriteableToBinary
	{
		public readonly ItemData item;
		public ItemStackBase(ItemData item)
		{
			if (item == null)
				throw new System.ArgumentException("Item cannot be null");

			this.item = item;
		}

		public string title => item.displayName;

		public string description => this switch
		{
			PotionItemUnique pot => pot.item.itemName switch
			{
				ItemName.HealingPotion => string.Concat(Enumerable.Repeat('♥', Mathf.RoundToInt(pot.potency))),
				_ => string.Empty
			},
			_ => string.Empty
		};


		public virtual void Write(in GameDataWriter writer)
		{
			writer.Write((int)item.itemName);
		}


	}
	public class ItemStackT<ItemDataT> : ItemStackBase where ItemDataT : ItemData
	{
		public ItemStackT(ItemDataT item) : base(item) { }
		public ItemDataT itemData
		{
			get => (ItemDataT)item;
		}
	}
	[CreateAssetMenu(menuName = "Game/Inventory")]
	public class InventoryController : SaveableSO, IVariableAddon
	{

		public struct ItemHistroy : IWriteableToBinary
		{
			public bool hasPickedUp;

			public void Write(in GameDataWriter writer)
			{
				writer.Write(hasPickedUp);
			}
		}
		ItemHistroy[] itemsHistroy;

		public ItemAddedEventChannelSO onItemAdded;

		public ItemDatabase db;

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
				ItemName item = (ItemName)System.Enum.Parse(typeof(ItemName), name);
				return new Value(ItemCount(item));
			}
			set => throw new System.NotImplementedException("Cannot set inventory values");
		}


		public InventoryPanel GetPanelFor(ItemType t)
		{
			switch (t)
			{
				case ItemType.Common: return common;
				case ItemType.Quest: return quest;
				case ItemType.Melee: return melee;
				case ItemType.SideArm: return sideArm;
				case ItemType.Ammo: return ammo;
				case ItemType.Bow: return bow;
				case ItemType.Currency: return currency;
				case ItemType.Armour: return armour;
				case ItemType.Potion: return potions;
				default: return null;
			}
		}

		public delegate T ReaderGenerator<T>(GameDataReader reader);


		public List<T> ReadList<T>(GameDataReader reader, ReaderGenerator<T> generator)
		{
			int count = reader.ReadInt();
			var list = new List<T>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(generator(reader));
			}
			return list;
		}



		public ItemStack ItemStackReader(GameDataReader reader) => new ItemStack(db[(ItemName)reader.ReadInt()], reader.ReadUInt());
		public ItemStackBase ItemStackBaseReader(GameDataReader reader) => new ItemStackBase(db[(ItemName)reader.ReadInt()]);
		public ItemStackT<ItemT> ItemStackTReader<ItemT>(GameDataReader reader) where ItemT : ItemData => new ItemStackT<ItemT>((ItemT)db[(ItemName)reader.ReadInt()]);

		public PotionItemUnique PotionItemUniqueReader(GameDataReader reader) =>
			new PotionItemUnique(db[(ItemName)reader.ReadInt()], reader.ReadFloat(), reader.ReadFloat());

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
			currency = new ValuePanel("Currency", int.MaxValue, ItemType.Currency, db);
		}
		public override void LoadBlank()
		{
			itemsHistroy = new ItemHistroy[db.itemData.Length];
		}
		public override void LoadBin(in GameDataReader reader)
		{
			//Debug.Log("Loading inventory...");


			common.items = ReadList<ItemStack>(reader, ItemStackReader);
			quest.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);
			melee.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);
			sideArm.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);
			bow.items = ReadList<ItemStackT<BowItemData>>(reader, ItemStackTReader<BowItemData>);
			armour.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);

			potions.items = ReadList<PotionItemUnique>(reader, PotionItemUniqueReader);
			ammo.items = ReadList<ItemStack>(reader, ItemStackReader);

			currency.currency = reader.ReadUInt();

			itemsHistroy = new ItemHistroy[db.itemData.Length];

			if (reader.saveVersion > new Version(0, 0, 2))
			{
				int saved = reader.ReadInt();
				for (int i = 0; i < saved; i++)
				{
					itemsHistroy[i].hasPickedUp = reader.ReadBool();
				}
			}

			//Debug.Log($"Loaded inventory: {currency.currency}");
		}
		public override void SaveBin(in GameDataWriter writer)
		{
			writer.WriteList(common.items);
			writer.WriteList(quest.items);
			writer.WriteList(melee.items);
			writer.WriteList(sideArm.items);
			writer.WriteList(bow.items);
			writer.WriteList(armour.items);
			writer.WriteList(potions.items);
			writer.WriteList(ammo.items);
			writer.Write(currency.currency);


			writer.WriteList(itemsHistroy);
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
			InventoryPanel p = GetPanelFor(item.type);
			var addedIndex = p.AddItem(item, count, desiredPosition);

			if (!hiddenAddition && addedIndex != -1 && p.type != ItemType.Currency)
			{
				onItemAdded.OnItemAdded(p[addedIndex], item.type, addedIndex, hiddenAddition);
				IncreaseItemHistroy(item.itemName);
			}

			return addedIndex != -1;
		}
		public bool TryAddItem(int index, ItemType type, uint count, bool hiddenAddition)
		{
			var b = GetPanelFor(type).AddItem(index, count);
			if (b) //Use itemat command to find the type of item that was added
			{
				onItemAdded.OnItemAdded(ItemAt(index, type), type, index, hiddenAddition);
				IncreaseItemHistroy(ItemAt(index, type).item.itemName);
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
				IncreaseItemHistroy(stack.item.itemName);
			}
			return b;
		}

		void IncreaseItemHistroy(ItemName name)
		{
			if (!itemsHistroy[(int)name].hasPickedUp)
			{
				UI.NewItemPrompt.singleton.ShowPrompt(db[name], 1, null, addItemsToInventory: false);
				itemsHistroy[(int)name].hasPickedUp = true;
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
		public uint ItemCount(ItemName n) => GetPanelFor(db[n].type).ItemCount(n);
		public uint ItemCount(int index, ItemType type) => GetPanelFor(type).ItemCount(index);
		public ItemStackBase ItemAt(int index, ItemType type) => GetPanelFor(type)[index];

		public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
		{
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}