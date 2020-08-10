using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
[CreateAssetMenu(fileName = "Camera Profile", menuName = "Game/Camera Profile", order = 0)]
public class CameraProfile : ScriptableObject
{
    public CinemachineFreeLook.Orbit topRig;
    public CinemachineFreeLook.Orbit middleRig;
    public CinemachineFreeLook.Orbit bottomRig;

}