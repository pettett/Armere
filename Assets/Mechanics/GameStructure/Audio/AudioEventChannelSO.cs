using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct AudioProfile
{
	[System.NonSerialized] public Vector3 position;
	[Range(0, 1)] public float spacialBlend;
	public float virtualAudioVolume;
}

[CreateAssetMenu(fileName = "AudioEventChannelSO", menuName = "Game/Audio/AudioEventChannelSO")]
public class AudioEventChannelSO : EventChannelSO<AudioClip, AudioProfile>
{
	public void RaiseEvent(AudioClipSet set, Vector3 position)
	{
		var p = set.profile;
		p.position = position;
		RaiseEvent(set.SelectClip(), p);
	}
}