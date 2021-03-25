using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffDisplay : MonoBehaviour
{
	Character.Buff buff;

	public TMPro.TextMeshProUGUI title;
	public TMPro.TextMeshProUGUI timeRemaining;


	public void OnBuffAdded(Character.Buff buff)
	{
		title.SetText(buff.buff.ToString());
		this.buff = buff;
	}
	private void Update()
	{
		timeRemaining.SetText(buff.remainingTime.ToString("T|0:00"));
		if (buff.remainingTime <= 0)
		{
			Destroy(gameObject);
		}
	}
}