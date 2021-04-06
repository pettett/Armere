using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "Game/Minigames/Minigame")]
public class Minigame : ScriptableObject
{
	public string minigameFinishedConversationNode = "Finished";
	public string minigameResultVariableName = "HighScore";

	public MinigameConstraint goal;
	public MinigameConstraint[] constraints;
	public VoidEventChannelSO setupMinigame;
	public OnMinigameEventChannel onMinigameStarted;
	public System.Action<object> onMinigameEnded;
	public void StartMinigame(MonoBehaviour source)
	{
		Assert.IsNotNull(onMinigameStarted);
		Assert.IsNotNull(source);

		source.StartCoroutine(MinigameRoutine());

		onMinigameStarted.RaiseEvent(this);
	}
	public IEnumerator MinigameRoutine()
	{
		Assert.IsNotNull(setupMinigame);
		Assert.IsNotNull(goal);
		Assert.IsNotNull(constraints);

		bool running = true;

		setupMinigame.RaiseEvent();
		void EndMinigame()
		{
			running = false;
		}

		goal.InitConstraint(false, this, EndMinigame);
		for (int i = 0; i < constraints.Length; i++)
			constraints[i].InitConstraint(true, this, EndMinigame);

		goal.StartConstraint();
		for (int i = 0; i < constraints.Length; i++)
			constraints[i].StartConstraint();

		Debug.Log("Minigame starting");

		while (running)
		{
			yield return null;
			goal.UpdateConstraint();
			for (int i = 0; i < constraints.Length; i++)
				constraints[i].UpdateConstraint();
		}


		Debug.Log("Minigame ending");

		goal.ResetConstraint();
		for (int i = 0; i < constraints.Length; i++)
			constraints[i].ResetConstraint();

		onMinigameEnded?.Invoke(goal.result);
	}

}
