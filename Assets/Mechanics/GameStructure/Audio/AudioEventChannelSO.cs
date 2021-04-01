using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct AudioProfile
{
	[System.NonSerialized] public Vector3 position;
	[Range(0, 1)] public float spacialBlend;
}

[CreateAssetMenu(fileName = "AudioEventChannelSO", menuName = "Game/Audio/AudioEventChannelSO")]
public class AudioEventChannelSO : EventChannelSO<AudioClip, AudioProfile>
{

}