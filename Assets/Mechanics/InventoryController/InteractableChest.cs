using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory.UI;
using System.Threading.Tasks;
using Armere.UI;

namespace Armere.Inventory
{

	public class InteractableChest : SpawnableBody, IInteractable
	{

		public bool canInteract { get; set; } = true;

		public float requiredLookDot => -1;

		public string interactionDescription => "Open";
		public string highlightDescription = "Chest";


		public string interactionName => highlightDescription;

		public Vector3 worldOffset => default;

		public event System.Action onChestOpened;

		public ItemData item;
		public uint count;
		public void Init(ItemData item, uint count)
		{
			this.item = item;
			this.count = count;
		}
		public void SpawnItemsToWorld()
		{
			for (int i = 0; i < count; i++)
			{
				ItemSpawner.SpawnItem((PhysicsItemData)item, transform.position, transform.rotation);
			}
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


	}
}
