using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/NPCs/Random Animations")]
public class RandomAnimationsTemplate : AnimalStateTemplate
{
	public AnimationTransition[] animations;
	public Vector2 animationIntervalRange = new Vector2(0.25f, 1);
	public override AnimalState StartState(AnimalMachine machine)
	{
		return new RandomAnimations(machine, this);
	}
}

public class RandomAnimations : AnimalState<RandomAnimationsTemplate>
{
	Coroutine routine;
	public RandomAnimations(AnimalMachine machine, RandomAnimationsTemplate t) : base(machine, t)
	{
		routine = machine.StartCoroutine(Routine());
	}
	public override void End()
	{
		machine.StopCoroutine(routine);
	}
	IEnumerator Routine()
	{
		while (true)
		{
			AnimationController.TriggerTransition(c.animator, t.animations[Random.Range(0, t.animations.Length)]);
			yield return AnimationController.WaitForAnimation(c.animator);

			yield return new WaitForSeconds(Random.Range(t.animationIntervalRange.x, t.animationIntervalRange.y));
		}
	}
}
