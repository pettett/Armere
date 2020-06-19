using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public class ItemStackBase
    {
        public ItemName name;
    }
    public abstract class InventoryPanel
    {
        public string name;
        public OptionDelegate[] options;
        public abstract bool AddItem(ItemName name, int count);
        public abstract bool TakeItem(ItemName name, int count);
        public abstract int ItemCount(ItemName item);

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
        public class ItemStack : ItemStackBase
        {
            public int count;
        }

        public List<ItemStack> items;

        public override int itemCount => items.Count;

        public override ItemStackBase this[int i] { get => items[i]; set => items[i].name = value.name; }

        public StackPanel(string name, OptionDelegate[] options) : base(name, options)
        {
            items = new List<ItemStack>();
        }

        public override int ItemCount(ItemName item)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].name == item)
                    return items[i].count;

            }
            return 0;
        }
        public override bool AddItem(ItemName item, int count)
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
        public override bool TakeItem(ItemName item, int count)
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

        public override bool AddItem(ItemName name, int count)
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


        public override int ItemCount(ItemName item)
        {
            int count = 0;
            for (int i = 0; i < itemCount; i++)
            {
                if (items[i].name == item)
                    count++;
            }
            return count;
        }

        public override bool TakeItem(ItemName name, int count)
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
    }


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

    public static bool AddItem(ItemName n, int count) => singleton.items(singleton.db[n].type).AddItem(n, count);
    public static bool TakeItem(ItemName n, int count = 1) => singleton.items(singleton.db[n].type).TakeItem(n, count);
    public static int ItemCount(ItemName n) => singleton.items(singleton.db[n].type).ItemCount(n);
}
