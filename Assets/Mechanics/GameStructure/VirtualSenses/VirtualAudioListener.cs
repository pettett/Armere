using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualAudioListener : MonoBehaviour
{
	public static readonly List<VirtualAudioListener> listeners = new List<VirtualAudioListener>();
	public float noiseThreshold;
	public bool pathfindNoiseVolume = true;
	public event System.Action<Vector3> onHearNoise;
	private void Start()
	{
		listeners.Add(this);
	}
	private void OnDestroy()
	{
		listeners.Remove(this);
	}

	public void OnHearNoise(Vector3 position)
	{
		onHearNoise?.Invoke(position);
	}
}
