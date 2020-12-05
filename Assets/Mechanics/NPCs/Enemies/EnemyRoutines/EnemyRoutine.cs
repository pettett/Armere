using UnityEngine;
using System.Collections;

public abstract class EnemyRoutine : ScriptableObject
{
    public abstract IEnumerator Routine(EnemyAI enemy);
}