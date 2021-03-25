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
		public ItemData item;
		public AssetReferenceGameObject chest;
		public uint containerCount = 1;
		public UnityEngine.Events.UnityEvent onChestOpened;

		void OnChestLoaded(AsyncOperationHandle<GameObject> handle)
		{
			var spawnable = handle.Result.GetComponent<InteractableChest>();
			spawnable.Init(item, containerCount);
			spawnable.onChestOpened += OnChestOpened;
		}
		public void Spawn()
		{
			Assert.IsTrue(chest.RuntimeKeyIsValid(), "Reference is null");
			GameObjectSpawner.OnDone(GameObjectSpawner.Spawn(chest, transform.position, transform.rotation), OnChestLoaded);
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