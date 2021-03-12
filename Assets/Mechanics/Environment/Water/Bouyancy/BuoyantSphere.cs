using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BuoyantSphere : BuoyantBody
{
	new SphereCollider collider;


	float drag;


	public static float SphereVolume(float r)
	{
		return (4f / 3f) * Mathf.PI * r * r * r;
	}

	public void Start()
	{
		collider = GetComponent<SphereCollider>();
		rb = GetComponent<Rigidbody>();

		drag = rb.drag;
		rb.mass = SphereVolume(collider.radius) * density;
	}

	public void FixedUpdate()
	{
		if (volume != null)
		{

			float hDiff = Mathf.Clamp(volume.bounds.max.y - transform.position.y + collider.radius, 0, collider.radius * 2);
			float v = (Mathf.PI * hDiff * hDiff / 3f) * (3f * collider.radius - hDiff);

			Vector3 volumeWeight = Physics.gravity * volume.density * v;

			rb.drag = Mathf.Lerp(drag, waterDrag, hDiff / (collider.radius * 2));
			rb.AddForce(-volumeWeight);

		}
		else
		{
			rb.drag = 0;
		}
	}


}
