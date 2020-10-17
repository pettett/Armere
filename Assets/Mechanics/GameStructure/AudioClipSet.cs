using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Audio Clip Set", menuName = "Game/Audio Clip Set", order = 0)]
public class AudioClipSet : ScriptableObject
{
    public AudioClip[] clips;
    public bool Valid()
    {
        return clips != null && clips.Length != 0;
    }
    public AudioClip SelectClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }
}