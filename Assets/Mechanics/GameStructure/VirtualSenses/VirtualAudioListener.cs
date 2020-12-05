using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualAudioListener : MonoBehaviour
{
    public float noiseThreshold;
    public bool pathfindNoiseVolume = true;
    public event System.Action<Vector3> onHearNoise;
    private void Start()
    {
        VirtualAudioController.singleton.listeners.Add(this);
    }
    private void OnDestroy()
    {
        VirtualAudioController.singleton.listeners.Remove(this);
    }

    public void OnHearNoise(Vector3 position)
    {
        onHearNoise?.Invoke(position);
    }
}
