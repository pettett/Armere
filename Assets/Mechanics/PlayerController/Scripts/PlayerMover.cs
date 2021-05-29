using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Armere.PlayerController
{
	public class PlayerMover : MonoBehaviour
	{
		public float colliderRadius = 1f;
		public float colliderHeight = 2f;
		public float colliderCenter = 1f;
		[MyBox.PositiveValueOnly] public int sensorArrayCount = 5;

		public Vector3 velocity;
		public Vector3 up = Vector3.up;

		new Rigidbody rigidbody;
		private void Start()
		{
			rigidbody = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			// Perform a single raycast using RaycastCommand and wait for it to complete
			// Setup the command and result buffers
			var results = new NativeArray<RaycastHit>(sensorArrayCount + 1, Allocator.TempJob);

			var commands = new NativeArray<RaycastCommand>(sensorArrayCount + 1, Allocator.TempJob);

			// Set the data of the first command
			Vector3 origin = transform.position + Vector3.up;
			Vector3 direction = Vector3.down;

			commands[0] = new RaycastCommand(origin, direction);


			for (int i = 0; i < sensorArrayCount; i++)
			{
				float theta = 2 * Mathf.PI * i / (float)sensorArrayCount;
				commands[i + 1] = new RaycastCommand(
					origin + colliderRadius * new Vector3(Mathf.Sin(theta), 0, Mathf.Cos(theta)),
					direction
				);
			}


			// Schedule the batch of raycasts
			JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1);

			// Wait for the batch processing job to complete
			handle.Complete();

			// If batchedHit.collider is null there was no hit

			Vector3 avg = Vector3.zero;

			for (int i = 0; i < results.Length; i++)
			{
				var r = results[i];
				if (r.collider == null) break;

				Debug.DrawLine(commands[i].from, r.point, Color.green, Time.deltaTime);
				avg += r.point;
			}

			avg /= results.Length;

			transform.position = avg + up * colliderCenter;

			rigidbody.velocity = velocity;


			// Dispose the buffers
			results.Dispose();
			commands.Dispose();
		}
	}
}
