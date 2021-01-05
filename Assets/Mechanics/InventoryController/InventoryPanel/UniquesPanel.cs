
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Inventory
{
    [System.Serializable]
    public class UniquesPanel<UniqueT> : InventoryPanel where UniqueT : ItemStackBase
    {

        public List<UniqueT> items;

        public UniquesPanel(string name, uint limit, ItemType type, ItemInteractionCommands commands) : base(name, limit, type, commands)
        {
            //items = new List<ItemStackBase>(limit > 20 ? 20 : (int)limit);
        }

        public override ItemStackBase this[int i] { get => items[i]; set => items[i] = (UniqueT)value; }

        public override int stackCount => items.Count;

        public bool AddItem(ItemName name)
        {
            if (items.Count < limit)
            {
                items.Add((UniqueT)System.Activator.CreateInstance(typeof(UniqueT), name));
                OnPanelUpdated();
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool AddItem(ItemStackBase item)
        {
            if (items.Count < limit)
            {
                items.Add((UniqueT)item);
                OnPanelUpdated();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int AddItem(ItemName name, uint count)
        {
            //Add every item until no more can be
            for (int i = 0; i < count; i++)
            {
                if (!AddItem(name)) return -1;
            }

            return items.Count - 1;
        }

        public override bool AddItem(int index, uint count)
        {
            return false; //cannot add item at index as no stacking
        }

        public override ItemStackBase ItemAt(int index) => items[index];



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
                    OnItemRemoved(i);
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
                OnItemRemoved(index);
                OnPanelUpdated();
                return true;
            }
            return false;
        }
    }
}