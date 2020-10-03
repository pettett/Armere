using UnityEngine;


public interface IAttackable
{
    void Attack(ItemName weapon, GameObject controller, Vector3 hitPosition);
}

public class WeaponTrigger : MonoBehaviour
{
    public ItemName weaponItem;
    public GameObject controller;
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject != controller.gameObject && other.TryGetComponent<IAttackable>(out var attackable))
        {
            attackable.Attack(weaponItem, controller, GetComponent<Collider>().bounds.center);
        }

    }
}
