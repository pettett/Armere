using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Armere.Inventory
{


	public class ItemStack : ItemStackBase, IGameDataSavableAsync<ItemStack>
	{
		public uint count;
		public ItemStack()
		{
		}

		public ItemStack(ItemData n) : base(n)
		{
			this.count = 1;
		}

		public ItemStack(ItemData n, uint count) : base(n)
		{
			this.count = count;
		}


		public void Read(in GameDataReader reader, System.Action<ItemStack> onDone)
		{
			count = reader.ReadUInt();

			reader.ReadAsync<ItemDataAsyncSerializer>(item =>
			{
				onDone?.Invoke(new ItemStack(item, count));
			});
		}

		public override string ToString()
		{
			return $"{item.displayName} x {count}";
		}

		public override void Write(in GameDataWriter writer)
		{
			writer.WritePrimitive(count);
			base.Write(in writer);
		}

		ItemStack IGameDataSerializable<ItemStack>.Init()
		{
			return this;
		}
	}

	public class StackPanel<StackT> : InventoryPanel, IGameDataSavableAsync<StackPanel<StackT>>
			where StackT : ItemStack, IGameDataSavableAsync<StackT>, new()
	{

		public readonly List<StackT> items;

		public override int stackCount => items.Count;

		public override ItemStackBase this[int i] { get => items[i]; set => items[i] = value as StackT; }

		public StackPanel(string name, uint limit, ItemType type, ItemInteractionCommands commands) : base(name, limit, type, commands)
		{
			items = new List<StackT>(limit > 20 ? 20 : (int)limit);
		}

		public override uint ItemCount(ItemData item)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].item == item)
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



		public override int AddItem(ItemData item, uint count, int desiredPosition = -1)
		{

			int stackIndex = items.FindIndex(s => s.item == item);


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

		public override bool TakeItem(ItemData item, uint count)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].item == item && items[i].count >= count)
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

		public void Read(in GameDataReader reader, Action<StackPanel<StackT>> onDone)
		{
			var t = this;
			reader.ReadAsyncInto<BinaryListAsyncSerializer<StackT>>(items, data =>
			{
				onDone?.Invoke(t);
			});
		}

		public void Write(in GameDataWriter writer)
		{
			writer.Write<BinaryListAsyncSerializer<StackT>>(items);
		}

		public StackPanel<StackT> Init()
		{
			return this;
		}
	}
}