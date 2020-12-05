using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class VirtualAudioController : MonoBehaviour
{
    public static VirtualAudioController singleton;

    [System.NonSerialized] public List<VirtualAudioListener> listeners = new List<VirtualAudioListener>();
    private void Awake()
    {
        singleton = this;
    }
    public void MakeNoise(Vector3 position, float volume)
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            float distance = Vector3.Distance(position, listeners[i].transform.position);
            //For every doubling of distance, the sound level reduces by 6 decibels (dB),
            float relitiveVolume = volume - Mathf.Log(distance, 2) * 6;

            if (relitiveVolume > listeners[i].noiseThreshold)
            {
                if (listeners[i].pathfindNoiseVolume)
                {
                    //Calculate a new distance with a pathfinding sample
                    NavMeshPath path = new NavMeshPath();
                    NavMesh.CalculatePath(position, listeners[i].transform.position, -1, path);
                    distance = 0;
                    for (int c = 0; c < path.corners.Length - 1; c++)
                    {
                        distance += Vector3.Distance(path.corners[c], path.corners[c + 1]);
                        Debug.DrawLine(path.corners[c], path.corners[c + 1], Color.blue, 10);
                    }

                    relitiveVolume = volume - Mathf.Log(distance, 2) * 6;
                    if (relitiveVolume > listeners[i].noiseThreshold)
                    {
                        //The virtual listener can hear this
                        listeners[i].OnHearNoise(position);
                    }
                }
                else
                {
                    //The virtual listener can hear this without pathfinding - through walls
                    listeners[i].OnHearNoise(position);
                }
            }
        }
    }
}