using System;

namespace Armere.Inventory
{

    public abstract class InventoryPanel
    {
        public readonly string name;
        public readonly ItemType type;
        public InventoryOptionDelegate[] options;
        public uint limit;

        public event Action<InventoryPanel> onPanelUpdated;

        public abstract bool AddItem(ItemName name, uint count);
        public abstract bool AddItem(int index, uint count);
        public abstract bool TakeItem(ItemName name, uint count);
        public abstract bool TakeItem(int index, uint count);
        public abstract uint ItemCount(ItemName item);
        public abstract uint ItemCount(int itemIndex);
        public abstract ItemName ItemAt(int index);

        public InventoryPanel(string name, uint limit, ItemType type, params InventoryOptionDelegate[] options)
        {
            this.name = name;
            this.options = options;
            this.limit = limit;
            this.type = type;
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


        protected void OnPanelUpdated() => onPanelUpdated?.Invoke(this);
    }
}