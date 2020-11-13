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
        public uint limit;

        public abstract bool AddItem(ItemName name, uint count);
        public abstract bool AddItem(int index, uint count);
        public abstract bool TakeItem(ItemName name, uint count);
        public abstract bool TakeItem(int index, uint count);
        public abstract uint ItemCount(ItemName item);
        public abstract uint ItemCount(int itemIndex);
        public abstract ItemName ItemAt(int index);

        public InventoryPanel(string name, uint limit, params OptionDelegate[] options)
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

        public event Action onPanelUpdated;
        protected void OnPanelUpdated() => onPanelUpdated?.Invoke();
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


        public List<ItemStack> items;

        public override int stackCount => items.Count;


        public override ItemStackBase this[int i] { get => items[i]; set => items[i] = value as ItemStack; }

        public StackPanel(string name, uint limit, params OptionDelegate[] options) : base(name, limit, options)
        {
            items = new List<ItemStack>(limit > 20 ? 20 : (int)limit);
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

            OnPanelUpdated();
            return true;
        }
        public override bool AddItem(int index, uint count)
        {
            if (index < items.Count && index >= 0)
            {
                //Never need to add item as the type being increased is not known,
                //so if out of range it can not be specified
                items[index].count += count;
                OnPanelUpdated();
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

                    OnPanelUpdated();
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
                OnPanelUpdated();
                return true;
            }
            else return false;
        }

        public override ItemName ItemAt(int index) => items[index].name;


    }
    public class UniquesPanel : InventoryPanel
    {

        public List<ItemStackBase> items;

        public UniquesPanel(string name, uint limit, params OptionDelegate[] options) : base(name, limit, options)
        {
            items = new List<ItemStackBase>(limit > 20 ? 20 : (int)limit);
        }

        public override ItemStackBase this[int i] { get => items[i]; set => items[i] = value; }

        public override int stackCount => items.Count;

        public override bool AddItem(ItemName name, uint count)
        {
            if (items.Count < limit)
            {
                items.Add(new ItemStackBase(name));
                OnPanelUpdated();
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
                    OnPanelUpdated();
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

    //Items added to this panel are not recorded, the values of the items are used
    public class ValuePanel : InventoryPanel
    {
        public uint currency;

        public ValuePanel(string name, uint limit, params OptionDelegate[] options) : base(name, limit, options)
        {
            currency = 0;
        }

        public override ItemStackBase this[int i] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int stackCount => 1;
        public override bool AddItem(ItemName name, uint count)
        {
            //Add the value of this item to the stack
            currency += InventoryController.singleton.db[name].sellValue * count;
            OnPanelUpdated();
            return true;
        }
        public override bool AddItem(int index, uint count)
        {
            throw new NotImplementedException();
        }
        public override ItemName ItemAt(int index)
        {
            throw new NotImplementedException();
        }
        public override uint ItemCount(ItemName item)
        {
            if (InventoryController.singleton.db[item].sellable)
                return currency % InventoryController.singleton.db[item].sellValue;
            else
                return 0;
        }
        public override uint ItemCount(int itemIndex)
        {
            return currency;
        }
        public override bool TakeItem(ItemName name, uint count)
        {
            if (currency >= InventoryController.singleton.db[name].sellValue * count)
            {
                currency -= InventoryController.singleton.db[name].sellValue * count;
                OnPanelUpdated();
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool TakeItem(int index, uint count)
        {
            throw new NotImplementedException();
        }
    }



    public delegate void NewItemDelegate(ItemName item, bool hiddenAddition);
    public NewItemDelegate onItemAdded;

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
            weapon = c.melee.items;
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
        melee.items = inventorySave.weapon;
        sideArm.items = inventorySave.sideArm;
    }

    public StackPanel common;
    public UniquesPanel quest;
    public UniquesPanel melee;
    public UniquesPanel bow;
    public StackPanel ammo;
    public UniquesPanel sideArm;
    public ValuePanel currency;



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
        melee = new UniquesPanel("Weapons", 10, OnSelectItem, OnDropItem);
        sideArm = new UniquesPanel("Side Arms", 10, OnSelectItem, OnDropItem);

        bow = new UniquesPanel("Bows", 10, OnSelectItem, OnDropItem);
        ammo = new StackPanel("Ammo", int.MaxValue, OnSelectItem, OnDropItem);
        currency = new ValuePanel("Currency", int.MaxValue);

        singleton = this;
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