using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NPCSpawn : Spawner
{


    public NPCName spawnedNPCName;

    public NPCTemplate template;
    public AssetReferenceGameObject baseNPCReference;

    public Transform[] conversationGroupTargetsOverride = new Transform[0];


    public Transform[] focusPoints;
    public Transform[] walkingPoints;


    // Start is called before the first frame update
    async void Start()
    {
        var npc = (NPC)await Spawn();
        npc.InitNPC(template, this, conversationGroupTargetsOverride);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        //draw a version of the npc template's mesh
        Gizmos.DrawCube(transform.position + Vector3.up * 0.9f, new Vector3(0.2f, 1.8f, 0.2f));
    }

    public override async Task<SpawnableBody> Spawn()
    {
        return await GameObjectSpawner.SpawnAsync(baseNPCReference, transform.position, transform.rotation);
    }
}
