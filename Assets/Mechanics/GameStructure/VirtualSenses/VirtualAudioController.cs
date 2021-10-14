using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class VirtualAudioController : MonoBehaviour
{
	public AudioEventChannelSO audioEventChannelSO;

	[ReadOnly] public int listenerCount;

	private void Awake()
	{
	}
	private void OnEnable()
	{
		if (audioEventChannelSO != null)
			audioEventChannelSO.OnEventRaised += OnAudioEvent;
	}
	private void OnDisable()
	{

		if (audioEventChannelSO != null)
			audioEventChannelSO.OnEventRaised -= OnAudioEvent;
	}
	private void Update()
	{
		listenerCount = VirtualAudioListener.listeners.Count;
	}
	public void OnAudioEvent(AudioClip clip, AudioEventData data)
	{
		MakeNoise(data.position, data.profile.virtualAudioVolume);
	}
	public void MakeNoise(Vector3 position, float volume)
	{
		for (int i = 0; i < VirtualAudioListener.listeners.Count; i++)
		{
			float distance = Vector3.Distance(position, VirtualAudioListener.listeners[i].transform.position);
			//For every doubling of distance, the sound level reduces by 6 decibels (dB),
			float relitiveVolume = volume - Mathf.Log(distance, 2) * 6;


			if (relitiveVolume > VirtualAudioListener.listeners[i].noiseThreshold)
			{
				if (VirtualAudioListener.listeners[i].pathfindNoiseVolume)
				{
					//Calculate a new distance with a pathfinding sample
					NavMeshPath path = new NavMeshPath();
					NavMesh.CalculatePath(position, VirtualAudioListener.listeners[i].transform.position, -1, path);
					distance = 0;
					for (int c = 0; c < path.corners.Length - 1; c++)
					{
						distance += Vector3.Distance(path.corners[c], path.corners[c + 1]);
						Debug.DrawLine(path.corners[c], path.corners[c + 1], Color.blue, 10);
					}

					relitiveVolume = volume - Mathf.Log(distance, 2) * 6;
					if (relitiveVolume > VirtualAudioListener.listeners[i].noiseThreshold)
					{

						//The virtual listener can hear this
						VirtualAudioListener.listeners[i].OnHearNoise(position);
					}
				}
				else
				{
					//The virtual listener can hear this without pathfinding - through walls
					VirtualAudioListener.listeners[i].OnHearNoise(position);
				}
			}
		}
	}
}