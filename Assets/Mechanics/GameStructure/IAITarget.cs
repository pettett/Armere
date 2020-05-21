using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IAITarget
{
    GameObject gameObject { get; }
    Collider collider { get; }

    bool canBeTargeted { get; }
    Vector3 velocity { get; }

    int engagementCount { get; set; }


    //should return whether or not the ai is going to investigate the sound
    void HearSound(IAITarget source, float volume, ref bool responded);
}
