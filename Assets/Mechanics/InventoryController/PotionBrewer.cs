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

		public YarnProgram selectionDialogue;

		public InventoryController playerInventory;

		public async void Interact(IInteractor interactor)
		{
			interactor.PauseControl();

			DialogueRunner.singleton.Add(selectionDialogue);

			// Reuse sell menu to select potion ingredient
			var selection = await ItemSelectionMenuUI.singleton.SelectItem(x => x.item.potionIngredient);

			DialogueRunner.singleton.Stop();
			DialogueRunner.singleton.Clear();
			DialogueRunner.singleton.ClearStringTable();
			GameCameras.s.lockingMouse = true;
			DialogueInstances.singleton.dialogueUI.FinishDialogue();

			if (selection.index != -1)
			{
				ItemData ingredient = playerInventory.ItemAt(selection.index, selection.type).item;


				for (int i = 0; i < playerInventory.potions.stackCount; i++)
				{
					if (playerInventory.potions.items[i].item.itemName == (ItemName)ingredient.potionWorksFrom)
					{
						if (playerInventory.TakeItem(ingredient.itemName))
						{
							//Take the item and change the contents
							playerInventory.potions.TakeItem(i, 1);

							PotionItemUnique pot = new PotionItemUnique(playerInventory.db[(ItemName)ingredient.newPotionType]);

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