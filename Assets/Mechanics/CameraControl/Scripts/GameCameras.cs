using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class GameCameras : MonoBehaviour
{
    public static GameCameras s;

    public float defaultTrackingOffset = 1.6f;
    float _playerTrackingOffset = 1.6f;

    public float defaultRigOffset = 0;
    float _playerRigOffset = 0;


    public float playerTrackingOffset
    {
        get => _playerTrackingOffset;
        set
        {
            _playerTrackingOffset = value;
            if (freeLook != null)
            {
                freeLook.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
                freeLook.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
                freeLook.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
            }
        }
    }


    public float playerRigOffset
    {
        get => _playerRigOffset;
        set
        {
            _playerRigOffset = value;
        }
    }

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
