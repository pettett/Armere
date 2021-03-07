using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractureable : MonoBehaviour
{
	public GameObject[] fractureOrder;
	public float fractureForce = 50;
	void Fracture(GameObject fracturePiece)
	{
		var rb = fracturePiece.GetComponent<Rigidbody>();
		rb.isKinematic = false;
		rb.AddForce(transform.TransformDirection(fracturePiece.transform.localPosition.normalized) * fractureForce, ForceMode.Acceleration);
		Destroy(fracturePiece, 5);
	}
	public void FractureFromHealthChange(float health, float maxHealth)
	{
		float t = 1 - health / maxHealth;
		t *= fractureOrder.Length;
		int fractures = Mathf.FloorToInt(t);

		for (int i = 0; i < fractures; i++)
		{
			if (fractureOrder[i] != null)
			{
				Fracture(fractureOrder[i]);
				fractureOrder[i] = null;
			}
		}
	}
	public void FractureAll()
	{
		//		Debug.Log("Fracture all");
		foreach (Transform child in transform)
		{
			Fracture(child.gameObject);
		}
		GetComponent<Collider>().enabled = false;
		Destroy(gameObject, 5);
	}
	private void OnDrawGizmosSelected()
	{
		foreach (Transform child in transform)
		{
			Gizmos.DrawLine(child.position, child.position + transform.TransformDirection(child.localPosition.normalized));
		}
	}
}
