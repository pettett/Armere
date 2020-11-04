using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using System.Threading.Tasks;

[System.Serializable]
public class EquipmentSet<T> : IEnumerable<T>
{
    public T melee;
    public T sidearm;
    public T bow;

    public EquipmentSet(T melee, T sidearm, T bow)
    {
        this.melee = melee;
        this.sidearm = sidearm;
        this.bow = bow;
    }

    public T this[ItemType t]
    {
        get
        {
            switch (t)
            {
                case ItemType.Melee: return melee;
                case ItemType.SideArm: return sidearm;
                case ItemType.Bow: return bow;
                default: throw new System.Exception("No such type in set");
            }
        }
        set
        {
            switch (t)
            {
                case ItemType.Melee: melee = value; return;
                case ItemType.SideArm: sidearm = value; return;
                case ItemType.Bow: bow = value; return;
                default: throw new System.Exception("No such type in set");
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        yield return melee;
        yield return sidearm;
        yield return bow;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}


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
            t.SetParent(anchorTrans, false);
            t.localScale = Vector3.one * 100;
            t.localPosition = posOffset;
            t.localRotation = rotOffset;
        }
    }




    [System.Serializable]
    public class HoldableObject
    {
        public HoldPoint holdPoint;
        public HoldPoint sheathedPoint;
        bool _sheathed = true;
        public bool sheathed
        {
            get => _sheathed;
            set
            {
                _sheathed = value;
                //Anchor only needs updating when sheath changes
                Anchor();
            }
        }
        [HideInInspector] public WorldObject worldObject;
        HoldableItemData holdable;
        public void Init(Animator a)
        {
            holdPoint.Init(a);
            sheathedPoint.Init(a);
        }
        public void Anchor()
        {
            if (worldObject != null)
                if (sheathed)
                    sheathedPoint.Anchor(worldObject.transform);
                else
                    holdPoint.Anchor(worldObject.transform);
        }

        public async void SetHeld(HoldableItemData holdable)
        {
            this.holdable = holdable;
            if (worldObject != null) Destroy(worldObject);
            worldObject = await WorldObjectSpawner.SpawnWorldObjectAsync(holdable.holdableWorldObjectData, Vector3.zero, Quaternion.identity, default);
        }

        public void RemoveHeld()
        {
            if (worldObject != null)
                WorldObjectSpawner.DestroyWorldObject(worldObject);
        }

        public void OnClank(AudioSource source)
        {
            if (holdable != null && holdable.clankSet != null && holdable.clankSet.Valid())
            {
                if (Random.Range(0f, 1f) > holdable.clankProbability)
                {
                    source.PlayOneShot(holdable.clankSet.SelectClip());
                }
            }
        }
    }



    public AudioSource source;
    // public HoldableObject weapon;
    // public HoldableObject bow;
    // public HoldableObject sidearm;

    public EquipmentSet<HoldableObject> holdables;


    Animator animator;

    AnimationController animationController;



    public IEnumerator DrawItem(ItemType type, AnimationTransitionSet transitionSet)
    {
        if (type == ItemType.Melee)
        {
            animationController.TriggerTransition(transitionSet.drawSword);
            animator.SetBool("Holding Sword", true);
        }
        animationController.TriggerTransition(transitionSet.swordWalking);

        yield return new WaitForSeconds(0.1f);
        holdables[type].sheathed = false;
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator SheathItem(ItemType type, AnimationTransitionSet transitionSet)
    {
        if (type == ItemType.Melee)
        {
            animationController.TriggerTransition(transitionSet.sheathSword);
            animator.SetBool("Holding Sword", false);
        }

        animationController.TriggerTransition(transitionSet.freeMovement);

        yield return new WaitForSeconds(0.2f);

        holdables[type].sheathed = true;
    }


    private void Start()
    {
        animator = GetComponent<Animator>();
        animationController = GetComponent<AnimationController>();

        foreach (HoldableObject h in holdables)
            h.Init(animator);

    }

    public void OnClank()
    {
        //Called by animator
        foreach (HoldableObject h in holdables)
            h.OnClank(source);
    }
    public void FootDown()
    {
        OnClank();
    }
}
