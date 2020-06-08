using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportWaypoints : MonoBehaviour
{

    public static TeleportWaypoints singleton;
    [SerializeField] private Transform[] _waypoints;
    public Dictionary<string, Transform> waypoints;
    private void Awake()
    {
        singleton = this;
        waypoints = new Dictionary<string, Transform>(_waypoints.Length);
        for (int i = 0; i < _waypoints.Length; i++)
        {
            waypoints[_waypoints[i].name] = _waypoints[i];
        }
    }
}
