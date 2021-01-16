using System.Collections.Generic;

namespace Armere.Inventory
{


	public class ItemStack : ItemStackBase
	{
		public uint count;

		public ItemStack() : base(ItemName.Stick)
		{
		}

		public ItemStack(ItemName n, uint count) : base(n)
		{
			this.count = count;
		}

		public override void Write(GameDataWriter writer)
		{
			writer.Write((int)name);
			writer.Write(count);
		}
	}

	public class StackPanel<StackT> : InventoryPanel where StackT : ItemStack, new()
	{

		public List<StackT> items;

		public override int stackCount => items.Count;

		public override ItemStackBase this[int i] { get => items[i]; set => items[i] = value as StackT; }

		public StackPanel(string name, uint limit, ItemType type, ItemInteractionCommands commands) : base(name, limit, type, commands)
		{
			items = new List<StackT>(limit > 20 ? 20 : (int)limit);
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



		public override int AddItem(ItemName item, uint count)
		{

			int stackIndex = items.FindIndex(s => s.name == item);


			if (stackIndex != -1)
			{
				items[stackIndex].count += count;
			}
			else if (items.Count < limit)
			{
				items.Add((StackT)System.Activator.CreateInstance(typeof(StackT), new object[] { item, count }));
				stackIndex = items.Count - 1;
			}
			else
			{
				return -1;
			}

			OnPanelUpdated();

			return stackIndex;
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

		public override ItemStackBase ItemAt(int index) => items[index];

		public override bool AddItem(ItemStackBase stack)
		{
			items.Add((StackT)stack);
			return true;
		}
	}
}