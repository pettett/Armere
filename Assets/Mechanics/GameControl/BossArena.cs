using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;

public class BossArena : MonoBehaviour
{
    public PlayableDirector entranceCutsceneDirector;

    public UnityEvent onFightStart;
    public UnityEvent onFightEnd;
    public Transform bossSpawn;
    public AssetReferenceGameObject boss;


    public bool debugStart = true;

    BossAI bossAI;

    public StagedMusicController musicController;


    public void Start()
    {
        if (debugStart)
            SpawnBoss();
    }

    public void StartEntranceCutscene()
    {
        entranceCutsceneDirector.Play();
        onFightStart.Invoke();
    }

    public async void SpawnBoss()
    {
        GameObject go = await boss.InstantiateAsync(bossSpawn.position, Quaternion.identity).Task;
        bossAI = go.GetComponent<BossAI>();

        bossAI.health.onDeath += OnBossDied;
        bossAI.musicController = musicController;
        bossAI.Init();
        UIController.singleton.bossBar.StartBossBarTracking(go.name);
    }

    public void OnBossDied(GameObject attacker, GameObject victim)
    {
        UIController.singleton.bossBar.UpdateHealth(0);
        onFightEnd.Invoke();
        StartCoroutine(StartEndingCutscene());
    }



    public IEnumerator StartEndingCutscene()
    {
        yield return new WaitForSeconds(1);
        yield return UIController.singleton.bossBar.StopBossBar();
    }
}
