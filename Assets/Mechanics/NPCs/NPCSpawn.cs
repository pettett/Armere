using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawn : MonoBehaviour
{
    public static Queue<GameObject> npcPool = new Queue<GameObject>();

    public NPCName spawnedNPCName;

    public NPCTemplate template;
    public GameObject baseNPC;

    public Transform[] conversationGroupTargetsOverride = new Transform[0];

    [System.Serializable] public class PositionDict : RotaryHeart.Lib.SerializableDictionary.SerializableDictionaryBase<string, Transform> { }
    public PositionDict focusPoints;
    public PositionDict walkingPoints;


    // Start is called before the first frame update
    void Start()
    {
        var npc = Spawner.Spawn(ref npcPool, baseNPC, transform.position, transform.rotation).GetComponent<NPC>();
        npc.InitNPC(template, this, conversationGroupTargetsOverride);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        //draw a version of the npc template's mesh
        if (baseNPC != null)
        {
            var m = baseNPC.GetComponentInChildren<SkinnedMeshRenderer>();

            if (m != null)
            {
                Gizmos.DrawMesh(m.sharedMesh, transform.position, transform.rotation, m.transform.lossyScale);
            }
        }
    }
}
