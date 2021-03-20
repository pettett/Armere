using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Armere.UI;

namespace Armere.Inventory.UI
{
	public class SelectableInventoryItemUI : InventoryItemUI, IPointerClickHandler
	{
		public event System.Action<ItemStackBase> onSelect;
		[HideInInspector] public InventoryUI inventoryUI;


		public void OnPointerClick(PointerEventData eventData)
		{
			if (interactable)
			{
				inventoryUI.ShowContextMenu(type, itemIndex, eventData.position);
				TooltipUI.current.EndCursorTooltip();
			}
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (interactable)
			{
				ItemStackBase stack = InventoryController.singleton.ItemAt(itemIndex, type);
				onSelect?.Invoke(stack);
				TooltipUI.current.BeginCursorTooltip(stack.title, stack.description);

			}

			inventoryUI.RemoveContextMenu();

			base.OnPointerEnter(eventData);
		}
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (interactable)
				TooltipUI.current.EndCursorTooltip();


			base.OnPointerExit(eventData);
		}

		public override void OnSelect(BaseEventData eventData)
		{
			if (interactable)
				onSelect?.Invoke(InventoryController.singleton.ItemAt(itemIndex, type));
			base.OnSelect(eventData);
		}
	}
}
