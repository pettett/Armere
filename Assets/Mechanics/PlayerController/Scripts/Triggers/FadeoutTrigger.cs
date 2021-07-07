using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Armere.PlayerController;

using Armere.UI;
public class FadeoutTrigger : PlayerTrigger
{
	public Transform wakeupTransform;
	public override void OnPlayerTrigger(PlayerController player)
	{
		StartCoroutine(UIController.singleton.FullFade(0.25f, 1f));
		StartCoroutine(MovePlayer(player));
		//Transition from walking to walking
		player.machine.ChangeToState(TransitionStateTemplate.GenerateTransition(1, player.machine.defaultState));
	}

	IEnumerator MovePlayer(PlayerController player)
	{
		yield return new WaitForSeconds(0.5f);
		player.transform.SetPositionAndRotation(wakeupTransform.position, wakeupTransform.rotation);
	}


}
