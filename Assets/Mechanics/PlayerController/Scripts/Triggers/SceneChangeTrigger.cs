using UnityEngine;
using Cinemachine;
using System.Collections;
using Armere.PlayerController;
public class SceneChangeTrigger : PlayerTrigger
{
    public CinemachineVirtualCamera transitionToCamera;
    public float dollyTime = 2f;
    AutoWalking walker;
    public Transform endTransform;
    public LevelName changeToLevel;
    public override void OnPlayerTrigger(PlayerController player)
    {
        StartSceneChange(player);
    }

    public void StartSceneChange(PlayerController player)
    {
        transitionToCamera.Priority = 20;
        walker = player.ChangeToState<AutoWalking>();
        walker.WalkTo(endTransform.position);
        StartCoroutine(Dolly());
    }
    IEnumerator Dolly()
    {
        float t = 0;
        float m = 1 / dollyTime;
        var d = transitionToCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        bool faded = false;
        while (t < 1)
        {
            t += Time.deltaTime * m;
            d.m_PathPosition = t;
            yield return new WaitForEndOfFrame();
            if (!faded && t > 0.6f)
            {
                faded = true;
                UIController.singleton.FadeOut(0.1f, changeToLevel.ToString());
            }
        }
        LevelController.ChangeToLevel(changeToLevel, null);

    }
}