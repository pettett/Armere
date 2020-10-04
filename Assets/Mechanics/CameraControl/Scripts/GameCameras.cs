using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class GameCameras : MonoBehaviour
{
    public static GameCameras s;
    private void Awake()
    {
        s = this;
    }
    private void Start()
    {
        freeLook.Follow = LevelInfo.currentLevelInfo.playerTransform;
        freeLook.LookAt = LevelInfo.currentLevelInfo.playerTransform;

        freeLookAim.Follow = LevelInfo.currentLevelInfo.playerTransform;
        freeLookAim.LookAt = LevelInfo.currentLevelInfo.playerTransform;
    }

    public void SwitchCinemachineCameras(Cinemachine.CinemachineFreeLook from, Cinemachine.CinemachineFreeLook to)
    {
        //Switch priorities

        from.Priority = 10;
        to.Priority = 20;

        to.m_XAxis.Value = from.m_XAxis.Value;
        to.m_YAxis.Value = from.m_YAxis.Value;
    }
    public void SwitchToAimCamera() => SwitchCinemachineCameras(freeLook, freeLookAim);
    public void SwitchToNormalCamera() => SwitchCinemachineCameras(freeLookAim, freeLook);

    public Transform cameraTransform;
    public CinemachineFreeLook freeLook;
    public CinemachineFreeLook freeLookAim;
    public CinemachineTargetGroup conversationGroup;
    public CinemachineVirtualCamera cutsceneCamera;
}
