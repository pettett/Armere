using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraVolume : MonoBehaviour
{
    [HideInInspector] public Collider c;
    public bool global = false;

    public float blendDistance = 1;
    public CameraProfile profile;
    private void OnEnable()
    {

        CameraVolumeController.Register(this, global);
        if (!global)
            c = GetComponent<Collider>();

    }
    private void OnDisable()
    {

        CameraVolumeController.UnRegister(this, global);

    }
}
