using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{

    [System.Serializable]
    public class ItemStackBase
    {
        public readonly ItemName name;

        public ItemStackBase(ItemName name)
        {
            this.name = name;
        }
    }
    public abstract class InventoryPanel
    {
        public readonly string name;
        public OptionDelegate[] options;
        public int limit;

        public abstract bool AddItem(ItemName name, uint count);
        public abstract bool AddItem(int index, uint count);
        public abstract bool TakeItem(ItemName name, uint count);
        public abstract bool TakeItem(int index, uint count);
        public abstract uint ItemCount(ItemName item);
        public abstract uint ItemCount(int itemIndex);
        public abstract ItemName ItemAt(int index);

        public InventoryPanel(string name, int limit, OptionDelegate[] options)
        {
            this.name = name;
            this.options = options;
            this.limit = limit;
        }
        public abstract ItemStackBase this[int i]
        {
            get;
            set;
        }
        public abstract int stackCount
        {
            get;
        }

    }

    public class StackPanel : InventoryPanel
    {
        [System.Serializable]
        public class ItemStack : ItemStackBase
        {
            public uint count;

            public ItemStack(ItemName n, uint count) : base(n)
            {
                this.count = count;
            }
        }
        public Action onPanelUpdated;

        public List<ItemStack> items;

        public override int stackCount => items.Count;


        public override ItemStackBase this[int i] { get => items[i]; set => items[i] = value as ItemStack; }

        public StackPanel(string name, int limit, params OptionDelegate[] options) : base(name, limit, options)
        {
            items = new List<ItemStack>(limit == int.MaxValue ? 0 : limit);
        }

        public override uint ItemCount(ItemName item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].name == item)
                    return items[i].count;

            }
            return 0u;

        }
        public override uint ItemCount(int itemIndex)
        {
            if (itemIndex < items.Count && itemIndex >= 0)
                return items[itemIndex].count;
            else
                return 0u;
        }



        public override bool AddItem(ItemName item, uint count)
        {

            ItemStack stack = items.Find(s => s.name == item);
            if (stack != null)
            {
                stack.count += count;
            }
            else if (items.Count < limit)
            {
                items.Add(new ItemStack(item, count));
            }
            else
            {
                return false;
            }

            onPanelUpdated?.Invoke();
            return true;
        }
        public override bool AddItem(int index, uint count)
        {
            if (index < items.Count && index >= 0)
            {
                //Never need to add item as the type being increased is not known,
                //so if out of range it can not be specified
                items[index].count += count;
                onPanelUpdated?.Invoke();
                return true;
            }
            else return false;
        }

        public override bool TakeItem(ItemName item, uint count)
        {

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].name == item && items[i].count >= count)
                {
                    items[i].count -= count;
                    if (items[i].count == 0)
                    {
                        items.RemoveAt(i);
                    }

                    onPanelUpdated?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public override bool TakeItem(int index, uint count)
        {
            //index within array and enough items to remove the amount
            if (index < items.Count && index >= 0 && items[index].count >= count)
            {
                items[index].count -= count;
                if (items[index].count == 0)
                    items.RemoveAt(index);
                onPanelUpdated?.Invoke();
                return true;
            }
            else return false;
        }

        public override ItemName ItemAt(int index) => items[index].name;


    }
    public class UniquesPanel : InventoryPanel
    {

        public List<ItemStackBase> items;

        public UniquesPanel(string name, int limit, params OptionDelegate[] options) : base(name, limit, options)
        {
            items = new List<ItemStackBase>(limit == int.MaxValue ? 0 : limit);
        }

        public override ItemStackBase this[int i] { get => items[i]; set => items[i] = value; }

        public override int stackCount => items.Count;

        public override bool AddItem(ItemName name, uint count)
        {
            if (items.Count < limit)
            {
                items.Add(new ItemStackBase(name));
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool AddItem(int index, uint count)
        {
            return false; //cannot add item at index as no stacking
        }

        public override ItemName ItemAt(int index) => items[index].name;


        public override uint ItemCount(ItemName item)
        {
            uint count = 0;
            for (int i = 0; i < stackCount; i++)
            {
                if (items[i].name == item)
                    count++;
            }
            return count;
        }
        public override uint ItemCount(int itemIndex)
        {
            if (itemIndex < items.Count && itemIndex >= 0)
                return 1u;
            else
                return 0u;

        }
        public override bool TakeItem(ItemName name, uint count)
        {
            for (int i = 0; i < stackCount; i++)
            {
                if (items[i].name == name)
                {
                    items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public override bool TakeItem(int index, uint count)
        {
            if (index < items.Count && index >= 0)
            {
                items.RemoveAt(index);
                return true;
            }
            return false;
        }
    }

    public delegate void NewItemDelegate(ItemName item);
    public NewItemDelegate onItemAdded;

    public ItemDatabase db;

    public InventoryPanel GetPanelFor(ItemType t)
    {
        switch (t)
        {
            case ItemType.Common: return common;
            case ItemType.Quest: return quest;
            case ItemType.Weapon: return weapon;
            case ItemType.SideArm: return sideArm;
            case ItemType.Ammo: return ammo;
            case ItemType.Bow: return bow;
            case ItemType.Currency: return currency;
            default: return null;
        }
    }

    public delegate void OptionDelegate(ItemType type, int itemIndex);
    [System.Serializable]
    public class InventorySave
    {
        public List<StackPanel.ItemStack> common;
        public List<ItemStackBase> quest;
        public List<ItemStackBase> weapon;
        public List<ItemStackBase> sideArm;
        public InventorySave(InventoryController c)
        {
            common = c.common.items;
            quest = c.quest.items;
            weapon = c.weapon.items;
            sideArm = c.sideArm.items;
        }
    }

    public InventorySave CreateSave()
    {
        return new InventorySave(this);
    }
    public void RestoreSave(InventorySave inventorySave)
    {
        common.items = inventorySave.common;
        quest.items = inventorySave.quest;
        weapon.items = inventorySave.weapon;
        sideArm.items = inventorySave.sideArm;
    }

    public StackPanel common;
    public UniquesPanel quest;
    public UniquesPanel weapon;
    public UniquesPanel bow;
    public StackPanel ammo;
    public UniquesPanel sideArm;
    public StackPanel currency;



    public static InventoryController singleton;
    public event OptionDelegate OnSelectItemEvent;
    public event OptionDelegate OnDropItemEvent;


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
        common = new StackPanel("Items", int.MaxValue);
        quest = new UniquesPanel("Quest Items", int.MaxValue);
        weapon = new UniquesPanel("Weapons", 10, OnSelectItem, OnDropItem);
        sideArm = new UniquesPanel("Side Arms", 10, OnSelectItem, OnDropItem);

        bow = new UniquesPanel("Bows", 10, OnSelectItem, OnDropItem);
        ammo = new StackPanel("Ammo", int.MaxValue, OnSelectItem, OnDropItem);
        currency = new StackPanel("Currency", 1);

        singleton = this;
    }

    public static bool AddItem(ItemName n, uint count)
    {
        var b = singleton.GetPanelFor(singleton.db[n].type).AddItem(n, count);
        if (b)
            singleton.onItemAdded?.Invoke(n);
        return b;
    }


    public static bool TakeItem(ItemName n, uint count = 1) => singleton.GetPanelFor(singleton.db[n].type).TakeItem(n, count);

    public static bool AddItem(int index, ItemType type, uint count)
    {
        var b = singleton.GetPanelFor(type).AddItem(index, count);
        if (b) //Use itemat command to find the type of item that was added
            singleton.onItemAdded?.Invoke(singleton.GetPanelFor(type).ItemAt(index));
        return b;
    }
    public static bool TakeItem(int index, ItemType type, uint count = 1) => singleton.GetPanelFor(type).TakeItem(index, count);
    public static uint ItemCount(ItemName n) => singleton.GetPanelFor(singleton.db[n].type).ItemCount(n);
    public static uint ItemCount(int index, ItemType type) => singleton.GetPanelFor(type).ItemCount(index);
    public static ItemName ItemAt(int index, ItemType type) => singleton.GetPanelFor(type)[index].name;
}
