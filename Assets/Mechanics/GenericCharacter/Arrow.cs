using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider)),
RequireComponent(typeof(Rigidbody))]
public class Arrow : SpawnableBody
{
	public Vector3EventChannelSO onArrowHitEventChannel;

	bool initialized = false;
	AmmoItemData ammo;
	Rigidbody rb;
	Collider col;
	bool hit = false;
	bool destroyOnHit = false;
	public CollisionDetectionMode initializedDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
	public void Initialize(AmmoItemData ammo, Vector3 velocity)
	{
		enabled = true;
		transform.localScale = Vector3.one;
		//Calibrate components
		this.ammo = ammo;


		rb = GetComponent<Rigidbody>();
		rb.velocity = velocity;
		transform.forward = velocity;
		rb.isKinematic = false;
		rb.collisionDetectionMode = initializedDetectionMode;

		destroyOnHit = ammo.flags.HasFlag(AmmoFlags.DropItemOnMiss);

		//Set initialzed
		initialized = true;

		StartCoroutine(EnableCollider());

	}
	IEnumerator EnableCollider()
	{
		yield return null;
		//Enable collider next frame to stop self collision
		col = GetComponent<Collider>();
		col.enabled = true;
	}

	// Update is called once per frame
	void Update()
	{
		if (!initialized)
		{
			Debug.LogError("Arrow not initialized by update", gameObject);
			return;
		}
		transform.forward = rb.velocity;
	}
	static void DrawCross(Vector3 center, float size, float time = 2)
	{
		Debug.DrawLine(center + Vector3.up * size, center - Vector3.up * size, Color.green, time);
		Debug.DrawLine(center + Vector3.right * size, center - Vector3.right * size, Color.green, time);
		Debug.DrawLine(center + Vector3.forward * size, center - Vector3.forward * size, Color.green, time);
	}


	private void OnCollisionEnter(Collision other)
	{
		if (!other.collider.isTrigger && !hit && initialized)
		{
			//Debug.Log($"{ammoName} collided with {other.gameObject.name}", other.gameObject);

			Vector3 collisionPoint = other.contacts[0].point;

			DrawCross(collisionPoint, 1, 10);


			hit = true;
			if (other.gameObject.TryGetComponent<Health>(out var h))
			{
				h.Damage(10, gameObject);
			}

			onArrowHitEventChannel?.RaiseEvent(collisionPoint);

			Destroy();


			if (!destroyOnHit)
			{
				//Turn arrow into an item if it is permitted
				var x = ItemSpawner.SpawnItemAsync(ammo, collisionPoint, transform.rotation);
			}

		}
	}
}