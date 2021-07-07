using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/NPCs/Act On Sound")]
public class ActOnSoundTemplate : AnimalStateTemplate
{
	public AnimalStateTemplate act;
	public override AnimalState StartState(AnimalMachine machine)
	{
		return new ActOnSound(machine, this);
	}
}

public class ActOnSound : AnimalState<ActOnSoundTemplate>
{
	readonly VirtualAudioListener virtualAudioListener;
	public ActOnSound(AnimalMachine machine, ActOnSoundTemplate t) : base(machine, t)
	{
		virtualAudioListener = machine.GetComponent<VirtualAudioListener>();
		virtualAudioListener.onHearNoise += OnHearNoise;
	}
	public override void End()
	{
		virtualAudioListener.onHearNoise -= OnHearNoise;
	}
	public void OnHearNoise(Vector3 source)
	{
		machine.ChangeToState(t.act);
	}
}
