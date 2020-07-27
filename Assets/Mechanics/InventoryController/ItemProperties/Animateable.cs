using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Animateable", menuName = "Game/Items/Animatable")]
public class Animateable : ItemPropertyBase
{
    public RuntimeAnimatorController animatorController;
    public Avatar avatar;
    public override void CreatePlayerObject(GameObject mesh)
    {
        var a = mesh.AddComponent<Animator>();
        a.runtimeAnimatorController = animatorController;
        a.Update(0.1f);
    }
}
