using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class InventoryUI : MonoBehaviour
{
    public int rowCount = 4;
    public GameObject gridPanelTemplate;
    public GameObject template;
    public GameObject blankSlotTemplate;

    public ItemDatabase db;
    public Transform gridPanelHolder;
    public ItemInfoDisplay selectedDisplay;
    public bool sellMenu;
    public System.Action<ItemType, int> onItemSelected;

    public void CreateTemplate(Transform itemGridPanel, InventoryController.InventoryPanel panel, int index)
    {
        var go = Instantiate(template, itemGridPanel.GetChild(1));
        go.transform.GetChild(0).GetComponent<Image>().sprite = db[panel[index].name].sprite;

        switch (panel[index])
        {
            case InventoryController.StackPanel.ItemStack stack:
                go.GetComponentInChildren<TextMeshProUGUI>().text = stack.count == 1 ? "" : stack.count.ToString();
                break;
            default:
                Destroy(go.GetComponentInChildren<TextMeshProUGUI>());
                break;
        }

        InventoryUIItem item = go.GetComponent<InventoryUIItem>();

        item.onSelect += () => OnItemSelected(panel[index].name);

        if (!sellMenu)
            item.optionDelegates = panel.options;
        else
            item.optionDelegates = new InventoryController.OptionDelegate[] { OnSelectItem };

        item.itemIndex = index;
        item.type = db[panel[index].name].type;
    }
    public void CreateBlankSlotTemplate(Transform itemGridPanel)
    {
        var go = Instantiate(blankSlotTemplate, itemGridPanel.GetChild(1));
    }


    public void OnSelectItem(ItemType type, int itemIndex)
    {
        print("Selected " + itemIndex.ToString());
        onItemSelected?.Invoke(type, itemIndex);
    }


    public Transform CreateGridPanelTemplate(InventoryController.InventoryPanel panel)
    {
        var go = Instantiate(gridPanelTemplate, gridPanelHolder);
        go.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = panel.name;
        return go.transform;
    }

    public void OnItemSelected(ItemName item)
    {
        selectedDisplay.ShowInfo(item, db);
    }

    private void OnEnable()
    {
        for (int i = 0; i < gridPanelHolder.childCount; i++)
        {
            Destroy(gridPanelHolder.GetChild(i).gameObject);
        }
        foreach (System.Tuple<ItemType, InventoryController.InventoryPanel> itemGroup in InventoryController.singleton.ItemPanels())
        {
            var grid = CreateGridPanelTemplate(itemGroup.Item2);
            //Add all the item readouts
            for (int i = 0; i < itemGroup.Item2.itemCount; i++)
            {
                CreateTemplate(grid, itemGroup.Item2, i);
            }
            int blankCount;
            switch (itemGroup.Item2)
            {
                case InventoryController.StackPanel stack:
                    blankCount = 4 - (stack.itemCount % rowCount);
                    break;
                case InventoryController.UniquesPanel uniques:

                    if (uniques.maxItems == int.MaxValue)
                        blankCount = 4 - (uniques.itemCount % rowCount);
                    else
                        blankCount = uniques.maxItems - uniques.itemCount;
                    break;
                default:
                    blankCount = 0;
                    break;
            }

            for (int i = 0; i < blankCount; i++)
            {
                CreateBlankSlotTemplate(grid);
            }

        }
    }



}
