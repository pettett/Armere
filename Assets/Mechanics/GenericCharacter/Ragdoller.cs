using UnityEngine;

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

    bool ragdollEnabled;
    Animator animator;
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


        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            //kinimatic true when ragdolling is false
            collider.enabled = enabled;
        }

        if (TryGetComponent<Rigidbody>(out Rigidbody rigidbody)) rigidbody.isKinematic = enabled;
        //the main collider has the opposite state
        GetComponent<Collider>().enabled = !enabled;
    }
}