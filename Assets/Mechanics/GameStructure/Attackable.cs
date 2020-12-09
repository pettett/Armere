using UnityEngine;
[System.Flags]
public enum AttackResult
{
    None = 0b0000,
    Damaged = 0b0001,
    Blocked = 0b0010,
    Headshot = 0b0100,
    Killed = 0b1000
}


public enum AttackFlags
{
    Blunt,
    Sharp,
    Explosive
}

public interface IAttackable : IScanable
{
    AttackResult Attack(AttackFlags flags, ItemName weapon, GameObject controller, Vector3 hitPosition);
}