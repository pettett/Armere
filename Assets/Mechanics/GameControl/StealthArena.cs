using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthArena : MonoBehaviour
{
	public Barrier[] lockdownBarriers = new Barrier[0];
	public EnemyAISpawner[] enemyAISpawners = new EnemyAISpawner[0];
	public void OnEnemyDetectedPlayer(EnemyAI enemy)
	{
		//Pause the game

		//Focus on the enemy
		GameCameras.s.SetCameraTargets(enemy.transform);
		GameCameras.s.EnableCutsceneCamera();
		//Lock down the area
		foreach (var barrier in lockdownBarriers)
		{
			barrier.Close();
		}

		StartCoroutine(GiveControlBackToPlayer());
	}

	IEnumerator GiveControlBackToPlayer()
	{
		yield return new WaitForSeconds(2);

		//Return camera to player so he can be killed by op characters
		GameCameras.s.DisableCutsceneCamera();
	}
	public void RemoveAllEnemies()
	{
		foreach (var s in enemyAISpawners)
		{
			Destroy(s.body);
		}
	}
}
