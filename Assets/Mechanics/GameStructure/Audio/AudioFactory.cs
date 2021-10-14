using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFactory : MonoBehaviour
{
	public AudioEventChannelSO makeNoise;
	public int maxSourcesBeforeCull = 5;

	readonly Queue<AudioSource> inactive = new Queue<AudioSource>(1);

	private void OnEnable()
	{
		makeNoise.OnEventRaised += MakeNoise;
	}
	private void OnDisable()
	{
		makeNoise.OnEventRaised -= MakeNoise;
	}
	public void MakeNoise(AudioClip clip, AudioEventData data)
	{
		StartCoroutine(PlayClip(clip, data));
	}
	public AudioSource GetSource()
	{
		if (inactive.Count > 0)
		{
			var source = inactive.Dequeue();
			source.gameObject.SetActive(true);
			return source;
		}
		else
		{
			var go = new GameObject("Audio source", typeof(AudioSource));
			go.transform.SetParent(transform);
			return go.GetComponent<AudioSource>();
		}
	}
	public IEnumerator PlayClip(AudioClip clip, AudioEventData data)
	{
		AudioSource source = GetSource();

		source.clip = clip;
		source.loop = false;
		source.transform.position = data.position;
		source.spatialBlend = data.profile.spacialBlend;

		source.Play();

		yield return new WaitForSeconds(clip.length);


		if (inactive.Count > maxSourcesBeforeCull)
		{
			//Destroy
			Destroy(source.gameObject);
		}
		else
		{
			source.gameObject.SetActive(false);
			inactive.Enqueue(source);
		}
	}
}
