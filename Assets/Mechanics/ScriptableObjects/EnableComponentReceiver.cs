using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableGameObjectReceiver : MonoBehaviour
{
	public GameObject go;

	public BoolEventChannelSO enabler;

	private void OnEnable()
	{
		enabler.OnEventRaised += Enable;
	}
	private void OnDisable()
	{

		enabler.OnEventRaised -= Enable;
	}
	public void Enable(bool enabled)
	{
		go.SetActive(enabled);
	}
}
