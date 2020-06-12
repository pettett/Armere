using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{

    public abstract class InventoryPanel : IEnumerable<System.Tuple<ItemName, int>>
    {
        public OptionDelegate[] options;
        public abstract bool AddItem(ItemName name);
        public abstract bool TakeItem(ItemName name, int count);
        public abstract int ItemCount(ItemName item);
        public abstract IEnumerator<Tuple<ItemName, int>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public InventoryPanel(OptionDelegate[] options)
        {
            this.options = options;
        }
    }

    public class StackPanel : InventoryPanel
    {
        public class ItemStack
        {
            public ItemName name;
            public int count;
        }
        Dictionary<ItemName, int> items;
        public List<ItemStack> iss;

        public StackPanel(OptionDelegate[] options) : base(options)
        {
            items = new Dictionary<ItemName, int>();
        }

        public override int ItemCount(ItemName item)
        {
            if (!items.ContainsKey(item))
                return 0;
            else
                return items[item];
        }
        public override bool AddItem(ItemName item)
        {
            if (!items.ContainsKey(item))
                items[item] = 1;
            else
                items[item]++;
            return true; //Always space to add new items
        }
        public override bool TakeItem(ItemName item, int count)
        {
            if (items.ContainsKey(item) && items[item] >= count)
            {
                items[item] -= count;
                if (items[item] == 0)
                {
                    items.Remove(item);
                }
                return true;
            }
            else
            {
                //Unable to remove that many items
                return false;
            }
        }

        public override IEnumerator<Tuple<ItemName, int>> GetEnumerator()
        {
            foreach (KeyValuePair<ItemName, int> k in items)
            {
                yield return new Tuple<ItemName, int>(k.Key, k.Value);
            }
        }
    }
    public class UniquesPanel : InventoryPanel
    {
        public int maxItems;
        public List<ItemName> items;

        public UniquesPanel(int maxItems, OptionDelegate[] options) : base(options)
        {
            this.maxItems = maxItems;

            items = new List<ItemName>(maxItems == int.MaxValue ? 0 : maxItems);
        }

        public override bool AddItem(ItemName name)
        {
            if (items.Count < maxItems)
            {
                items.Add(name);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override IEnumerator<Tuple<ItemName, int>> GetEnumerator()
        {
            foreach (ItemName k in items)
            {
                yield return new Tuple<ItemName, int>(k, 1);
            }
        }

        public override int ItemCount(ItemName item)
        {
            return items.Contains(item) ? 1 : 0;
        }

        public override bool TakeItem(ItemName name, int count)
        {
            return items.Remove(name);
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

        common = new StackPanel(new OptionDelegate[0]);
        quest = new UniquesPanel(int.MaxValue, new OptionDelegate[0]);
        weapon = new UniquesPanel(10, new OptionDelegate[] { OnSelectItem, OnDropItem });
        sideArm = new UniquesPanel(10, new OptionDelegate[] { OnSelectItem, OnDropItem });

        singleton = this;
    }

    public static bool AddItem(ItemName n) => singleton.items(singleton.db[n].type).AddItem(n);
    public static bool TakeItem(ItemName n, int count = 1) => singleton.items(singleton.db[n].type).TakeItem(n, count);
    public static int ItemCount(ItemName n) => singleton.items(singleton.db[n].type).ItemCount(n);
}
