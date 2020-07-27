using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIItem : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler
{
    public event System.Action onSelect;
    public int itemIndex;
    public ItemType type;
    public InventoryUI inventoryUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryUI.ShowOptionMenu(type, itemIndex, eventData.position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onSelect?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect?.Invoke();
    }


}
