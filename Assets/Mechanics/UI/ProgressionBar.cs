using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ProgressionBar : MonoBehaviour
{
	public TextMeshProUGUI display;
	public string displayFormat = "n1";
	public Image displayImage;

	public FloatFloatEventChannelSO valueChangedChannel;
	private void OnEnable()
	{
		if (valueChangedChannel != null)
			valueChangedChannel.OnEventRaised += SetProgress;
	}
	private void OnDisable()
	{
		if (valueChangedChannel != null)
			valueChangedChannel.OnEventRaised -= SetProgress;
	}
	public void SetProgress(float progress, float maxProgress)
	{
		display.text = progress.ToString(displayFormat);
		displayImage.fillAmount = progress / maxProgress;
	}
}
