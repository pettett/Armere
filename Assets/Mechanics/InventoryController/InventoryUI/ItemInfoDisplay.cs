using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections;

namespace Armere.Inventory.UI
{
	public class ItemInfoDisplay : MonoBehaviour
	{
		public Image thumbnail;
		public TextMeshProUGUI title;
		public TextMeshProUGUI description;
		AsyncOperationHandle<Sprite> spriteAsyncOperation;
		public void ShowInfo(ItemStackBase stackBase)
		{
			title.text = stackBase.item.displayName;
			description.text = stackBase.item.description;
			StartCoroutine(LoadSprite(stackBase));
		}
		IEnumerator LoadSprite(ItemStackBase stackBase)
		{
			//Load the sprite
			spriteAsyncOperation = Addressables.LoadAssetAsync<Sprite>(stackBase.item.thumbnail);
			if (!spriteAsyncOperation.IsDone)
				yield return spriteAsyncOperation;
			thumbnail.sprite = spriteAsyncOperation.Result;
		}

		private void OnDestroy()
		{
			if (spriteAsyncOperation.IsValid())
				Addressables.Release(spriteAsyncOperation);
		}
	}
}