using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerBox : MonoBehaviour
{
	public float minTimeBetweenTriggers = Mathf.Infinity;
	float lastActivationTime;

	[System.Serializable]
	public class ColliderEvent : UnityEvent<Collider> { }

	public ColliderEvent onTriggerEnter;
	public bool requiresTag;
	[TagSelector] public string activatorTag = "Player";

	public event System.Action<Collider> onTriggerEnterEvent;

	private void Start()
	{
		GetComponent<Collider>().isTrigger = true;
		lastActivationTime = -minTimeBetweenTriggers;
	}

	protected void ResetTriggerTimer()
	{
		lastActivationTime = Time.time;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (requiresTag && !other.CompareTag(activatorTag)) return;
		if (lastActivationTime + minTimeBetweenTriggers > Time.time) return; //Not enough time passed to trigger again

		ResetTriggerTimer();

		onTriggerEnter.Invoke(other);
		onTriggerEnterEvent?.Invoke(other);

		OnTrigger(other);
		//Dont bother ever scanning again
		if (minTimeBetweenTriggers == Mathf.Infinity)
			GetComponent<Collider>().enabled = false;
	}

	public virtual void OnTrigger(Collider other) { }



}
