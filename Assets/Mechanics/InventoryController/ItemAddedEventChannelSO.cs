using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Armere.Inventory
{

	[CreateAssetMenu(fileName = "Item Added Event", menuName = "Channels/Inventory/Item Added Event", order = 0)]
	public class ItemAddedEventChannelSO : ScriptableObject
	{
		public event UnityAction<ItemStackBase, ItemType, int, bool> onItemAddedEvent;

		public void OnItemAdded(ItemStackBase item, ItemType type, int index, bool hiddenAddition)
		{
			onItemAddedEvent?.Invoke(item, type, index, hiddenAddition);
		}
	}
}
