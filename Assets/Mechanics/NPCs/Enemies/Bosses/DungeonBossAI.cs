using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBossAI : BossAI
{
	//Responsible for providing the states of the boss fight
	[Range(0, 1)] //Stage 1 is over 1/3 of the health
	public float stageOneEndHealth = 2 / 3f;

	[Range(0, 1)] //Stage 2 default is over 1/2 of the health
	public float stageTwoEndHealth = 1 / 6f;

	int currentStage = 0;
	float[] healthStages;

	public override void Start()
	{
		base.Start();
		health.onTakeDamage += OnBossDamageTaken;
		health.onDeathEvent.AddListener(OnBossDied);

		healthStages = new float[] { stageOneEndHealth, stageTwoEndHealth };
	}

	public override void Init()
	{
		musicController.StartTrack();
		musicController.onLoopStart += MakeUnInvincible;

		health.blockingDamage = true;
		health.minBlockingDot = -2;
		//Make boss bar invincible
		UIController.singleton.bossBar.StartInvincible();
	}


	void MakeInvincible()
	{
		health.blockingDamage = true;
		//Make boss bar invincible
		UIController.singleton.bossBar.StartInvincible();

		musicController.ProgressLoop();
	}

	public void MakeUnInvincible()
	{

		health.blockingDamage = false;

		//Make boss bar not invincible
		UIController.singleton.bossBar.EndInvincible();
	}
	public void OnBossDied()
	{
		musicController.onLoopStart -= MakeUnInvincible;
		musicController.ProgressLoop();
	}
	public void OnBossDamageTaken(GameObject attacker, GameObject victim)
	{
		//Push the ai back
		float currentProportion = health.health / health.maxHealth;



		if (currentProportion < healthStages[currentStage])
		{
			//Heal up to the border
			health.SetHealth(healthStages[currentStage] * health.maxHealth);

			UIController.singleton.bossBar.UpdateHealth(healthStages[currentStage]);

			MakeInvincible();

			currentStage++;

			if (currentStage == healthStages.Length)
			{
				Debug.Log("Final Stage");
			}
			else
			{
				Debug.Log("Moving to next stage");
			}

		}
		else
		{
			UIController.singleton.bossBar.UpdateHealth(currentProportion);
		}


	}

}
