using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

namespace Armere.Inventory.UI
{
	public class NewItemPrompt : MonoBehaviour
	{
		public static NewItemPrompt singleton;
		public GameObject holder;
		public ItemDatabase db;
		public Image thumbnail;
		public TextMeshProUGUI title;
		public TextMeshProUGUI description;

		public async void ShowPrompt(ItemData item, uint count, System.Action onPromptRemoved)
		{
			if (count == 0)
			{
				Debug.LogWarning("Giving prompt for 0 items!");
				return;
			}

			holder.SetActive(true);

			if (count == 1)
				title.text = item.name;
			else //Tell the player how many items they are getting
				title.text = string.Format("{0} x{1}", item.name, count);

			description.text = item.description;



			InventoryController.singleton.TryAddItem(item, count, true);

			thumbnail.sprite = await item.thumbnail.LoadAssetAsync().Task;

			await Task.Delay(500);

			var continueAction = new InputAction(binding: "/*/<button>");

			TaskCompletionSource<InputAction.CallbackContext> onButtonPressed = new TaskCompletionSource<InputAction.CallbackContext>();

			continueAction.performed += onButtonPressed.SetResult;
			continueAction.Enable();

			await onButtonPressed.Task;

			continueAction.Disable();
			//Remove the prompt
			Addressables.Release(thumbnail.sprite);

			holder.SetActive(false);
			onPromptRemoved?.Invoke();
		}





		private void Start()
		{
			holder.SetActive(false);
			singleton = this;
		}
	}
}