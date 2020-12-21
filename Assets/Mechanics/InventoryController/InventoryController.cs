using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Armere.Inventory
{

    public delegate void InventoryNewItemDelegate(ItemName item, bool hiddenAddition);
    public delegate void InventoryOptionDelegate(ItemType type, int itemIndex);

    [System.Serializable]
    public class ItemStackBase
    {
        public readonly ItemName name;

        public ItemStackBase(ItemName name)
        {
            this.name = name;
        }
    }


    public class InventoryController : MonoBehaviour
    {
        public InventoryNewItemDelegate onItemAdded;

        public ItemDatabase db;

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
                default: return null;
            }
        }



        [System.Serializable]
        public class InventorySave
        {
            public List<StackPanel.ItemStack> common;
            public List<ItemStackBase> quest;
            public List<ItemStackBase> weapon;
            public List<ItemStackBase> sideArm;
            public List<ItemStackBase> bow;
            public List<StackPanel.ItemStack> ammo;
            public uint currency;
            public InventorySave()
            {
                common = new List<StackPanel.ItemStack>();
                quest = new List<ItemStackBase>();
                weapon = new List<ItemStackBase>();
                sideArm = new List<ItemStackBase>();
                bow = new List<ItemStackBase>();
                ammo = new List<StackPanel.ItemStack>();
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
                currency = c.currency.currency;
            }
        }

        public InventorySave CreateSave()
        {
            return new InventorySave(this);
        }


        public StackPanel common;
        public UniquesPanel quest;
        public UniquesPanel melee;
        public UniquesPanel bow;
        public StackPanel ammo;
        public UniquesPanel sideArm;
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
        public void OnSaveStateLoaded(InventorySave save)
        {

            common = new StackPanel("Items", int.MaxValue, ItemType.Common);
            quest = new UniquesPanel("Quest Items", int.MaxValue, ItemType.Quest);
            melee = new UniquesPanel("Weapons", 10, ItemType.Melee, OnSelectItem, OnDropItem);
            sideArm = new UniquesPanel("Side Arms", 10, ItemType.SideArm, OnSelectItem, OnDropItem);
            bow = new UniquesPanel("Bows", 10, ItemType.Bow, OnSelectItem, OnDropItem);
            ammo = new StackPanel("Ammo", int.MaxValue, ItemType.Ammo, OnSelectItem, OnDropItem);
            currency = new ValuePanel("Currency", int.MaxValue, ItemType.Currency);

            common.items = save.common;
            quest.items = save.quest;
            melee.items = save.weapon;
            sideArm.items = save.sideArm;
            bow.items = save.bow;
            ammo.items = save.ammo;
            currency.currency = save.currency;

        }


        public static bool AddItem(ItemName n, uint count, bool hiddenAddition)
        {
            var b = singleton.GetPanelFor(singleton.db[n].type).AddItem(n, count);
            if (b)
                singleton.onItemAdded?.Invoke(n, hiddenAddition);
            return b;
        }


        public static bool TakeItem(ItemName n, uint count = 1) => singleton.GetPanelFor(singleton.db[n].type).TakeItem(n, count);

        public static bool AddItem(int index, ItemType type, uint count, bool hiddenAddition)
        {
            var b = singleton.GetPanelFor(type).AddItem(index, count);
            if (b) //Use itemat command to find the type of item that was added
                singleton.onItemAdded?.Invoke(singleton.GetPanelFor(type).ItemAt(index), hiddenAddition);
            return b;
        }
        public static bool TakeItem(int index, ItemType type, uint count = 1) => singleton.GetPanelFor(type).TakeItem(index, count);
        public static uint ItemCount(ItemName n) => singleton.GetPanelFor(singleton.db[n].type).ItemCount(n);
        public static uint ItemCount(int index, ItemType type) => singleton.GetPanelFor(type).ItemCount(index);
        public static ItemName ItemAt(int index, ItemType type) => singleton.GetPanelFor(type)[index].name;
    }
}