using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockThrowSpell : Spell
{
	readonly Projectile createdRock;
	Vector3 vel;

	public RockThrowSpell(Character caster, Projectile rockPrefab) : base(caster)
	{
		//Make the rock come from the bottom
		createdRock = MonoBehaviour.Instantiate(rockPrefab, GetPoint() + Vector3.down * 3, Quaternion.identity);
	}
	Vector3 GetPoint()
	{
		return caster.transform.TransformPoint(new Vector3(
			Oscillate(1, 0.1f, 0.9f, 0),
			Oscillate(1.5f, 0.1f, 1f, 1.5f),
			Oscillate(1, 0.1f, 0.9f, 3)
		));
	}
	public static float Oscillate(float mean, float intensity, float frequency, float offset)
	{
		return Mathf.Sin(Time.time * frequency + offset) * intensity + mean;
	}
	public override void Update()
	{
		createdRock.transform.position = Vector3.SmoothDamp(
			createdRock.transform.position,
			GetPoint(),
			ref vel, 0.1f, 100);
	}

	public override void CancelCast(bool manualCancel)
	{
		MonoBehaviour.Destroy(createdRock.gameObject);
	}

	public override void Cast()
	{
		createdRock.LaunchProjectile(caster.transform.forward * 10);

	}

	public override void Begin()
	{
	}
}

[CreateAssetMenu(menuName = "Game/Spells/Rock Throw")]
public class RockThrowSpellAction : SpellAction
{
	public Projectile rockPrefab;
	public override Spell BeginCast(Character caster)
	{
		return new RockThrowSpell(caster, rockPrefab);
	}

}
