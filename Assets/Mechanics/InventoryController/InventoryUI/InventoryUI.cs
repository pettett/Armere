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

    public void CreateTemplate(Transform itemGridPanel, ItemName name, int count)
    {
        var go = Instantiate(template, itemGridPanel);
        go.transform.GetChild(0).GetComponent<Image>().sprite = db[name].sprite;
        go.GetComponentInChildren<TextMeshProUGUI>().text = count.ToString();
        go.GetComponent<InventoryUIItem>().onSelect += () => OnItemSelected(name);
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
        foreach (KeyValuePair<ItemType, Dictionary<ItemName, int>> itemGroup in InventoryController.singleton.items)
        {
            var grid = CreateGridPanelTemplate();
            foreach (KeyValuePair<ItemName, int> item in itemGroup.Value)
                CreateTemplate(grid, item.Key, item.Value);

        }
    }
}
