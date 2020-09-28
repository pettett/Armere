using System.Threading.Tasks;
using UnityEngine;

public abstract class HoldableItemData : PhysicsItemData
{
    public async virtual Task<GameObject> CreatePlayerObject()
    {
        return await spawnedGameobject.InstantiateAsync().Task;
    }
    public virtual void OnItemEquip(Animator anim) { }
    public virtual void OnItemDeEquip(Animator anim) { }

}