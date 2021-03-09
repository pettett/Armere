using System.Threading.Tasks;
using Armere.Inventory;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

public class EnemyAISpawner : Spawner
{
	public AssetReferenceGameObject ai;
	public AIWaypointGroup optionalWaypoints;
	public bool autoEngage;
	public MeleeWeaponItemData meleeWeapon;
	public UnityEngine.Events.UnityEvent<AIHumanoid> onPlayerDetected;

	[System.NonSerialized] public EnemyAI body;
	public override async Task<SpawnableBody> Spawn()
	{
		Assert.IsTrue(ai.RuntimeKeyIsValid(), "Reference is null");
		body = (EnemyAI)await GameObjectSpawner.SpawnAsync(ai, transform.position, transform.rotation);
		body.waypointGroup = optionalWaypoints;
		body.onPlayerDetected += onPlayerDetected.Invoke;
		body.meleeWeapon = meleeWeapon;
		body.InitEnemy();
		return body;
	}




	private async void Start()
	{
		await Spawn();
	}
}