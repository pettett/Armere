using System.Collections.Generic;

namespace Armere.Inventory
{
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

        public StackPanel(string name, uint limit, ItemType type, params InventoryOptionDelegate[] options) : base(name, limit, type, options)
        {
            //items = new List<ItemStack>(limit > 20 ? 20 : (int)limit);
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
}