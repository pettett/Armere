using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CollisionAudio : MonoBehaviour
{
	new Rigidbody rigidbody;
	public float momentumThreshold = 1f;
	public float volumeScaleFromMomentum = 1f;
	public AudioClipSet collisionSounds;
	public AudioEventChannelSO soundEvent;
	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();

		Assert.IsNotNull(rigidbody);
		Assert.IsNotNull(collisionSounds);
		Assert.IsNotNull(soundEvent);
	}
	private void OnCollisionEnter(Collision other)
	{
		float impactMomentum = other.relativeVelocity.magnitude * rigidbody.mass;
		if (impactMomentum > momentumThreshold)
		{
			MakeNoise(other.contacts[0].point, impactMomentum * volumeScaleFromMomentum);

		}
	}

	public void MakeNoise(Vector3 position, float volume)
	{
		soundEvent.RaiseEvent(collisionSounds, position, new AudioProfile(volume));
	}
}
