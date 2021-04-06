using UnityEngine;
using UnityEngine.Events;
public abstract class EventChannelSO<T0> : ScriptableObject
{
	public event UnityAction<T0> OnEventRaised;
	public void RaiseEvent(T0 arg)
	{
		OnEventRaised?.Invoke(arg);
	}
}
public abstract class EventChannelSO<T0, T1> : ScriptableObject
{
	public event UnityAction<T0, T1> OnEventRaised;
	public void RaiseEvent(T0 arg0, T1 arg1)
	{
		OnEventRaised?.Invoke(arg0, arg1);
	}
}