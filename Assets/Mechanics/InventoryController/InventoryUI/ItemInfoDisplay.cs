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
    AsyncOperationHandle<Sprite> sprite;
    public async void ShowInfo(ItemName item, ItemDatabase db)
    {
        title.text = db[item].name;
        description.text = db[item].description;
        //Load the sprite
        sprite = db[item].displaySprite.LoadAssetAsync();
        thumbnail.sprite = await sprite.Task;
    }

    private void OnDestroy()
    {
        Addressables.Release(sprite);
    }
}