using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class InventoryUIItem : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    public event System.Action onSelect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onSelect?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect?.Invoke();
    }


}
