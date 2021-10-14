using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class ArmereBehaviour : MonoBehaviour
{
	public void AssertComponent<T>(out T comp) => Assert.IsTrue(TryGetComponent(out comp));
	public void FindComponent<T>(out T comp)
	{
		if (!TryGetComponent(out comp))
			comp = GetComponentInChildren<T>();


	}

}
