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
			title.text = stackBase.item.displayName;
			description.text = stackBase.item.description;
			//Load the sprite
			spriteAsyncOperation = Addressables.LoadAssetAsync<Sprite>(stackBase.item.displaySprite);
			thumbnail.sprite = await spriteAsyncOperation.Task;
		}

		private void OnDestroy()
		{
			if (spriteAsyncOperation.IsValid())
				Addressables.Release(spriteAsyncOperation);
		}
	}
}