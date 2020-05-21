using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawn : MonoBehaviour
{
    public NPCName[] spawnedNPCNames;
    public Transform[] spawnPoints;
    public NPCTemplate template;
    public GameObject baseNPC;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < spawnedNPCNames.Length; i++)
        {
            Vector3 pos = i >= spawnPoints.Length ? transform.position : spawnPoints[i].position;
            Quaternion rot = i >= spawnPoints.Length ? transform.rotation : spawnPoints[i].rotation;
            var npc = Instantiate(baseNPC, pos, rot).GetComponent<NPC>();
            npc.InitNPC(template, spawnedNPCNames[i]);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        //draw a version of the npc template's mesh
        var m = baseNPC.GetComponentInChildren<SkinnedMeshRenderer>();

        if (spawnPoints.Length == 0)
            Gizmos.DrawMesh(m.sharedMesh, transform.position, transform.rotation, m.transform.lossyScale);
        else for (int i = 0; i < spawnPoints.Length; i++)
            {
                Gizmos.DrawMesh(m.sharedMesh, spawnPoints[i].position, spawnPoints[i].rotation, m.transform.lossyScale);
            }
    }

}
