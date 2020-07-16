using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{


    [System.Serializable]
    public class ItemStackBase
    {
        public ItemName name;
    }
    public abstract class InventoryPanel
    {
        public string name;
        public OptionDelegate[] options;
        public abstract bool AddItem(ItemName name, uint count);
        public abstract bool AddItem(int index, uint count);
        public abstract bool TakeItem(ItemName name, uint count);
        public abstract bool TakeItem(int index, uint count);
        public abstract uint ItemCount(ItemName item);
        public abstract ItemName ItemAt(int index);

        public InventoryPanel(string name, OptionDelegate[] options)
        {
            this.name = name;
            this.options = options;
        }
        public abstract ItemStackBase this[int i]
        {
            get;
            set;
        }
        public abstract int itemCount
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
        }

        public List<ItemStack> items;

        public override int itemCount => items.Count;

        public override ItemStackBase this[int i] { get => items[i]; set => items[i].name = value.name; }

        public StackPanel(string name, OptionDelegate[] options) : base(name, options)
        {
            items = new List<ItemStack>();
        }

        public override uint ItemCount(ItemName item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].name == item)
                    return items[i].count;

            }
            return 0;
        }
        public override bool AddItem(ItemName item, uint count)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].name == item)
                {
                    items[i].count += count;
                    return true;
                }
            }

            items.Add(new ItemStack() { name = item, count = count });

            return true; //Always space to add new items
        }
        public override bool AddItem(int index, uint count)
        {
            if (index < items.Count && index >= 0)
            {
                items[index].count += count;
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
                return true;
            }
            else return false;
        }

        public override ItemName ItemAt(int index) => items[index].name;


    }
    public class UniquesPanel : InventoryPanel
    {
        public int maxItems;
        public List<ItemStackBase> items;

        public UniquesPanel(int maxItems, string name, OptionDelegate[] options) : base(name, options)
        {
            this.maxItems = maxItems;

            items = new List<ItemStackBase>(maxItems == int.MaxValue ? 0 : maxItems);
        }

        public override ItemStackBase this[int i] { get => items[i]; set => items[i] = value; }

        public override int itemCount => items.Count;

        public override bool AddItem(ItemName name, uint count)
        {
            if (items.Count < maxItems)
            {
                items.Add(new ItemStackBase() { name = name });
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
            for (int i = 0; i < itemCount; i++)
            {
                if (items[i].name == item)
                    count++;
            }
            return count;
        }

        public override bool TakeItem(ItemName name, uint count)
        {
            for (int i = 0; i < itemCount; i++)
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

    public InventoryPanel items(ItemType t)
    {
        switch (t)
        {
            case ItemType.Common:
                return common;
            case ItemType.Quest:
                return quest;
            case ItemType.Weapon:
                return weapon;
            case ItemType.SideArm:
                return sideArm;
            default:
                return null;
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
    public UniquesPanel sideArm;

    public IEnumerable<Tuple<ItemType, InventoryPanel>> ItemPanels()
    {
        return new Tuple<ItemType, InventoryPanel>[]{
            new Tuple<ItemType, InventoryPanel>(ItemType.Common,common),
            new Tuple<ItemType, InventoryPanel>(ItemType.Weapon,weapon),
            new Tuple<ItemType, InventoryPanel>(ItemType.SideArm,sideArm),
            new Tuple<ItemType, InventoryPanel>(ItemType.Quest,quest)
        };
    }

    public static InventoryController singleton;
    public event OptionDelegate OnSelectItemEvent;

    public void OnSelectItem(ItemType type, int itemIndex)
    {
        print("Selected " + itemIndex.ToString());
        OnSelectItemEvent?.Invoke(type, itemIndex);
    }
    public void OnDropItem(ItemType type, int itemIndex)
    {
        print("Dropped " + itemIndex.ToString());
    }

    private void Awake()
    {

        common = new StackPanel("Items", new OptionDelegate[0]);
        quest = new UniquesPanel(int.MaxValue, "Quest Items", new OptionDelegate[0]);
        weapon = new UniquesPanel(10, "Weapons", new OptionDelegate[] { OnSelectItem, OnDropItem });
        sideArm = new UniquesPanel(10, "Side Arms", new OptionDelegate[] { OnSelectItem, OnDropItem });

        singleton = this;
    }

    public static bool AddItem(ItemName n, uint count)
    {
        var b = singleton.items(singleton.db[n].type).AddItem(n, count);
        if (b)
            singleton.onItemAdded?.Invoke(n);
        return b;
    }


    public static bool TakeItem(ItemName n, uint count = 1) => singleton.items(singleton.db[n].type).TakeItem(n, count);

    public static bool AddItem(int index, ItemType type, uint count)
    {
        var b = singleton.items(type).AddItem(index, count);
        if (b) //Use itemat command to find the type of item that was added
            singleton.onItemAdded?.Invoke(singleton.items(type).ItemAt(index));
        return b;
    }
    public static bool TakeItem(int index, ItemType type, uint count = 1) => singleton.items(type).TakeItem(index, count);
    public static uint ItemCount(ItemName n) => singleton.items(singleton.db[n].type).ItemCount(n);
}
