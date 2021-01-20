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
		onSavingBegin.onEventRaised += Enable;
		onSavingFinish.onEventRaised += Disable;
		Disable();
	}

	private void OnDestroy()
	{
		onSavingBegin.onEventRaised -= Enable;
		onSavingFinish.onEventRaised -= Disable;
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
