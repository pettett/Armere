using System;

namespace Armere.Inventory
{

	//Items added to this panel are not recorded, the values of the items are used
	public class ValuePanel : InventoryPanel
	{
		public uint currency;
		public readonly ItemDatabase db;

		public ValuePanel(string name, uint limit, ItemType type, ItemDatabase db) : base(name, limit, type, ItemInteractionCommands.None)
		{
			currency = 0;
			this.db = db;
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
		public override uint ItemCount(ItemData data)
		{
			if (data.sellable)
				return currency % data.sellValue;
			else
				return 0;
		}
		public override uint ItemCount(int itemIndex)
		{
			return currency;
		}
		public override bool TakeItem(ItemData data, uint count)
		{
			return TakeValue(data.sellValue * count);
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