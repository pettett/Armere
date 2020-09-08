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
    public ScrollRect scroll;
    GameObject contextMenu;

    [System.Serializable]
    public class BoolEvent : UnityEngine.Events.UnityEvent<bool> { }

    public BoolEvent onContextMenuEnabled;
    public BoolEvent onContextMenuDisabled;
    public async void CreateTemplate(Transform itemGridPanel, InventoryController.InventoryPanel panel, int index)
    {
        var go = Instantiate(template, itemGridPanel.GetChild(1));
        go.transform.GetChild(0).GetComponent<Image>().sprite = await db[panel[index].name].displaySprite.LoadAssetAsync().Task;

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

        item.inventoryUI = this;
        // if (!sellMenu)
        //     item.optionDelegates = panel.options;
        // else
        //     item.optionDelegates = new InventoryController.OptionDelegate[] { OnSelectItem };

        item.itemIndex = index;
        item.type = db[panel[index].name].type;
    }

    public void ShowOptionMenu(ItemType type, int index, Vector2 mousePosition)
    {
        if (sellMenu)
        {
            OnSelectItem(type, index);
        }
        else
        {
            if (contextMenu != null) Destroy(contextMenu);
            contextMenu = new GameObject("Menu", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            contextMenu.GetComponent<LayoutElement>().ignoreLayout = true;

            contextMenu.transform.SetParent(gridPanelHolder);
            (contextMenu.transform as RectTransform).pivot = new Vector2(0f, 1f);
            (contextMenu.transform as RectTransform).position = mousePosition + new Vector2(-10, 10);

            EnableContextMenu(true);
        }
    }
    void EnableContextMenu(bool enabled)
    {
        onContextMenuEnabled.Invoke(enabled);
        onContextMenuDisabled.Invoke(!enabled);
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

    void AddItemGroup(InventoryController.InventoryPanel panel)
    {
        var grid = CreateGridPanelTemplate(panel);
        //Add all the item readouts
        for (int i = 0; i < panel.stackCount; i++)
        {
            CreateTemplate(grid, panel, i);
        }
        int blankCount;
        if (panel.limit == int.MaxValue)
            blankCount = 4 - (panel.stackCount % rowCount);
        else
            blankCount = panel.limit - panel.stackCount;

        for (int i = 0; i < blankCount; i++)
        {
            CreateBlankSlotTemplate(grid);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < gridPanelHolder.childCount; i++)
        {
            Destroy(gridPanelHolder.GetChild(i).gameObject);
        }
        //Currency if left for the currency display
        AddItemGroup(InventoryController.singleton.common);
        AddItemGroup(InventoryController.singleton.weapon);
        AddItemGroup(InventoryController.singleton.sideArm);
        AddItemGroup(InventoryController.singleton.bow);
        AddItemGroup(InventoryController.singleton.ammo);
        AddItemGroup(InventoryController.singleton.quest);
    }



}
