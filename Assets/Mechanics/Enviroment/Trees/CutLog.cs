using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CutLog : MonoBehaviour, IAttackable
{
    public GameObject canopy;
    public GameObject trunk;

    public Vector2 lengthRegion;

    ItemName spawnedItem = ItemName.Stick;
    Vector2Int itemCount = new Vector2Int(1, 3);

    public async void Cut()
    {
        int spawns = Random.Range(itemCount.x, itemCount.y + 1);

        IEnumerable<Task<InteractableItem>> SpawnTasks()
        {
            for (int i = 0; i < spawns; i++)
            {
                yield return Items.SpawnItem(
                    InventoryController.singleton.db[spawnedItem] as PhysicsItemData,
                    transform.position + transform.up * Mathf.Lerp(lengthRegion.x, lengthRegion.y, (i + 0.5f) / (float)spawns),
                    Quaternion.Euler(0, Random.Range(0, 360), 0));
            }
        }

        await Task.WhenAll(
            SpawnTasks()
        );

        Destroy(gameObject);
    }

    public void Attack(ItemName weapon, GameObject controller, Vector3 hitPosition)
    {
        Cut();
    }

    //Remove the canopy when hit ground
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject != trunk && (other.rigidbody?.isKinematic ?? true))
        {
            //Only interact with kinematic rbs or nothing
            Destroy(canopy);
        }
    }
}
