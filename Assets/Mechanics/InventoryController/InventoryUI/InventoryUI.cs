using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

namespace Armere.Inventory.UI
{
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
        GameObject contextMenu;

        public GameObject contextMenuPrefab;
        public GameObject contextMenuButtonPrefab;

        public UnityEngine.Events.UnityEvent<bool> onContextMenuEnabled;
        public UnityEngine.Events.UnityEvent<bool> onContextMenuDisabled;

        Dictionary<ItemType, SelectableInventoryItemUI[]> inventoryUIPanels = null;

        System.Predicate<ItemStackBase> currentPredicate;

        public SelectableInventoryItemUI CreateTemplate(Transform itemGridPanel, InventoryPanel panel, int index)
        {
            var go = Instantiate(template, itemGridPanel.GetChild(1));

            SelectableInventoryItemUI item = go.GetComponent<SelectableInventoryItemUI>();

            switch (panel[index])
            {
                case ItemStack stack:
                    item.countText.text = stack.count == 1 ? "" : stack.count.ToString();
                    break;
                default:
                    if (item.countText != null)
                        Destroy(item.countText);
                    break;
            }

            item.onSelect += OnItemSelected;
            item.type = panel.type;

            item.inventoryUI = this;
            // if (!sellMenu)
            //     item.optionDelegates = panel.options;
            // else
            //     item.optionDelegates = new InventoryController.OptionDelegate[] { OnSelectItem };

            item.ChangeItemIndex(index);
            //Change selectable based on predicate,
            //Allows some items to be removed for selection
            item.interactable = currentPredicate(panel.ItemAt(index));

            return item;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //If the user has clicked on the background, they do not want to use the context menu
            RemoveContextMenu();
        }

        public void RemoveContextMenu()
        {
            if (contextMenu != null)
            {
                Destroy(contextMenu);
                EnableContextMenu(false);
            }
        }

        IEnumerable<ItemInteractionCommands> CommandsEnabled(InventoryPanel panel)
        {
            foreach (ItemInteractionCommands c in System.Enum.GetValues(typeof(ItemInteractionCommands)))
            {
                if (c != ItemInteractionCommands.None && panel.commands.HasFlag(c)) yield return c;
            }
        }

        public void Callback(ItemInteractionCommands commands, ItemType type, int index)
        {
            switch (commands)
            {
                case ItemInteractionCommands.Drop:
                    InventoryController.singleton.OnDropItem(type, index);
                    break;
                case ItemInteractionCommands.Equip:
                    InventoryController.singleton.OnSelectItem(type, index);
                    break;
                case ItemInteractionCommands.Consume:
                    InventoryController.singleton.OnConsumeItem(type, index);
                    break;
            }
        }

