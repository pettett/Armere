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

	[SerializeField, ReadOnly] bool ragdollEnabled;
	public bool alwaysKinematic = false;
	public Transform hips;
	Animator animator;


	private void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		enabled = false;
	}
	[MyBox.ButtonMethod]
	void Ragdoll()
	{
		RagdollEnabled = true;
	}
	[MyBox.ButtonMethod]
	void Compose()
	{
		RagdollEnabled = false;
	}

	void SetRagdollActive(bool enabled)
	{
		ragdollEnabled = enabled;

		animator.enabled = !enabled;

		this.enabled = enabled;



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



		if (TryGetComponent<Rigidbody>(out Rigidbody rigidbody)) rigidbody.isKinematic = enabled || alwaysKinematic;
		//the main collider has the opposite state
		GetComponent<Collider>().enabled = !enabled;
	}
	private void Update()
	{
		transform.position = hips.position;
		hips.localPosition = default;
	}

}