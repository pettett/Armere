using UnityEngine;

[CreateAssetMenu(fileName = "Music Track", menuName = "Game/Music Track", order = 0)]
public class MusicTrack : ScriptableObject
{
    public AudioClip dayLoop;
    public AudioClip nightOverrideLoop;
    [Range(0, 1)] public float volume = 1;
}