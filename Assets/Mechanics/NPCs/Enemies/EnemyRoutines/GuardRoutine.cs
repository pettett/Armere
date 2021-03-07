using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guard Routine", menuName = "Game/NPCs/Guard Routine", order = 0)]
public class GuardRoutine : EnemyRoutine
{
	public override bool alertOnAttack => true;

	public override bool searchOnEvent => true;

	public override bool investigateOnSight => true;

	public override IEnumerator Routine(EnemyAI enemy)
	{
		enemy.debugText.SetText("Guarding");


		int waypoint = enemy.GetClosestWaypoint();
		yield return enemy.GoToWaypoint(waypoint);
	}
}