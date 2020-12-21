using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

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


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        SkinnedMeshRenderer r = baseNPCReference.editorAsset.GetComponentInChildren<SkinnedMeshRenderer>();
        Mesh m = r.sharedMesh;
        var mats = r.sharedMaterials;

        for (int i = 0; i < m.subMeshCount; i++)
        {
            //Graphics.DrawMesh(m, transform.position, transform.rotation, mats[i], 0);
            Gizmos.color = mats[i].color;
            Gizmos.DrawMesh(m, i, transform.position, transform.rotation, r.transform.lossyScale);
        }

    }
#endif

    public override async Task<SpawnableBody> Spawn()
    {
        Assert.IsTrue(baseNPCReference.RuntimeKeyIsValid(), "Reference to npc base is null");
        return await GameObjectSpawner.SpawnAsync(baseNPCReference, transform.position, transform.rotation);
    }
}
