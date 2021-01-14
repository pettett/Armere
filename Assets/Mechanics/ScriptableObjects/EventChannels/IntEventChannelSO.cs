using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "Void Int Channel", menuName = "Channels/Void Int Channel")]
public class IntEventChannelSO : ScriptableObject
{
	public UnityAction<int> OnEventRaised;
	public void RaiseEvent(int arg)
	{
		OnEventRaised?.Invoke(arg);
	}
}