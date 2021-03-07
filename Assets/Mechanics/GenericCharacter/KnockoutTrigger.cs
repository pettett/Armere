using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockoutTrigger : MonoBehaviour
{
	public float knockoutTime = 4f;
	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out Character c))
		{
			c.Knockout(knockoutTime);
		}
	}
}
