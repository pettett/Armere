using System;

namespace Armere.Inventory
{

	//Items added to this panel are not recorded, the values of the items are used
	public class ValuePanel : InventoryPanel
	{
		public uint currency;

		public ValuePanel(string name, uint limit, ItemType type) : base(name, limit, type, ItemInteractionCommands.None)
		{
			currency = 0;
		}

		public override ItemStackBase this[int i] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override int stackCount => 1;
		public override int AddItem(ItemData item, uint count, int desiredPosition = -1)
		{
			//Add the value of this item to the stack
			currency += item.sellValue * count;
			OnPanelUpdated();

			return 0;
		}

		public override bool AddItem(int index, uint count)
		{
			throw new NotImplementedException();
		}
		public override ItemStackBase ItemAt(int index)
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
			return TakeValue(InventoryController.singleton.db[name].sellValue * count);
		}
		public bool TakeValue(uint value)
		{
			if (currency >= value)
			{
				currency -= value;
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

		public override bool AddItem(ItemStackBase stack)
		{
			throw new NotImplementedException();
		}
	}
}