using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
using Armere.UI;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{

	public class InteractableItem : ItemSpawnable, IInteractable
	{

		public bool canInteract { get; set; } = true;

		public float requiredLookDot => -1;

		public string interactionDescription => "Pickup";

		public string interactionName => item.displayName;

		public Vector3 worldOffset => default;


		public void Interact(IInteractor c) =>
			AddItemsToInventory(
				() =>
				{
					Destroy();
				}, c.transform.GetComponent<GameObjectInventory>().inventory);




#if UNITY_EDITOR //Utility methods for dealing with item spawner systems

		[MyBox.ButtonMethod]
		void ConvertToSpawner()
		{
			var spawner = new GameObject($"{gameObject.name} Spawner", typeof(ItemSpawner));
			spawner.transform.position = transform.position;
			spawner.transform.rotation = transform.rotation;

			spawner.transform.SetParent(transform.parent);

			spawner.GetComponent<ItemSpawner>().item = (AssetReferenceT<PhysicsItemData>)(AssetReference)((PhysicsItemData)item).selfReference;
			//spawner.GetComponent<ItemSpawner>().count = count;


		}

#endif

	}
}

