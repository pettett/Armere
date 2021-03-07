using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.Inventory.UI
{

	public class ReplaceItemSelectorUI : MonoBehaviour
	{
		public InventoryItemUI incomingItemDispalyUI;
		public ScrollingSelectorUI scrollingSelector;
		public InventoryController replacingFor;

		public InputReader input;


		private void Start()
		{
			replacingFor.onTriggerReplaceItemDialogue += StartReplaceDialogue;
		}
		private void OnDestroy()
		{
			replacingFor.onTriggerReplaceItemDialogue -= StartReplaceDialogue;
		}

		public void StartReplaceDialogue(ItemData item, System.Action onItemReplaced)
		{
			scrollingSelector.layers[0].selecting = item.type;
			scrollingSelector.gameObject.SetActive(true);


			incomingItemDispalyUI.SetupItemDisplayAsync(item, 0); //0 Count removes count display, as it is not required
																  //Wait for submit


			void OnSumbit(InputActionPhase phase)
			{
				if (phase == InputActionPhase.Performed)
				{
					int selection = scrollingSelector.layers[0].selection;
					replacingFor.ReplaceItem(selection, item);

					scrollingSelector.gameObject.SetActive(false);
					input.uiSubmitEvent -= OnSumbit;
					input.uiCancelEvent -= OnCancel;


					onItemReplaced();
				}
			};
			void OnCancel(InputActionPhase phase)
			{
				if (phase == InputActionPhase.Performed)
				{
					//Just go back to gameplay
					scrollingSelector.gameObject.SetActive(false);
					input.uiSubmitEvent -= OnSumbit;
					input.uiCancelEvent -= OnCancel;

				}
			};
			//Scrolling selector auto enables ui input
			input.uiSubmitEvent += OnSumbit;
			input.uiCancelEvent += OnCancel;


		}
	}
}
