using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(WeaponGraphicsController), typeof(AnimationController))]
public abstract class Character : SpawnableBody
{
	public static Character playerCharacter;

	[System.NonSerialized] public WeaponGraphicsController weaponGraphics;
	[System.NonSerialized] public AnimationController animationController;

	public abstract void Knockout(float time);


}
