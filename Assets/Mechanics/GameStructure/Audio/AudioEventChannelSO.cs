using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct AudioEventData
{
	public readonly Vector3 position;
	public readonly AudioProfile profile;

	public AudioEventData(Vector3 position, AudioProfile profile)
	{
		this.position = position;
		this.profile = profile;
	}
}

[System.Serializable]
public struct AudioProfile
{
	[Range(0, 1)] public float spacialBlend;
	public float virtualAudioVolume;

	public AudioProfile(float virtualAudioVolume)
	{
		this.spacialBlend = 1;
		this.virtualAudioVolume = virtualAudioVolume;
	}
}

[CreateAssetMenu(fileName = "AudioEventChannelSO", menuName = "Game/Audio/AudioEventChannelSO")]
public class AudioEventChannelSO : EventChannelSO<AudioClip, AudioEventData>
{
	public void RaiseEvent(AudioClipSet set, Vector3 position, AudioProfile profile)
	{
		RaiseEvent(set.SelectClip(), new AudioEventData(position, profile));
	}
	public void RaiseEvent(AudioClipSet set, Vector3 position) => RaiseEvent(set, position, set.profile);

}