        public void ShowContextMenu(ItemType type, int index, Vector2 mousePosition)
        {
            if (sellMenu)
            {
                OnSelectItem(type, index);
            }
            else
            {
                RemoveContextMenu();




                InventoryPanel p = InventoryController.singleton.GetPanelFor(type);
                ItemInteractionCommands[] commands = CommandsEnabled(p).ToArray();

                if (commands.Length != 0)
                {
                    ItemData item = db[InventoryController.ItemAt(index, type).name];

                    contextMenu = Instantiate(contextMenuPrefab, transform);

                    (contextMenu.transform as RectTransform).pivot = new Vector2(0f, 1f);
                    (contextMenu.transform as RectTransform).position = mousePosition + new Vector2(20, 10);

                    for (int i = 0; i < commands.Length; i++)
                    {
                        //Add the buttons
                        var button = Instantiate(contextMenuButtonPrefab, contextMenu.transform);

                        int callbackIndex = i;
                        var text = button.GetComponentInChildren<TextMeshProUGUI>();
                        if (item.disabledCommands.HasFlag(commands[i]))
                        {
                            //When this button is clicked, do nothing
                            button.GetComponent<Button>().interactable = false;
                            text.color = button.GetComponent<Button>().colors.disabledColor;
                        }
                        else
                        {
                            //When this button is clicked, apply it and close the menu
                            button.GetComponent<Button>().onClick.AddListener(() => Callback(commands[callbackIndex], type, index));
                            button.GetComponent<Button>().onClick.AddListener(RemoveContextMenu);
                        }


                        text.SetText(commands[i].ToString());


                    }

                    EnableContextMenu(true);
                }

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
            onItemSelected?.Invoke(type, itemIndex);
        }


        public Transform CreateGridPanelTemplate(InventoryPanel panel)
        {
            UnityEngine.Assertions.Assert.IsNotNull(panel, "Panel cannot be null");

            var go = Instantiate(gridPanelTemplate, gridPanelHolder);
            go.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = panel.name;
            return go.transform;
        }

        public void OnItemSelected(ItemStackBase item)
        {
            selectedDisplay.ShowInfo(item, db);
        }

        void AddItemGroup(InventoryPanel panel)
        {
            var grid = CreateGridPanelTemplate(panel);
            inventoryUIPanels[panel.type] = new SelectableInventoryItemUI[panel.stackCount];
            //Add all the item readouts
            for (int i = 0; i < panel.stackCount; i++)
            {
                inventoryUIPanels[panel.type][i] = CreateTemplate(grid, panel, i);

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

            panel.onPanelUpdated += UpdatePanel;
        }

        public void UpdatePanel(InventoryPanel panel)
        {

            for (int i = 0; i < panel.stackCount; i++)
            {
                inventoryUIPanels[panel.type][i].ChangeItemIndex(i);
                //Test if selectable from predicate
                inventoryUIPanels[panel.type][i].interactable = currentPredicate(panel.ItemAt(i));
            }
            for (int i = panel.stackCount; i < inventoryUIPanels[panel.type].Length; i++)
            {
                //Clean up the new free square

                if (inventoryUIPanels[panel.type][i] != null)
                {
                    Destroy(inventoryUIPanels[panel.type][i].gameObject);
                }
            }
        }

        public void RemoveItemGroup(InventoryPanel panel)
        {
            panel.onPanelUpdated -= UpdatePanel;
        }

        public void CleanUpInventory()
        {
            for (int i = 0; i < gridPanelHolder.childCount; i++)
            {
                Destroy(gridPanelHolder.GetChild(i).gameObject);
            }
        }
        private void OnDisable()
        {
            if (!sellMenu)
                DisableMenu();
        }

        private void OnEnable()
        {
            if (!sellMenu)
                EnableMenu(_ => true);
        }
        public void DisableMenu()
        {
            inventoryUIPanels = null;

            RemoveItemGroup(InventoryController.singleton.melee);
            RemoveItemGroup(InventoryController.singleton.sideArm);
            RemoveItemGroup(InventoryController.singleton.bow);
            RemoveItemGroup(InventoryController.singleton.ammo);
            RemoveItemGroup(InventoryController.singleton.armour);
            RemoveItemGroup(InventoryController.singleton.common);
            RemoveItemGroup(InventoryController.singleton.quest);
            RemoveItemGroup(InventoryController.singleton.potions);


            CleanUpInventory();


            TooltipUI.current.OnCursorExitItemUI();
        }
        public void EnableMenu(System.Predicate<ItemStackBase> predicate)
        {
            currentPredicate = predicate;

            CleanUpInventory();
            //print("Creating inventory");
            inventoryUIPanels = new Dictionary<ItemType, SelectableInventoryItemUI[]>();
            //Currency if left for the currency display


            AddItemGroup(InventoryController.singleton.melee);
            AddItemGroup(InventoryController.singleton.sideArm);
            AddItemGroup(InventoryController.singleton.bow);
            AddItemGroup(InventoryController.singleton.ammo);
            AddItemGroup(InventoryController.singleton.armour);
            AddItemGroup(InventoryController.singleton.common);
            AddItemGroup(InventoryController.singleton.quest);
            AddItemGroup(InventoryController.singleton.potions);
        }
    }
}