using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicVolume : MonoBehaviour
{
    public static List<MusicVolume> volumes = new List<MusicVolume>();
    // Start is called before the first frame update
    void Start()
    {
        volumes.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
