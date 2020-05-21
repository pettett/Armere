using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
public class WeaponGraphicsController : MonoBehaviour
{
    public Vector3 weaponLookSpot;
    public GameObject weapon;
    public Transform weaponHolder;
    public Transform firstPersonHolder;
    public Transform rightHandBone;

    // Update is called once per frame
    public bool weaponLookAt;




    public VisualEffect[] fireEffects;

    Vector3 weaponPosition;

    Transform grab1;
    Transform grab2;

    Animator anim;

    [HideInInspector]
    public Animator weaponAnimator;

    public bool localPlayer = false;

    public string firstPersonLayer;


    ///<summary>
    ///Used to make the weapon attached to hand in ragdoll
    ///</summary>
    public void PlaceWeaponInHand()
    {
        weaponPosition = weapon.transform.localPosition;
        weapon.transform.SetParent(rightHandBone);
        weapon.transform.localPosition = grab1.localPosition;
        weapon.transform.rotation = Quaternion.identity;
    }
    ///<summary>
    ///Bring the weapon out of ragdoll mode
    ///</summary>
    public void PlaceWeaponInSpace()
    {
        weapon.transform.SetParent(weaponHolder);
        weapon.transform.localPosition = weaponPosition;
        weapon.transform.rotation = Quaternion.identity;
    }

    Quaternion fpWeaponRotation = Quaternion.identity;

    void Update()
    {
        // if (weapon != null)
        // {
        //     if (!localPlayer)
        //     {
        //         if (weaponLookAt)
        //         {
        //             weapon.transform.LookAt(weaponLookSpot);
        //             weapon.transform.rotation = weapon.transform.rotation * weaponProfile.weaponLookOffset;
        //         }
        //         else
        //         {
        //             weapon.transform.localRotation = weaponProfile.weaponLookOffset;
        //         }
        //     }
        //     else
        //     {
        //         fpWeaponRotation = Quaternion.Slerp(fpWeaponRotation, weapon.transform.rotation, Time.deltaTime * 75);

        //         //apply a drag effect to the weapon            
        //         //weapon.transform.localPosition = weaponProfile.firstPersonOffset;

        //         weapon.transform.position = firstPersonHolder.transform.position + fpWeaponRotation * weaponProfile.firstPersonOffset;
        //     }

        // }

    }

    private void OnAnimatorIK()
    {
        // if (weaponProfile.lockToBone && !localPlayer)
        //     //offset to position based of should position
        //     weapon.transform.position = anim.GetBoneTransform(weaponProfile.bones).transform.position + transform.TransformDirection(weaponProfile.boneOffset);
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Awake()
    {
        firstPersonHolder = Camera.main.transform;
        weaponHolder = weaponHolder ?? transform.GetChild(2);

        anim = GetComponent<Animator>();


    }


    public void RemoveWeapon()
    {
        Destroy(weapon);
    }

    public void DrawWeapon()
    {
        // RemoveWeapon();

        // weaponProfile = weaponObject;


        // weapon = Instantiate(weaponProfile.prefab, localPlayer ? firstPersonHolder : weaponHolder);
        // weaponAnimator = weapon.GetComponent<Animator>();
        // Transform effect = weapon.transform.Find("Fire Effect");

        // grab1 = weapon.transform.Find("Grab Point 1");
        // grab2 = weapon.transform.Find("Grab Point 2");

        // if (effect != null && effect.TryGetComponent<VisualEffect>(out VisualEffect vfx))
        // {
        //     fireEffects = new VisualEffect[] { vfx };
        // }
        // if (!localPlayer)
        // {
        //     //external players should see this in the hands of the model


        //     if (TryGetComponent<AnimationController>(out AnimationController ac))
        //     {
        //         ac.holdPoints[0].gripPoint = grab1;
        //         ac.holdPoints[1].gripPoint = grab2;
        //     }
        // }
        // else
        // {
        //     //local player should see first person view

        //     //weapon.transform.rotation = Quaternion.identity;

        //     int layer = LayerMask.NameToLayer(firstPersonLayer);
        //     //place all renderers on the first person layer
        //     foreach (var item in weapon.GetComponentsInChildren<Renderer>())
        //     {
        //         item.gameObject.layer = layer;
        //         item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //     }

        // }




    }
    public void SetVfxFloat(string name, float f)
    {
        foreach (var fireEffect in fireEffects)
            fireEffect.SetFloat(name, f);
    }
    public void ActivateVFX()
    {
        foreach (var fireEffect in fireEffects)
        {
            fireEffect.Play();
        }

    }


    public void TriggerVFX(string eventName)
    {
        foreach (var fireEffect in fireEffects)
        {
            fireEffect.SendEvent(eventName);
        }

    }
    public void CancelVFX()
    {
        foreach (var fireEffect in fireEffects)
        {
            if (fireEffect != null)
            {
                fireEffect.Reinit();
                fireEffect.Stop();
            }

        }
    }
}
