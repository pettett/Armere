using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassPusher : MonoBehaviour
{

	public float radius;
	public Vector3 offset;
	public Vector4 Data
	{
		get
		{
			Vector3 pos = transform.TransformPoint(offset);
			return new Vector4(pos.x, pos.y, pos.z, radius);
		}
	}

	public bool setMainPusher = false;
	float Max(Vector3 values) => Mathf.Max(Mathf.Max(values.x, values.y), values.z);
	private void Reset()
	{
		if (TryGetComponent<MeshFilter>(out var m))
		{
			offset = m.sharedMesh.bounds.center;
			radius = Max(m.sharedMesh.bounds.extents);
		}
	}
	private void OnEnable()
	{
		GrassController.pushers.Add(this);

	}
	private void OnDisable()
	{
		GrassController.pushers.Remove(this);

	}
	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.TransformPoint(offset), radius);
	}
}
