using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class InventoryUIItem : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler
{
    public event System.Action onSelect;
    public int itemIndex;
    public ItemType type;
    public InventoryController.OptionDelegate[] optionDelegates;
    public void OnPointerClick(PointerEventData eventData)
    {
        print("Clicked on this item");
        if (optionDelegates.Length > 0)
        {
            optionDelegates[0](type, itemIndex);
        }
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
