using UnityEngine;
namespace Armere.Inventory
{

	public class PassiveInteractableItem : ItemSpawnable, IPassiveInteractable
	{
		public bool canInteract { get; set; } = true;

		public void Interact(IInteractor interactor)
		{
			if (interactor.transform.GetComponent<GameObjectInventory>() is GameObjectInventory i)
				AddItemsToInventory(() => Destroy(), i.inventory);
		}
	}
}