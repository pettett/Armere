using UnityEngine;
using Cinemachine;
using System.Collections;
public class SceneChangeTrigger : TriggerBox
{
    public CinemachineVirtualCamera transitionToCamera;
    public float dollyTime = 2f;
    private void Start()
    {
        onTriggerEnterEvent += OnPlayerEnter;
    }
    PlayerController.AutoWalking walker;
    public Transform endTransform;
    public LevelName changeToLevel;
    void OnPlayerEnter(Collider player)
    {
        transitionToCamera.Priority = 20;

        walker = player.GetComponent<PlayerController.Player_CharacterController>().ChangeToState<PlayerController.AutoWalking>();
        walker.WalkTo(endTransform.position);
        StartCoroutine(Dolly());
    }
    IEnumerator Dolly()
    {
        float t = 0;
        float m = 1 / dollyTime;
        var d = transitionToCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        while (t < 1)
        {
            t += Time.deltaTime * m;
            d.m_PathPosition = t;
            yield return new WaitForEndOfFrame();
        }
        LevelController.ChangeToLevel(changeToLevel);
    }
}