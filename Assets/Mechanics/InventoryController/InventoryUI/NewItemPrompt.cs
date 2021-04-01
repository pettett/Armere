using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Armere.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Armere.Inventory.UI
{
	public class NewItemPrompt : MonoBehaviour
	{
		public static NewItemPrompt singleton;
		public GameObject holder;
		public Image thumbnail;
		public TextMeshProUGUI title;
		public TextMeshProUGUI description;

		public InputReader reader;
		public float entryTime = 1;
		public float readingTime = 0.1f;

		public void ShowPrompt(ItemData item, uint count, System.Action onPromptRemoved, bool addItemsToInventory = true)
		{
			StartCoroutine(RunPromptRoutine(item, count, onPromptRemoved, addItemsToInventory));
		}
		IEnumerator RunPromptRoutine(ItemData item, uint count, System.Action onPromptRemoved, bool addItemsToInventory)
		{
			if (count == 0)
			{
				Debug.LogWarning("Giving prompt for 0 items!");
				yield break;
			}
			Debug.Log("Opeing new item prompt");

			holder.SetActive(true);
			holder.transform.localPosition = Vector2.down * (((RectTransform)UIController.singleton.transform).sizeDelta.y + 200);
			LeanTween.moveLocalY(holder, 0, entryTime).setIgnoreTimeScale(true).setEaseOutCubic();

			Time.timeScale = 0;

			//reader.DisableAllInput();

			if (count == 1)
				title.text = item.name;
			else //Tell the player how many items they are getting
				title.text = string.Format("{0} x{1}", item.name, count);

			description.text = item.description;


			if (addItemsToInventory)
				InventoryController.singleton.TryAddItem(item, count, true);

			AsyncOperationHandle<Sprite> handle = item.thumbnail.LoadAssetAsync();

			Spawner.OnDone(
				handle,
				x => thumbnail.sprite = x.Result
			);


			yield return new WaitForSecondsRealtime(entryTime + readingTime);

			var continueAction = new InputAction(binding: "/*/<button>");

			TaskCompletionSource<InputAction.CallbackContext> onButtonPressed = new TaskCompletionSource<InputAction.CallbackContext>();
			bool pressed = false;
			continueAction.performed += (x) => pressed = true;
			continueAction.Enable();

			while (!pressed)
				yield return null;


			LeanTween.moveLocalY(holder, (((RectTransform)UIController.singleton.transform).sizeDelta.y + 200), 0.1f
				).setIgnoreTimeScale(true).setEaseInCubic().setOnComplete(() => holder.SetActive(false));


			Time.timeScale = 1;

			//reader.SwitchToGameplayInput();
			continueAction.Disable();
			continueAction.Dispose();
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