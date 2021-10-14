using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/NPCs/Animals/Act On Sound")]
public class AnimalActOnSoundTemplate : AnimalStateTemplate
{
	public AnimalStateContextTemplate<Vector3> act;
	public override AnimalState StartState(AnimalMachine machine)
	{
		return new AnimalActOnSound(machine, this);
	}
}

public class AnimalActOnSound : AnimalState<AnimalActOnSoundTemplate>
{
	readonly VirtualAudioListener virtualAudioListener;
	public AnimalActOnSound(AnimalMachine machine, AnimalActOnSoundTemplate t) : base(machine, t)
	{
		machine.AssertComponent(out virtualAudioListener);
		virtualAudioListener.onHearNoise += OnHearNoise;
	}
	public override void End()
	{
		virtualAudioListener.onHearNoise -= OnHearNoise;
	}
	public void OnHearNoise(Vector3 source)
	{
		machine.ChangeToState(t.act, source);
	}
}
