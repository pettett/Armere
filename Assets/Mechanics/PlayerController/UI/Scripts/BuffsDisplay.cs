using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffsDisplay : MonoBehaviour
{
	public OnBuffChangedEventChannel buffs;

	public GameObject prefab;
	private void OnEnable()
	{
		buffs.OnEventRaised += OnBuffAdded;
	}
	private void OnDisable()
	{
		buffs.OnEventRaised -= OnBuffAdded;
	}

	public void OnBuffAdded(Character.Buff buff)
	{
		Debug.Log("New buff");
		Instantiate(prefab, transform).GetComponent<BuffDisplay>().OnBuffAdded(buff);
	}

}