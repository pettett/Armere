
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Inventory
{

	public class UniquesPanel<UniqueT> : InventoryPanel where UniqueT : ItemStackBase
	{

		public List<UniqueT> items;

		public UniquesPanel(string name, uint limit, ItemType type, ItemInteractionCommands commands)
			: base(name, limit, type, commands)
		{
			items = new List<UniqueT>(limit > 20 ? 20 : (int)limit);
		}

		public override ItemStackBase this[int i] { get => items[i]; set => items[i] = (UniqueT)value; }

		public override int stackCount => items.Count;

		public bool AddSingleItem(ItemData item, int desiredPosition, out int addedIndex)
		{
			if (items.Count < limit)
			{
				UniqueT data = (UniqueT)System.Activator.CreateInstance(typeof(UniqueT), new object[] { item });
				Debug.Log($"Inserting {desiredPosition}");
				if (desiredPosition < 0 || desiredPosition >= items.Count)
				{
					addedIndex = items.Count;
					items.Add(data);
				}
				else
				{
					addedIndex = desiredPosition;
					items.Insert(desiredPosition, data);
				}

				OnPanelUpdated();
				return true;
			}
			else
			{
				addedIndex = -1;
				return false;
			}
		}
		public override bool AddItem(ItemStackBase item)
		{
			if (items.Count < limit)
			{

				items.Add((UniqueT)item);


				OnPanelUpdated();
				return true;
			}
			else
			{
				return false;
			}
		}

		public override int AddItem(ItemData item, uint count, int desiredPosition = -1)
		{
			int addedTo = -1;
			//Add every item until no more can be
			for (int i = 0; i < count; i++)
			{
				if (!AddSingleItem(item, desiredPosition, out addedTo)) return -1;
			}

			return addedTo;
		}

		public override bool AddItem(int index, uint count)
		{
			return false; //cannot add item at index as no stacking
		}

		public override ItemStackBase ItemAt(int index) => items[index];



		public override uint ItemCount(ItemData item)
		{
			uint count = 0;
			for (int i = 0; i < stackCount; i++)
			{
				if (items[i].item == item)
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
		public override bool TakeItem(ItemData item, uint count)
		{
			for (int i = 0; i < stackCount; i++)
			{
				if (items[i].item == item)
				{
					items.RemoveAt(i);
					OnPanelUpdated();
					OnItemRemoved(i);
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
				OnItemRemoved(index);
				OnPanelUpdated();
				return true;
			}
			return false;
		}
	}
}