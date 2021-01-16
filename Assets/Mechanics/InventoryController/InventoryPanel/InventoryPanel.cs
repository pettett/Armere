using System;

namespace Armere.Inventory
{

	public abstract class InventoryPanel
	{
		public readonly string name;
		public readonly ItemType type;
		public readonly ItemInteractionCommands commands;

		public uint limit;

		public event Action<InventoryPanel> onPanelUpdated;
		public event Action<InventoryPanel, int> onItemRemoved;

		public abstract int AddItem(ItemName name, uint count);
		public abstract bool AddItem(int index, uint count);
		public abstract bool AddItem(ItemStackBase stack);
		public abstract bool TakeItem(ItemName name, uint count);
		public abstract bool TakeItem(int index, uint count);
		public abstract uint ItemCount(ItemName item);
		public abstract uint ItemCount(int itemIndex);
		public abstract ItemStackBase ItemAt(int index);

		public InventoryPanel(string name, uint limit, ItemType type, ItemInteractionCommands commands)
		{
			this.name = name;
			this.limit = limit;
			this.type = type;
			this.commands = commands;
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
		protected void OnItemRemoved(int index) => onItemRemoved?.Invoke(this, index);
	}
}