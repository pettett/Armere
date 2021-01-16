using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "Void Channel", menuName = "Channels/Void Channel")]
public class VoidEventChannelSO : ScriptableObject
{
	public UnityAction onEventRaised;
	public void RaiseEvent()
	{
		onEventRaised?.Invoke();
	}
}