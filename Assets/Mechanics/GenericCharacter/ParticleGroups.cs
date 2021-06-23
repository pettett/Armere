using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGroups : MonoBehaviour
{
	[System.Serializable]
	public struct ParticleGroup
	{
		public string name;
		public ParticleSystem[] particles;
	}
	public ParticleGroup[] groups;

	public int GetGroup(string name)
	{
		for (int i = 0; i < groups.Length; i++)
		{
			if (groups[i].name == name)
			{
				return i;
			}
		}
		throw new System.ArgumentException("No group in groups");
	}
	public void PlayGroup(string group) => PlayGroup(GetGroup(group));
	public void PlayGroup(int group) => ForEachParticle(group, x => x.Play());

	public void PauseGroup(string group) => PauseGroup(GetGroup(group));
	public void PauseGroup(int group) => ForEachParticle(group, x => x.Pause());


	public void ForEachParticle(int group, System.Action<ParticleSystem> ForEach)
	{
		for (int i = 0; i < groups[group].particles.Length; i++)
		{
			ForEach.Invoke(groups[group].particles[i]);
		}
	}
}
