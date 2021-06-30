using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{

	public readonly List<MusicVolume> volumes = new List<MusicVolume>();
	public static MusicController instance;

	public Dictionary<MusicTrack, int> currentTracks = new Dictionary<MusicTrack, int>();
	List<AudioSource> activeSources = new List<AudioSource>(1);
	Queue<AudioSource> pooledSources = new Queue<AudioSource>();


	public void Register(MusicVolume v)
	{
		volumes.Add(v);
	}
	public void Unregister(MusicVolume v)
	{
		volumes.Remove(v);
	}

	// Start is called before the first frame update
	void OnEnable()
	{
		instance = this;
	}


	// Update is called once per frame
	void Update()
	{
		//Go though all the volumes and test to see which one is closest
		foreach (var vol in volumes)
		{
			Vector3 closestPoint = vol.ClosestPoint(LevelInfo.currentLevelInfo.playerTransform.position);
			float distance = (closestPoint - LevelInfo.currentLevelInfo.playerTransform.position).sqrMagnitude;

			float sqrRadius = vol.m_blendDistance * vol.m_blendDistance;

			if (distance < sqrRadius)
			{
				float fade = 1 - distance / sqrRadius;

				if (!currentTracks.ContainsKey(vol.m_track))
				{
					//Play the track if not already there
					Play(vol.m_track);
				}
				activeSources[currentTracks[vol.m_track]].volume = fade;
			}
			else
			{
				if (currentTracks.ContainsKey(vol.m_track))
				{
					//This will break if more then 1 volume has the same music
					Stop(vol.m_track);
				}
			}
		}
	}

	AudioSource AddSource()
	{
		var s = gameObject.AddComponent<AudioSource>();
		s.loop = true;
		return s;
	}
	public void Play(MusicTrack t)
	{
		//Start looping this track into play
		if (pooledSources.Count == 0)
			activeSources.Add(AddSource());
		else
			activeSources.Add(pooledSources.Dequeue());
		int i = activeSources.Count - 1;

		currentTracks[t] = i;
		activeSources[i].clip = t.dayLoop;
		activeSources[i].enabled = true;
		activeSources[i].volume = t.volume;
		activeSources[i].Play();
	}

	public void Stop(MusicTrack t)
	{
		//Cancel the playing track
		int i = currentTracks[t];
		currentTracks.Remove(t);
		activeSources[i].Stop();
		activeSources[i].enabled = false;
		pooledSources.Enqueue(activeSources[i]);
		activeSources.RemoveAt(i);

	}

	public static void SetTrackVolume(MusicTrack t, float volume)
	{
		instance.activeSources[instance.currentTracks[t]].volume = volume;
	}
	public static bool TrackPlaying(MusicTrack t)
	{
		return instance.currentTracks.ContainsKey(t);
	}
}
