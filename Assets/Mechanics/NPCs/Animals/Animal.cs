using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Armere.Inventory;

public class Animal : MonoBehaviour
{
	public Animator animator;
	public AnimalMachine machine;

	public void Disappear()
	{
		LeanTween.scale(gameObject, Vector3.zero, 0.2f).setOnComplete(
			() =>
			{
				Destroy(gameObject);
			}
		);
	}

}