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
		public ItemName item;
		public AssetReferenceGameObject chest;
		public uint containerCount = 1;
		public UnityEngine.Events.UnityEvent onChestOpened;

		public override async Task<SpawnableBody> Spawn()
		{
			Assert.IsTrue(chest.RuntimeKeyIsValid(), "Reference is null");
			SpawnableBody spawnable = await GameObjectSpawner.SpawnAsync(chest, transform.position, transform.rotation);
			((ItemSpawnable)spawnable).Init(item, containerCount);
			((InteractableChest)spawnable).onChestOpened += OnChestOpened;
			return spawnable;
		}

		public void OnChestOpened()
		{
			onChestOpened.Invoke();
		}
		private async void Start()
		{
			await Spawn();
		}
	}
}