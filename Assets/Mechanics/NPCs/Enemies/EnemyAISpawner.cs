using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

public class EnemyAISpawner : Spawner
{
    public AssetReferenceGameObject ai;
    public AIWaypointGroup optionalWaypoints;
    public bool autoEngage;
    public EnemyRoutine idleRoutine;
    public ItemName meleeWeapon = ItemName.IronSword;
    public UnityEngine.Events.UnityEvent<EnemyAI> onPlayerDetected;

    [System.NonSerialized] public EnemyAI body;
    public override async Task<SpawnableBody> Spawn()
    {
        Assert.IsTrue(ai.RuntimeKeyIsValid(), "Reference is null");
        body = (EnemyAI)await GameObjectSpawner.SpawnAsync(ai, transform.position, transform.rotation);
        body.waypointGroup = optionalWaypoints;
        body.autoEngage = autoEngage;
        body.onPlayerDetected += onPlayerDetected.Invoke;
        body.meleeWeapon = meleeWeapon;
        body.idleRoutine = idleRoutine;
        body.InitEnemy();
        return body;
    }




    private async void Start()
    {
        await Spawn();
    }
}