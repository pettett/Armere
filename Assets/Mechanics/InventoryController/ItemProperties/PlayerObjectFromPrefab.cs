using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerObjectFromPrefab", menuName = "Game/Items/PlayerObjectFromPrefab")]
public class PlayerObjectFromPrefab : ItemPropertyBase
{
    public GameObject prefab;
    public override GameObject CreatePlayerObject(ItemName name, ItemDatabase db)
    {
        return Instantiate(prefab);
    }
}
