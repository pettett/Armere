using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
public class ItemInfoDisplay : MonoBehaviour
{
    public Image thumbnail;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    AsyncOperationHandle<Sprite> spriteAsyncOperation;
    public async void ShowInfo(ItemName item, ItemDatabase db)
    {
        title.text = db[item].displayName;
        description.text = db[item].description;
        //Load the sprite
        spriteAsyncOperation = Addressables.LoadAssetAsync<Sprite>(db[item].displaySprite);
        thumbnail.sprite = await spriteAsyncOperation.Task;
    }

    private void OnDestroy()
    {
        if (spriteAsyncOperation.IsValid())
            Addressables.Release(spriteAsyncOperation);
    }
}