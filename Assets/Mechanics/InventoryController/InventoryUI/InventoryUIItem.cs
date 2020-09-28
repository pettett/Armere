using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class InventoryUIItem : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler
{
    public event System.Action onSelect;
    public int itemIndex;
    public ItemType type;
    public InventoryUI inventoryUI;
    public Image thumbnail;
    public TMPro.TextMeshProUGUI countText;
    public TMPro.TextMeshProUGUI infoText;
    AsyncOperationHandle<Sprite> asyncOperation;
    public async void SetupItem(ItemData item)
    {
        type = item.type;
        asyncOperation = item.displaySprite.LoadAssetAsync();
        thumbnail.sprite = await asyncOperation.Task;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryUI.ShowContextMenu(type, itemIndex, eventData.position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onSelect?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect?.Invoke();
    }

    private void OnDestroy()
    {
        if (asyncOperation.IsValid())
            Addressables.Release(asyncOperation);
    }

}
