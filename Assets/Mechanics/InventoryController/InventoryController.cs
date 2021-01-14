using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using Yarn;

public interface IWriteableToBinary
{
    void Write(GameDataWriter writer);
}


namespace Armere.Inventory
{

    public delegate void InventoryNewItemDelegate(ItemStackBase item, ItemType type, int index, bool hiddenAddition);
    public delegate void InventoryOptionDelegate(ItemType type, int itemIndex);

    [Flags]
    public enum ItemInteractionCommands
    {
        None = 0,
        Drop = 1 << 0,
        Equip = 1 << 1,
        Consume = 1 << 2
    }


    [System.Serializable]
    public class ItemStackBase : IWriteableToBinary
    {
        public readonly ItemName name;

        public ItemStackBase(ItemName name)
        {
            this.name = name;
        }

        public virtual void Write(GameDataWriter writer)
        {
            writer.Write((int)name);
        }
    }


    public class InventoryController : MonoSaveable, IVariableAddon
    {
        public InventoryNewItemDelegate onItemAdded;

        public ItemDatabase db;
        const string itemPrefix = "$Item_";

        public string prefix => itemPrefix;

        public Value this[string name]
        {
            get
            {
                ItemName item = (ItemName)System.Enum.Parse(typeof(ItemName), name);
                return new Value(InventoryController.ItemCount(item));
            }
            set => throw new System.NotImplementedException("Cannot set stage of quest");
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

        public void WriteList<T>(GameDataWriter writer, List<T> items) where T : IWriteableToBinary
        {
            writer.Write(items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Write(writer);
            }
        }

        public ItemStack ItemStackReader(GameDataReader reader) => new ItemStack((ItemName)reader.ReadInt(), reader.ReadUInt());
        public ItemStackBase ItemStackBaseReader(GameDataReader reader) => new ItemStackBase((ItemName)reader.ReadInt());
        public PotionItemUnique PotionItemUniqueReader(GameDataReader reader) =>
            new PotionItemUnique((ItemName)reader.ReadInt(), reader.ReadFloat(), reader.ReadFloat());

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
            melee = new UniquesPanel<ItemStackBase>("Weapons", 10, ItemType.Melee, ItemInteractionCommands.Equip | ItemInteractionCommands.Drop);
            sideArm = new UniquesPanel<ItemStackBase>("Side Arms", 10, ItemType.SideArm, ItemInteractionCommands.Equip | ItemInteractionCommands.Drop);
            bow = new UniquesPanel<ItemStackBase>("Bows", 10, ItemType.Bow, ItemInteractionCommands.Equip | ItemInteractionCommands.Drop);
            armour = new UniquesPanel<ItemStackBase>("Armour", int.MaxValue, ItemType.Armour, ItemInteractionCommands.Equip);
            potions = new UniquesPanel<PotionItemUnique>("Potions", int.MaxValue, ItemType.Potion, ItemInteractionCommands.Consume);
            ammo = new StackPanel<ItemStack>("Ammo", int.MaxValue, ItemType.Ammo, ItemInteractionCommands.Equip);
            currency = new ValuePanel("Currency", int.MaxValue, ItemType.Currency);
        }
        public override void LoadBlank()
        {
            CreatePanels();
        }
        public override void LoadBin(Version saveVersion, GameDataReader reader)
        {
            //Debug.Log("Loading inventory...");
            CreatePanels();

            common.items = ReadList<ItemStack>(reader, ItemStackReader);
            quest.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);
            melee.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);
            sideArm.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);
            bow.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);
            armour.items = ReadList<ItemStackBase>(reader, ItemStackBaseReader);

            potions.items = ReadList<PotionItemUnique>(reader, PotionItemUniqueReader);
            ammo.items = ReadList<ItemStack>(reader, ItemStackReader);

            currency.currency = reader.ReadUInt();

            //Debug.Log($"Loaded inventory: {currency.currency}");
        }
        public override void SaveBin(GameDataWriter writer)
        {
            WriteList(writer, common.items);
            WriteList(writer, quest.items);
            WriteList(writer, melee.items);
            WriteList(writer, sideArm.items);
            WriteList(writer, bow.items);
            WriteList(writer, armour.items);
            WriteList(writer, potions.items);
            WriteList(writer, ammo.items);
            writer.Write(currency.currency);
        }



        public StackPanel<ItemStack> common;
        public UniquesPanel<ItemStackBase> quest;
        public UniquesPanel<ItemStackBase> melee;
        public UniquesPanel<ItemStackBase> bow;
        public UniquesPanel<ItemStackBase> armour;
        public StackPanel<ItemStack> ammo;
        public UniquesPanel<ItemStackBase> sideArm;
        public UniquesPanel<PotionItemUnique> potions;

        public ValuePanel currency;



        public static InventoryController singleton;
        public event InventoryOptionDelegate OnSelectItemEvent;
        public event InventoryOptionDelegate OnDropItemEvent;
        public event InventoryOptionDelegate OnConsumeItemEvent;


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

        private void Awake()
        {
            if (singleton == null)
                singleton = this;

        }
        private void Start()
        {
            DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(this);
        }






        public static bool AddItem(ItemName n, uint count, bool hiddenAddition)
        {
            InventoryPanel p = singleton.GetPanelFor(singleton.db[n].type);
            var addedIndex = p.AddItem(n, count);

            if (!hiddenAddition && addedIndex != -1 && p.type != ItemType.Currency)
            {
                InventoryController.singleton.onItemAdded?.Invoke(p[addedIndex], singleton.db[n].type, addedIndex, hiddenAddition);
            }

            return addedIndex != -1;
        }


        public static bool TakeItem(ItemName n, uint count = 1) => singleton.GetPanelFor(singleton.db[n].type).TakeItem(n, count);

        public static bool AddItem(int index, ItemType type, uint count, bool hiddenAddition)
        {
            var b = singleton.GetPanelFor(type).AddItem(index, count);
            if (b) //Use itemat command to find the type of item that was added
                singleton.onItemAdded?.Invoke(ItemAt(index, type), type, index, hiddenAddition);
            return b;
        }

        public static bool AddItem(ItemStackBase stack, bool hiddenAddition = false)
        {
            ItemType type = singleton.db[stack.name].type;
            var b = singleton.GetPanelFor(type).AddItem(stack);
            if (b) //Use itemat command to find the type of item that was added
                singleton.onItemAdded?.Invoke(stack, type, singleton.GetPanelFor(type).stackCount - 1, hiddenAddition);
            return b;
        }

        public static bool TakeItem(int index, ItemType type, uint count = 1) => singleton.GetPanelFor(type).TakeItem(index, count);
        public static uint ItemCount(ItemName n) => singleton.GetPanelFor(singleton.db[n].type).ItemCount(n);
        public static uint ItemCount(int index, ItemType type) => singleton.GetPanelFor(type).ItemCount(index);
        public static ItemStackBase ItemAt(int index, ItemType type) => singleton.GetPanelFor(type)[index];


    }
}