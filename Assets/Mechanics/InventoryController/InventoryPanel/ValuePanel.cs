using System;

namespace Armere.Inventory
{

    //Items added to this panel are not recorded, the values of the items are used
    public class ValuePanel : InventoryPanel
    {
        public uint currency;

        public ValuePanel(string name, uint limit, ItemType type, params InventoryOptionDelegate[] options) : base(name, limit, type, options)
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
}