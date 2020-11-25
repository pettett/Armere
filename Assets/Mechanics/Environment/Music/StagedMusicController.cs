using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagedMusicController : MonoBehaviour
{
    public AudioClip mainSong;

    [System.Serializable]
    public struct LoopSection
    {
        public AudioClip clip;
        public float startPoint;
    }

    public LoopSection[] loopSections;


    AudioSource mainSource;
    AudioSource playingLoopSource;
    AudioSource nextLoopSource;
    int loopStage = 0;

    bool progress = false;

    public event System.Action onLoopStart;

    // Start is called before the first frame update
    void Start()
    {
        playingLoopSource = gameObject.AddComponent<AudioSource>();
        playingLoopSource.loop = true;

        nextLoopSource = gameObject.AddComponent<AudioSource>();
        nextLoopSource.loop = true;

        mainSource = gameObject.AddComponent<AudioSource>();
    }
    public void StartTrack()
    {
        PlayFirstTransition();
    }


    public void ProgressLoop()
    {
        //StartCoroutine(PlayTransition());
        double time = AudioSettings.dspTime;
        float timeToLoopEnd = playingLoopSource.clip.length - playingLoopSource.time;
        double switchTime = time + timeToLoopEnd;

        playingLoopSource.SetScheduledEndTime(switchTime);
        mainSource.time = loopSections[loopStage].startPoint + loopSections[loopStage].clip.length;
        mainSource.PlayScheduled(switchTime);

        if (loopStage != loopSections.Length - 1)
        {
            //Also schedule the next loop
            loopStage++;

            float timeBetweenLoops = loopSections[loopStage].startPoint - (loopSections[loopStage - 1].startPoint + loopSections[loopStage - 1].clip.length);

            double loopStartTime = switchTime + timeBetweenLoops;

            nextLoopSource.clip = loopSections[loopStage].clip;
            nextLoopSource.time = 0;
            nextLoopSource.PlayScheduled(loopStartTime);
            mainSource.SetScheduledEndTime(loopStartTime);

            (nextLoopSource, playingLoopSource) = (playingLoopSource, nextLoopSource);

            StartCoroutine(WaitForLoopStart(timeToLoopEnd + timeBetweenLoops));
        }
    }

    public void PlayFirstTransition()
    {
        playingLoopSource.clip = loopSections[loopStage].clip;

        mainSource.clip = mainSong;
        mainSource.time = 0;


        double time = AudioSettings.dspTime + 1;

        mainSource.PlayScheduled(time);
        mainSource.SetScheduledEndTime(time + loopSections[loopStage].startPoint);
        playingLoopSource.PlayScheduled(time + loopSections[loopStage].startPoint);


        // yield return new WaitUntil(() => mainSource.time >= loopSections[loopStage].startPoint);

        // mainSource.Stop();
        // yield return PlayLoop();

        StartCoroutine(WaitForLoopStart(loopSections[loopStage].startPoint + 1));
    }

    IEnumerator WaitForLoopStart(float length)
    {
        yield return new WaitForSecondsRealtime(length);
        onLoopStart?.Invoke();
    }


}
