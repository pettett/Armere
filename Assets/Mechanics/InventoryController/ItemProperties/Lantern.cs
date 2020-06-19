using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Lantern", menuName = "Game/Items/Lantern", order = 0)]
public class Lantern : ItemPropertyBase
{
    public Color color;
    public float range = 15;
    public float intensity = 3;
    public override void CreatePlayerObject(GameObject mesh)
    {
        var holder = new GameObject("Lantern Holder");
        mesh.transform.SetParent(holder.transform);
        var l = mesh.AddComponent<Light>();
        l.color = color;
        l.range = range;
        l.intensity = intensity;

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
