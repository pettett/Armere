using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct AudioProfile
{
	public Vector3 position;
	public float spacialBlend;
}

[CreateAssetMenu(fileName = "AudioEventChannelSO", menuName = "Game/Audio/AudioEventChannelSO")]
public class AudioEventChannelSO : EventChannelSO<AudioClip, AudioProfile>
{

}