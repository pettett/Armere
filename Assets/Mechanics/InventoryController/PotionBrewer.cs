using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
using Armere.Inventory.UI;
using Yarn.Unity;

namespace Armere.Inventory
{

	public class PotionBrewer : MonoBehaviour, IInteractable
	{
		public bool canInteract { get; set; } = true;
		public float requiredLookDot => 0;
		public string interactionDescription => "Brew Potion";

		public string interactionName => null;

		public Vector3 worldOffset => default;

		public YarnProgram selectionDialogue;

		public InventoryController playerInventory;

		public async void Interact(IInteractor interactor)
		{
			interactor.PauseControl();

			DialogueInstances.singleton.runner.Add(selectionDialogue);

			// Reuse sell menu to select potion ingredient
			var selection = await ItemSelectionMenuUI.singleton.SelectItem(x => x.item.potionIngredient);

			DialogueInstances.singleton.runner.Stop();
			DialogueInstances.singleton.runner.Clear();

			GameCameras.s.lockingMouse = true;
			DialogueInstances.singleton.ui.DialogueComplete();

			if (selection.index != -1)
			{
				ItemData ingredient = playerInventory.ItemAt(selection.index, selection.type).item;


				for (int i = 0; i < playerInventory.potions.stackCount; i++)
				{
					if (playerInventory.potions.items[i].item == ingredient.potionWorksFrom)
					{
						if (playerInventory.TakeItem(ingredient))
						{
							//Take the item and change the contents
							playerInventory.potions.TakeItem(i, 1);

							PotionItemUnique pot = new PotionItemUnique(ingredient.newPotionType, 0, 0);

							pot.potency = ingredient.increasedPotency;
							playerInventory.TryAddItem(pot);

						}


						break;
					}
				}
			}

			interactor.ResumeControl();

		}


	}
}