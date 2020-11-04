using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour, IPointerClickHandler
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
    public void CreateTemplate(Transform itemGridPanel, InventoryController.InventoryPanel panel, int index)
    {
        var go = Instantiate(template, itemGridPanel.GetChild(1));

        SelectableInventoryItemUI item = go.GetComponent<SelectableInventoryItemUI>();

        switch (panel[index])
        {
            case InventoryController.StackPanel.ItemStack stack:
                item.countText.text = stack.count == 1 ? "" : stack.count.ToString();
                break;
            default:
                if (item.countText != null)
                    Destroy(item.countText);
                break;
        }

        item.onSelect += () => OnItemSelected(panel[index].name);
        item.type = db[panel[index].name].type;
        item.inventoryUI = this;
        // if (!sellMenu)
        //     item.optionDelegates = panel.options;
        // else
        //     item.optionDelegates = new InventoryController.OptionDelegate[] { OnSelectItem };

        item.ChangeItemIndex(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //If the user has clicked on the background, they do not want to use the context menu
        if (contextMenu != null) RemoveContextMenu();
    }

    public void RemoveContextMenu()
    {
        Destroy(contextMenu);
        EnableContextMenu(false);
    }

    public void ShowContextMenu(ItemType type, int index, Vector2 mousePosition)
    {
        if (sellMenu)
        {
            OnSelectItem(type, index);
        }
        else
        {
            if (contextMenu != null) RemoveContextMenu();

            contextMenu = new GameObject("Menu", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
            contextMenu.GetComponent<LayoutElement>().ignoreLayout = true;

            contextMenu.transform.SetParent(gridPanelHolder);
            (contextMenu.transform as RectTransform).pivot = new Vector2(0f, 1f);
            (contextMenu.transform as RectTransform).position = mousePosition + new Vector2(-10, 10);

            ItemData item = db[InventoryController.ItemAt(index, type)];
            InventoryController.InventoryPanel p = InventoryController.singleton.GetPanelFor(type);
            for (int i = 0; i < p.options.Length; i++)
            {
                //Add the buttons
                var button = new GameObject(p.options[i].Method.Name, typeof(Image), typeof(Button));
                button.transform.SetParent(contextMenu.transform);
                int callbackIndex = i;
                //When this button is clicked, apply it and close the menu
                button.GetComponent<Button>().onClick.AddListener(() => p.options[callbackIndex](type, index));
                button.GetComponent<Button>().onClick.AddListener(RemoveContextMenu);

                var textObject = new GameObject("Text", typeof(TextMeshProUGUI));
                textObject.transform.SetParent(button.transform);

                //Make the text occupy the entire button (Very annoying)
                (textObject.transform as RectTransform).anchorMin = Vector2.zero;
                (textObject.transform as RectTransform).anchorMax = Vector2.one;
                (textObject.transform as RectTransform).anchoredPosition = Vector2.zero;
                (textObject.transform as RectTransform).sizeDelta = Vector2.zero;
                textObject.GetComponent<TextMeshProUGUI>().text = p.options[i].Method.Name;
                textObject.GetComponent<TextMeshProUGUI>().fontSize = 12;
                textObject.GetComponent<TextMeshProUGUI>().color = Color.black;


            }

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
        //Blank slots should not interrupt raycasts
        go.GetComponent<Graphic>().raycastTarget = false;
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
            blankCount = (int)panel.limit - panel.stackCount;

        for (int i = 0; i < blankCount; i++)
        {
            CreateBlankSlotTemplate(grid);
        }
    }

    public void CleanUpInventory()
    {
        for (int i = 0; i < gridPanelHolder.childCount; i++)
        {
            Destroy(gridPanelHolder.GetChild(i).gameObject);
        }
    }

    private void OnEnable()
    {
        CleanUpInventory();
        //Currency if left for the currency display
        AddItemGroup(InventoryController.singleton.common);
        AddItemGroup(InventoryController.singleton.melee);
        AddItemGroup(InventoryController.singleton.sideArm);
        AddItemGroup(InventoryController.singleton.bow);
        AddItemGroup(InventoryController.singleton.ammo);
        AddItemGroup(InventoryController.singleton.quest);
    }

}
