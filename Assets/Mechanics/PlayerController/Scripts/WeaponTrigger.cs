using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    public System.Action<Collider> onTriggerEnter;
    private void OnTriggerEnter(Collider other)
    {
        print("Trigger Entered");
        onTriggerEnter?.Invoke(other);
    }
}
