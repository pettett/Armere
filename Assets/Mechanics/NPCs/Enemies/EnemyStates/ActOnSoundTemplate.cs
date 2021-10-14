using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/NPCs/Enemies/Act on Sound")]
public class ActOnSoundTemplate : AIStateTemplate
{
	public AIContextStateTemplate<Vector3> onHeard;

	public override AIState StartState(AIMachine machine)
	{
		return new ActOnSound(machine, this);
	}
}
public class ActOnSound : AIState<ActOnSoundTemplate>
{

	readonly VirtualAudioListener virtualAudioListener;
	public ActOnSound(AIMachine machine, ActOnSoundTemplate t) : base(machine, t)
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
		machine.ChangeToState(t.onHeard, source);
	}
}