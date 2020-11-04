using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, WorldObjectComponent(typeof(AttackableComponent), "Attackable")]
public sealed class AttackableComponentSettings : WorldObjectDataComponentSettings
{
    public bool oneShotHit;
    [MyBox.ConditionalField("oneShotHit", true)] public float health;
    public Vector3 targetOffset;
}
