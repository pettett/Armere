using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.Assertions;
namespace Armere.Inventory
{

	public class ChestSpawner : Spawner
	{
		public AssetReferenceT<ItemData> itemRef;
		public AssetReferenceGameObject chest;
		public uint containerCount = 1;
		public UnityEngine.Events.UnityEvent onChestOpened;

		public void Spawn()
		{
			Assert.IsTrue(chest.RuntimeKeyIsValid(), "Reference is null");

			if (itemRef.RuntimeKeyIsValid())
				ItemDatabase.LoadItemDataAsync(itemRef, item =>
				{
					GameObjectSpawner.OnDone(GameObjectSpawner.Spawn(chest, transform.position, transform.rotation), handle =>
					 {
						 var spawnable = handle.Result.GetComponent<InteractableChest>();
						 spawnable.Init(item, containerCount);
						 spawnable.onChestOpened += OnChestOpened;
					 });
				});
			else
				Debug.LogWarning("Attemping to spawn invalid item");



		}

		public void OnChestOpened()
		{
			onChestOpened.Invoke();
		}
		private void Start()
		{
			Spawn();
		}
	}
}