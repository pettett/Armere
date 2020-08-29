using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerController;

public class FadeoutTrigger : PlayerTrigger
{
    public Transform wakeupTransform;
    public override void OnPlayerTrigger(Player_CharacterController player)
    {
        StartCoroutine(UIController.singleton.FullFade(0.25f, 1f));
        StartCoroutine(MovePlayer(player));
        //Transition from walking to walking
        player.ChangeToState<TransitionState<Walking>>(1f);
    }

    IEnumerator MovePlayer(Player_CharacterController player)
    {
        yield return new WaitForSeconds(0.5f);
        player.transform.SetPositionAndRotation(wakeupTransform.position, wakeupTransform.rotation);
    }


}
