using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;

public class PotionBrewer : MonoBehaviour, IInteractable
{
    public bool canInteract { get; set; } = true;
    public float requiredLookDot => 0;
    public string interactionDescription => "Brew Potion";
    [System.Serializable]
    public struct PotionCreationTemplate
    {
        public ItemName ingredient;
    }

    public PotionCreationTemplate template;

    public void Interact(IInteractor interactor)
    {
        //TODO: Reuse sell menu to select potion ingredient
        for (int i = 0; i < InventoryController.singleton.potions.stackCount; i++)
        {
            if (InventoryController.singleton.potions.items[i].name == (ItemName)InventoryController.singleton.db[template.ingredient].potionWorksFrom)
            {
                if (InventoryController.TakeItem(template.ingredient))
                {
                    //Take the item and change the contents

                    InventoryController.singleton.potions.items[i].name = (ItemName)InventoryController.singleton.db[template.ingredient].newPotionType;
                    InventoryController.singleton.onItemAdded?.Invoke(InventoryController.singleton.potions.items[i], ItemType.Potion, i, false);
                }


                break;
            }
        }

    }

    public void OnEndHighlight()
    {

    }

    public void OnStartHighlight()
    {

    }
}
