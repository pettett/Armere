using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory.UI;
using System.Threading.Tasks;

namespace Armere.Inventory
{

	public class InteractableChest : SpawnableBody, IInteractable
	{

		public bool canInteract { get; set; } = true;

		public float requiredLookDot => -1;

		public string interactionDescription => "Open Chest";
		public string highlightDescription = "Chest";

		public Vector3 offset => throw new System.NotImplementedException();
		public event System.Action onChestOpened;

		public ItemData item;
		public uint count;
		public void Init(ItemData item, uint count)
		{
			this.item = item;
			this.count = count;
		}
		public async void SpawnItemsToWorld()
		{
			Task<ItemSpawnable>[] t = new Task<ItemSpawnable>[count];

			for (int i = 0; i < count; i++)
			{
				t[i] = ItemSpawner.SpawnItemAsync((PhysicsItemData)item, transform.position, transform.rotation);
			}

			await Task.WhenAll(t);
		}
		public void Interact(IInteractor c)
		{

			Time.timeScale = 0;

			NewItemPrompt.singleton.ShowPrompt(item, count, () => { Time.timeScale = 1; });

			onChestOpened?.Invoke();

			//Do not allow multiple chest opens
			canInteract = false;
			gameObject.gameObject.transform.GetChild(0).localEulerAngles = new Vector3(0, 40, 0);

			//Do not drop items on destroy
			count = 0;
		}

		public void OnStartHighlight()
		{
			//Show an arrow for this item with name on ui
			UIController.singleton.itemIndicator.StartIndication(transform, highlightDescription);
		}

		public void OnEndHighlight()
		{
			//remove arrow
			UIController.singleton.itemIndicator.EndIndication();
		}
	}
}
