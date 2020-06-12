using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
public class WeaponGraphicsController : MonoBehaviour
{
    //Class to sort out locking objects to parts of the body
    [System.Serializable]
    public class HoldPoint
    {
        public HumanBodyBones anchor;
        public Vector3 posOffset;
        public Quaternion rotOffset;
        Transform anchorTrans;
        public void Init(Animator animator)
        {
            anchorTrans = animator.GetBoneTransform(anchor);
        }
        public void Anchor(Transform t)
        {
            t.SetPositionAndRotation(anchorTrans.TransformPoint(posOffset), anchorTrans.rotation * rotOffset);
        }

    }
    public HoldPoint weaponHoldPoint;
    public HoldPoint sheathedHoldPoint;
    public HoldPoint sideArmHoldPoint;


    public GameObject heldWeapon;

    public GameObject heldSidearm;

    public bool swordSheathed;
    private void Start()
    {
        Animator a = GetComponent<Animator>();
        weaponHoldPoint.Init(a);
        sheathedHoldPoint.Init(a);
        sideArmHoldPoint.Init(a);
    }

    public void SetHeldSidearm(ItemName s, ItemDatabase db) => SetHeld(ref heldSidearm, s, db);
    public void SetHeldWeapon(ItemName weapon, ItemDatabase db) => SetHeld(ref heldWeapon, weapon, db);
    public void RemoveSidearm() => Destroy(heldSidearm);
    public void RemoveWeapon() => Destroy(heldWeapon);
    void SetHeld(ref GameObject gameObject, ItemName weapon, ItemDatabase db)
    {
        if (gameObject != null) Destroy(gameObject);
        gameObject = CreateItem(weapon, db);
    }
    public GameObject CreateItem(ItemName weapon, ItemDatabase db)
    {

        //Spawn the item from the pool
        var go = new GameObject(weapon.ToString(),
            typeof(MeshRenderer),
            typeof(MeshFilter));

        go.GetComponent<MeshFilter>().mesh = db[weapon].mesh;

        go.GetComponent<MeshRenderer>().materials = db[weapon].materials;

        foreach (var p in db[weapon].properties)
        {
            p.CreatePlayerObject(go);
        }

        return go;
        //go.transform.SetParent(handBoneTransform, false);
    }


    private void Update()
    {
        //Only lock objects to anchors if they exist
        if (heldWeapon != null)
        {
            if (swordSheathed)
                sheathedHoldPoint.Anchor(heldWeapon.transform);
            else
                weaponHoldPoint.Anchor(heldWeapon.transform);
        }
        if (heldSidearm != null) sideArmHoldPoint.Anchor(heldSidearm.transform);
    }
}
