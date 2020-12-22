using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using Yarn;

namespace Armere.Inventory
{

    public delegate void InventoryNewItemDelegate(ItemStackBase item, ItemType type, int index, bool hiddenAddition);
    public delegate void InventoryOptionDelegate(ItemType type, int itemIndex);

    [System.Serializable]
    public class ItemStackBase
    {
        public ItemName name;

        public ItemStackBase(ItemName name)
        {
            this.name = name;
        }
    }


    public class InventoryController : MonoBehaviour, IVariableAddon
    {
        public InventoryNewItemDelegate onItemAdded;

        public ItemDatabase db;
        const string itemPrefix = "$Item_";

        public string prefix => itemPrefix;

        public Value this[string name]
        {
            get
            {
                ItemName item = (ItemName)System.Enum.Parse(typeof(ItemName), name.Substring(itemPrefix.Length));
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
                case ItemType.Potion: return potions;
                default: return null;
            }
        }



        [System.Serializable]
        public class InventorySave
        {
            public List<ItemStack> common;
            public List<ItemStackBase> quest;
            public List<ItemStackBase> weapon;
            public List<ItemStackBase> sideArm;
            public List<ItemStackBase> bow;
            public List<ItemStack> ammo;
            public List<PotionItemUnique> potions;
            public uint currency;
            public InventorySave()
            {
                common = new List<ItemStack>();
                quest = new List<ItemStackBase>();
                weapon = new List<ItemStackBase>();
                sideArm = new List<ItemStackBase>();
                bow = new List<ItemStackBase>();
                ammo = new List<ItemStack>();
                potions = new List<PotionItemUnique>();
                currency = 0;
            }

            public InventorySave(InventoryController c)
            {
                common = c.common.items;
                quest = c.quest.items;
                weapon = c.melee.items;
                sideArm = c.sideArm.items;
                bow = c.bow.items;
                ammo = c.ammo.items;
                potions = c.potions.items;
                currency = c.currency.currency;
            }
        }

        public InventorySave CreateSave()
        {
            return new InventorySave(this);
        }


        public StackPanel<ItemStack> common;
        public UniquesPanel<ItemStackBase> quest;
        public UniquesPanel<ItemStackBase> melee;
        public UniquesPanel<ItemStackBase> bow;
        public StackPanel<ItemStack> ammo;
        public UniquesPanel<ItemStackBase> sideArm;
        public UniquesPanel<PotionItemUnique> potions;

        public ValuePanel currency;



        public static InventoryController singleton;
        public event InventoryOptionDelegate OnSelectItemEvent;
        public event InventoryOptionDelegate OnDropItemEvent;


        public void OnSelectItem(ItemType type, int itemIndex)
        {
            print("Selected " + itemIndex.ToString());
            OnSelectItemEvent?.Invoke(type, itemIndex);
        }
        public void OnDropItem(ItemType type, int itemIndex)
        {
            print("Dropped " + itemIndex.ToString());
            OnDropItemEvent?.Invoke(type, itemIndex);
        }

        private void Awake()
        {

            singleton = this;

        }
        private void Start()
        {
            DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(this);
        }

        public void OnSaveStateLoaded(InventorySave save)
        {

            common = new StackPanel<ItemStack>("Items", int.MaxValue, ItemType.Common);
            quest = new UniquesPanel<ItemStackBase>("Quest Items", int.MaxValue, ItemType.Quest);
            melee = new UniquesPanel<ItemStackBase>("Weapons", 10, ItemType.Melee, OnSelectItem, OnDropItem);
            sideArm = new UniquesPanel<ItemStackBase>("Side Arms", 10, ItemType.SideArm, OnSelectItem, OnDropItem);
            bow = new UniquesPanel<ItemStackBase>("Bows", 10, ItemType.Bow, OnSelectItem, OnDropItem);

            potions = new UniquesPanel<PotionItemUnique>("Potions", int.MaxValue, ItemType.Potion);

            ammo = new StackPanel<ItemStack>("Ammo", int.MaxValue, ItemType.Ammo, OnSelectItem, OnDropItem);
            currency = new ValuePanel("Currency", int.MaxValue, ItemType.Currency);

            common.items = save.common;
            quest.items = save.quest;
            melee.items = save.weapon;
            sideArm.items = save.sideArm;
            bow.items = save.bow;
            ammo.items = save.ammo;
            potions.items = save.potions;
            currency.currency = save.currency;

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
        public static bool TakeItem(int index, ItemType type, uint count = 1) => singleton.GetPanelFor(type).TakeItem(index, count);
        public static uint ItemCount(ItemName n) => singleton.GetPanelFor(singleton.db[n].type).ItemCount(n);
        public static uint ItemCount(int index, ItemType type) => singleton.GetPanelFor(type).ItemCount(index);
        public static ItemStackBase ItemAt(int index, ItemType type) => singleton.GetPanelFor(type)[index];

    }
}