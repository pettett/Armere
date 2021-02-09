using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingIndicatorUI : MonoBehaviour
{
	public VoidEventChannelSO onSavingBegin;
	public VoidEventChannelSO onSavingFinish;

	// Start is called before the first frame update
	void Start()
	{
		onSavingBegin.OnEventRaised += Enable;
		onSavingFinish.OnEventRaised += Disable;
		Disable();
	}

	private void OnDestroy()
	{
		onSavingBegin.OnEventRaised -= Enable;
		onSavingFinish.OnEventRaised -= Disable;
	}

	public void Enable()
	{
		gameObject.SetActive(true);
	}
	public void Disable()
	{
		gameObject.SetActive(false);
	}
}
