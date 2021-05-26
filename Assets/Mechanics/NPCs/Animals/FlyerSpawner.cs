using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyerSpawner : MonoBehaviour
{
	public FlyerTemplate template;
	public Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);
	public float spawnsPerSecond = 1;
	public uint maxSpawns = 10;
	uint spawns = 0;
	IEnumerator Start()
	{
		var wait = new WaitForSeconds(spawnsPerSecond);
		while (spawns < maxSpawns)
		{
			yield return wait;
			Vector3 pos = localBounds.center + Vector3.Scale(localBounds.size, (new Vector3(Random.value, Random.value, Random.value) * 2 - Vector3.one));
			pos = transform.TransformPoint(pos);
			Instantiate(template.prefab, pos, Quaternion.identity).GetComponent<Flyer>().Init(template, transform);
			spawns++;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(localBounds.center, localBounds.size);
	}
}
