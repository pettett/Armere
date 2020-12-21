
using System.Collections.Generic;

namespace Armere.Inventory
{
    public class UniquesPanel : InventoryPanel
    {

        public List<ItemStackBase> items;

        public UniquesPanel(string name, uint limit, ItemType type, params InventoryOptionDelegate[] options) : base(name, limit, type, options)
        {
            //items = new List<ItemStackBase>(limit > 20 ? 20 : (int)limit);
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
                OnPanelUpdated();
                return true;
            }
            return false;
        }
    }
}