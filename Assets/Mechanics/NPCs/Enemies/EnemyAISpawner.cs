using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyAISpawner : Spawner
{
    public AssetReferenceGameObject ai;
    public AIWaypointGroup optionalWaypoints;
    public EnemyAI.EnemyBehaviour spawnedEnemyBehaviour;

    public UnityEngine.Events.UnityEvent<EnemyAI> onPlayerDetected;
    [System.NonSerialized] public EnemyAI body;
    public override async Task<SpawnableBody> Spawn()
    {
        body = (EnemyAI)await GameObjectSpawner.SpawnAsync(ai, transform.position, transform.rotation);
        body.waypointGroup = optionalWaypoints;
        body.enemyBehaviour = spawnedEnemyBehaviour;
        body.onPlayerDetected += onPlayerDetected.Invoke;

        body.InitEnemy();
        return body;
    }




    private async void Start()
    {
        await Spawn();
    }
}