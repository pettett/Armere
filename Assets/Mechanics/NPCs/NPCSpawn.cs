using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawn : MonoBehaviour
{


    public NPCName spawnedNPCName;

    public NPCTemplate template;
    public GameObject baseNPC;

    public Transform[] conversationGroupTargetsOverride = new Transform[0];


    public Transform[] focusPoints;
    public Transform[] walkingPoints;


    // Start is called before the first frame update
    void Start()
    {
        var npc = Instantiate(baseNPC, transform.position, transform.rotation).GetComponent<NPC>();
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
