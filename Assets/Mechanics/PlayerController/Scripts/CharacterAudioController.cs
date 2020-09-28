using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.PlayerController
{
    [RequireComponent(typeof(AudioSource))]

    public class CharacterAudioController : MonoBehaviour
    {
        AudioSource source;
        PlayerController c;
        public AudioClip[] footSteps;
        private void Start()
        {
            source = GetComponent<AudioSource>();
            c = GetComponent<PlayerController>();
        }
        public void FootDown()
        {
            source.PlayOneShot(footSteps[Random.Range(0, footSteps.Length - 1)]);
            if (c.currentWater != null)
            {
                //Make foot splash
                c.currentWater.CreateSplash(c.currentWater.GetSurfacePosition(transform.position), 0.5f);
            }
        }
    }
}