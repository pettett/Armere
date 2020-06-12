using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Lantern", menuName = "Game/Items/Lantern", order = 0)]
public class Lantern : ItemPropertyBase
{
    public Color color;
    public override void CreatePlayerObject(GameObject mesh)
    {
        var holder = new GameObject("Lantern Holder");
        mesh.transform.SetParent(holder.transform);
        var l = mesh.AddComponent<Light>();
        l.color = color;
    }
    public override void OnItemEquip(Animator anim)
    {
        anim.SetBool("HoldingLantern", true);
    }
    public override void OnItemDeEquip(Animator anim)
    {
        anim.SetBool("HoldingLantern", false);
    }
}
