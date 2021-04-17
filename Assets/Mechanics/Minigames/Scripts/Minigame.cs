using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Yarn;

[CreateAssetMenu(menuName = "Game/Minigames/Minigame")]
public class Minigame : ScriptableObject, IVariableAddon
{

	public MinigameConstraint goal;
	public MinigameConstraint[] constraints;
	public VoidEventChannelSO setupMinigame;
	public OnMinigameEventChannel onMinigameStarted;
	public System.Action<object> onMinigameEnded;

	public string prefix => "$Minigame_";
	bool running = false;
	public Value this[string name]
	{
		get => name.ToLower() switch
		{
			"running" => new Value(running),
			"result" => new Value(goal.result),
			_ => Value.NULL
		}; set => throw new System.NotImplementedException();
	}

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

		running = true;

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

	public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
	{
		throw new System.NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new System.NotImplementedException();
	}
}
