using System.Threading.Tasks;
using UnityEngine;

public abstract class HoldableItemData : PhysicsItemData
{
    public async virtual Task<GameObject> CreatePlayerObject()
    {
        return await spawnedGameobject.InstantiateAsync().Task;
    }


    [Range(0, 1)]
    public float clankProbability = 0.7f;
    public AudioClipSet clankSet;

}