using UnityEngine;
using System.Collections;

public interface IEnemyRoutine
{
    bool alertOnAttack { get; }
    bool searchOnEvent { get; }
    bool investigateOnSight { get; }
    IEnumerator Routine(EnemyAI enemy);
}
public abstract class EnemyRoutine : ScriptableObject, IEnemyRoutine
{
    public abstract bool alertOnAttack { get; }
    public abstract bool searchOnEvent { get; }
    public abstract bool investigateOnSight { get; }
    public abstract IEnumerator Routine(EnemyAI enemy);
}