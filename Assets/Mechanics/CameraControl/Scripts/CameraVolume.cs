using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
[RequireComponent(typeof(Collider))]
public class CameraVolume : MonoBehaviour
{
    [HideInInspector] public Collider c;

    public float blendDistance = 1;
    public CinemachineFreeLook.Orbit topRig;
    public CinemachineFreeLook.Orbit middleRig;
    public CinemachineFreeLook.Orbit bottomRig;

    private void OnEnable()
    {
        CameraVolumeController.Register(this);
        c = GetComponent<Collider>();
    }
    private void OnDisable()
    {
        CameraVolumeController.UnRegister(this);
    }
}
