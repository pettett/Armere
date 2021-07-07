using System.Threading.Tasks;
using Armere.Inventory;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyAISpawner : Spawner
{
	public AssetReferenceGameObject ai;
	public AIWaypointGroup optionalWaypoints;
	public bool autoEngage;
	public UnityEngine.Events.UnityEvent<AIHumanoid> onPlayerDetected;

	[System.NonSerialized] public EnemyAI body;

	public AIStateTemplate startingState;

	void OnEnemySpawned(AsyncOperationHandle<GameObject> handle)
	{
		var body = handle.Result.GetComponent<EnemyAI>();

		body.waypointGroup = optionalWaypoints;
		body.onPlayerDetected += onPlayerDetected.Invoke;
		if (startingState != null)
			body.machine.defaultState = startingState;
		body.InitEnemy();
	}

	public void Spawn()
	{
		Assert.IsTrue(ai.RuntimeKeyIsValid(), "Reference is null");
		var handle = GameObjectSpawner.Spawn(ai, transform.position, transform.rotation);
		GameObjectSpawner.OnDone(handle, OnEnemySpawned);
	}




	private void Start()
	{
		Spawn();
	}
}