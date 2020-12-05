using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guard Routine", menuName = "Game/Guard Routine", order = 0)]
public class GuardRoutine : EnemyRoutine
{
    public override IEnumerator Routine(EnemyAI enemy)
    {
        enemy.investigateOnSight = true;
        int waypoint = enemy.GetClosestWaypoint();
        yield return enemy.GoToWaypoint(waypoint);
    }
}