using System;

namespace Armere.Inventory
{
	class CurrencyStack : ItemStackBase
	{
		public readonly uint currency;

		public CurrencyStack(uint currency)
		{
			this.currency = currency;
		}

		public override string ToString()
		{
			return $"Currency: {currency}";
		}
	}
	//Items added to this panel are not recorded, the values of the items are used
	public class ValuePanel : InventoryPanel, IBinaryVariableSerializer<ValuePanel>
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
		public override ItemStackBase ItemAt(int index) => new CurrencyStack(currency);
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

		public ValuePanel Read(in GameDataReader reader)
		{
			currency = reader.ReadUInt();
			return this;
		}

		public void Write(in GameDataWriter writer)
		{
			writer.WritePrimitive(currency);
		}
	}
}