using UnityEngine;
using Armere.Inventory;

[System.Flags]
public enum AttackResult
{
	None = 0b0000,
	Damaged = 0b0001,
	Blocked = 0b0010,
	Headshot = 0b0100,
	Killed = 0b1000
}


public interface IAttackable : IScanable
{
	AttackResult Attack(AttackFlags flags, WeaponItemData weapon, GameObject controller, Vector3 hitPosition);
}