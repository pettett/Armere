using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TimedEventGenerator : MonoBehaviour
{
	public UnityEvent Event;
	public float timeBetweenEvents = 1;
	// Start is called before the first frame update
	IEnumerator Start()
	{
		var wait = new WaitForSeconds(timeBetweenEvents);
		while (true)
		{
			yield return wait;
			Event.Invoke();

		}
	}

}
