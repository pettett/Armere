using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
namespace Armere.Inventory.UI
{
    public class ItemInfoDisplay : MonoBehaviour
    {
        public Image thumbnail;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        AsyncOperationHandle<Sprite> spriteAsyncOperation;
        public async void ShowInfo(ItemStackBase stackBase, ItemDatabase db)
        {
            title.text = db[stackBase.name].displayName;
            description.text = db[stackBase.name].description;
            //Load the sprite
            spriteAsyncOperation = Addressables.LoadAssetAsync<Sprite>(db[stackBase.name].displaySprite);
            thumbnail.sprite = await spriteAsyncOperation.Task;
        }

        private void OnDestroy()
        {
            if (spriteAsyncOperation.IsValid())
                Addressables.Release(spriteAsyncOperation);
        }
    }
}