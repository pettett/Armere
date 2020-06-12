using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class InventoryUI : MonoBehaviour
{
    public GameObject gridPanelTemplate;
    public GameObject template;
    public ItemDatabase db;
    public Transform gridPanelHolder;
    public Image selectedSprite;
    public TextMeshProUGUI selectedTitle;
    public TextMeshProUGUI selectedDescription;

    public void CreateTemplate(Transform itemGridPanel, ItemName name, int count, InventoryController.OptionDelegate[] optionDelegates, int index)
    {
        var go = Instantiate(template, itemGridPanel);
        go.transform.GetChild(0).GetComponent<Image>().sprite = db[name].sprite;

        go.GetComponentInChildren<TextMeshProUGUI>().text = count == 1 ? "" : count.ToString();

        go.GetComponent<InventoryUIItem>().onSelect += () => OnItemSelected(name);
        go.GetComponent<InventoryUIItem>().optionDelegates = optionDelegates;
        go.GetComponent<InventoryUIItem>().itemIndex = index;
        go.GetComponent<InventoryUIItem>().type = db[name].type;
    }
    public Transform CreateGridPanelTemplate()
    {
        var go = Instantiate(gridPanelTemplate, gridPanelHolder);
        return go.transform;
    }

    public void OnItemSelected(ItemName item)
    {
        selectedTitle.text = db[item].name;
        selectedSprite.sprite = db[item].sprite;
        selectedDescription.text = db[item].description;
    }

    private void OnEnable()
    {
        for (int i = 0; i < gridPanelHolder.childCount; i++)
        {
            Destroy(gridPanelHolder.GetChild(i).gameObject);
        }
        foreach (System.Tuple<ItemType, InventoryController.InventoryPanel> itemGroup in InventoryController.singleton.ItemPanels())
        {
            var grid = CreateGridPanelTemplate();
            int i = 0;
            foreach (System.Tuple<ItemName, int> item in itemGroup.Item2)
            {
                CreateTemplate(grid, item.Item1, item.Item2, itemGroup.Item2.options, i);
                i++;
            }

        }
    }
}
