using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{


    [CreateAssetMenu(menuName = "Game/Items/Lantern Item Data", fileName = "New Lantern Item Data")]
    [AllowItemTypes(ItemType.SideArm)]
    public class LanternItemData : SideArmItemData
    {
        public Color color;
        public float range = 15;
        public float intensity = 3;

        // public override GameObject CreatePlayerObject(ItemName name, ItemDatabase db)
        // {
        //     var holder = new GameObject("Lantern Holder");
        //     GameObject mesh = base.CreatePlayerObject(name, db);
        //     mesh.transform.SetParent(holder.transform);
        //     var l = mesh.AddComponent<Light>();
        //     l.color = color;
        //     l.range = range;
        //     l.intensity = intensity;
        //     return holder;
        // }

        // public override void OnItemEquip(Animator anim)
        // {
        //     anim.SetBool("HoldingLantern", true);
        // }
        // public override void OnItemDeEquip(Animator anim)
        // {
        //     anim.SetBool("HoldingLantern", false);
        // }
    }
}