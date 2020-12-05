using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudio : MonoBehaviour
{
    AudioSource source;
    new Rigidbody rigidbody;
    public float momentumThreshold = 1f;
    public float volumeScaleFromMomentum = 0.001f;
    public AudioClipSet collisionSounds;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
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
        //TODO: Scale volume by momentum
        source.PlayOneShot(collisionSounds.SelectClip());


        VirtualAudioController.singleton.MakeNoise(position, volume);
    }
}
