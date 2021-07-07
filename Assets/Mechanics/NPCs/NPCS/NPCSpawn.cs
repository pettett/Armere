using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

public class NPCSpawn : Spawner
{
	public NPCTemplate template;
	public AssetReferenceGameObject baseNPCReference;

	public Transform[] conversationGroupTargetsOverride = new Transform[0];


	public Transform[] focusPoints;
	public Transform[] walkingPoints;


	// Start is called before the first frame update
	void Start()
	{
		Spawn();
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

	public void OnNPCLoaded(AsyncOperationHandle<GameObject> handle)
	{
		var npc = handle.Result.GetComponent<AIHumanoid>();
		handle.Result.GetComponent<AIMachine>().defaultState = template;
		npc.Spawn(this);
	}

	public void Spawn()
	{
		Assert.IsTrue(baseNPCReference.RuntimeKeyIsValid(), "Reference to npc base is null");
		var handle = GameObjectSpawner.Spawn(baseNPCReference, transform.position, transform.rotation);
		GameObjectSpawner.OnDone(handle, OnNPCLoaded);
	}
}
