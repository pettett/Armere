using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
using Armere.Inventory.UI;
using Yarn.Unity;

public class PotionBrewer : MonoBehaviour, IInteractable
{
    public bool canInteract { get; set; } = true;
    public float requiredLookDot => 0;
    public string interactionDescription => "Brew Potion";


    public YarnProgram selectionDialogue;

    public async void Interact(IInteractor interactor)
    {
        interactor.PauseControl();

        DialogueRunner.singleton.Add(selectionDialogue);

        // Reuse sell menu to select potion ingredient
        var selection = await ItemSelectionMenuUI.singleton.SelectItem(x => InventoryController.singleton.db[x.name].potionIngredient);

        DialogueRunner.singleton.Clear();
        DialogueRunner.singleton.ClearStringTable();

        DialogueInstances.singleton.dialogueUI.FinishDialogue();


        ItemName ingredient = InventoryController.ItemAt(selection.index, selection.type).name;


        for (int i = 0; i < InventoryController.singleton.potions.stackCount; i++)
        {
            if (InventoryController.singleton.potions.items[i].name == (ItemName)InventoryController.singleton.db[ingredient].potionWorksFrom)
            {
                if (InventoryController.TakeItem(ingredient))
                {
                    //Take the item and change the contents
                    InventoryController.singleton.potions.TakeItem(i, 1);

                    PotionItemUnique pot = new PotionItemUnique((ItemName)InventoryController.singleton.db[ingredient].newPotionType);

                    pot.potency = InventoryController.singleton.db[ingredient].increasedPotency;
                    InventoryController.singleton.potions.AddItem(pot);

                }


                break;
            }
        }

        interactor.ResumeControl();

    }

    public void OnEndHighlight()
    {

    }

    public void OnStartHighlight()
    {

    }
}
