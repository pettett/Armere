using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class CharacterAudioController : MonoBehaviour
{
    AudioSource source;
    public AudioClip[] footSteps;
    private void Start()
    {
        source = GetComponent<AudioSource>();
    }
    public void FootDown()
    {
        source.PlayOneShot(footSteps[Random.Range(0, footSteps.Length - 1)]);
    }
}
