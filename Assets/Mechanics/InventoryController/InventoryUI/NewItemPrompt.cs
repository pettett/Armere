using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Armere.UI;

namespace Armere.Inventory.UI
{
	public class NewItemPrompt : MonoBehaviour
	{
		public static NewItemPrompt singleton;
		public GameObject holder;
		public Image thumbnail;
		public TextMeshProUGUI title;
		public TextMeshProUGUI description;

		public async void ShowPrompt(ItemData item, uint count, System.Action onPromptRemoved, bool addItemsToInventory = true)
		{
			if (count == 0)
			{
				Debug.LogWarning("Giving prompt for 0 items!");
				return;
			}

			holder.SetActive(true);
			holder.transform.localPosition = Vector2.down * (((RectTransform)UIController.singleton.transform).sizeDelta.y + 200);
			LeanTween.moveLocalY(holder, 0, 1).setIgnoreTimeScale(true).setEaseOutCubic();

			if (count == 1)
				title.text = item.name;
			else //Tell the player how many items they are getting
				title.text = string.Format("{0} x{1}", item.name, count);

			description.text = item.description;


			if (addItemsToInventory)
				InventoryController.singleton.TryAddItem(item, count, true);

			thumbnail.sprite = await item.thumbnail.LoadAssetAsync().Task;

			await Task.Delay(500);

			var continueAction = new InputAction(binding: "/*/<button>");

			TaskCompletionSource<InputAction.CallbackContext> onButtonPressed = new TaskCompletionSource<InputAction.CallbackContext>();

			continueAction.performed += onButtonPressed.SetResult;
			continueAction.Enable();

			await onButtonPressed.Task;

			LeanTween.moveLocalY(holder, (((RectTransform)UIController.singleton.transform).sizeDelta.y + 200), 1
				).setIgnoreTimeScale(true).setEaseInCubic().setOnComplete(() => holder.SetActive(false));

			continueAction.Disable();
			//Remove the prompt
			Addressables.Release(thumbnail.sprite);

			onPromptRemoved?.Invoke();
		}





		private void Start()
		{
			holder.SetActive(false);
			singleton = this;
		}
	}
}