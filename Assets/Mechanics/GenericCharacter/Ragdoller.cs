using UnityEngine;
using System.Linq;
public class Ragdoller : MonoBehaviour
{
	public bool RagdollEnabled
	{
		get
		{
			return ragdollEnabled;
		}
		set
		{
			SetRagdollActive(value);
		}
	}

	[SerializeField] bool ragdollEnabled;
	public bool alwaysKinematic = false;
	public Transform hips;
	Animator animator;

	Vector3 startingPos;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	void SetRagdollActive(bool enabled)
	{
		ragdollEnabled = enabled;

		animator.enabled = !enabled;



		foreach (var rb in GetComponentsInChildren<Rigidbody>())
		{
			//kinimatic true when ragdolling is false
			rb.isKinematic = !enabled;
		}


		foreach (var collider in GetComponentsInChildren<Collider>().Where(x => !x.isTrigger))
		{
			//kinimatic true when ragdolling is false
			collider.enabled = enabled;
		}

		if (!ragdollEnabled)
		{
			transform.position = hips.position;
		}
		else
		{
			startingPos = hips.position;
		}


		if (TryGetComponent<Rigidbody>(out Rigidbody rigidbody)) rigidbody.isKinematic = enabled || alwaysKinematic;
		//the main collider has the opposite state
		GetComponent<Collider>().enabled = !enabled;
	}

}