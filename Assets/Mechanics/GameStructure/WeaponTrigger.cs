using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IAttackable : IScanable
{
    AttackResult Attack(ItemName weapon, GameObject controller, Vector3 hitPosition);
}

[System.Flags]
public enum AttackResult
{
    None = 0b0000,
    Damaged = 0b0001,
    Blocked = 0b0010,
    Headshot = 0b0100,
    Killed = 0b1000
}


public class WeaponTrigger : MonoBehaviour
{
    public ItemName weaponItem;
    public GameObject controller;

    public Collider trigger;
    GameObject hitSparkEffect;
    public event System.Action<AttackResult> onWeaponHit;
    bool _enableTrigger = false;
    public bool inited { get; private set; }

    public bool enableTrigger
    {
        get => _enableTrigger;
        set
        {
            _enableTrigger = value;
            trigger.enabled = _enableTrigger;
        }
    }

    public async void Init(AssetReferenceGameObject hitSparkEffectReference)
    {

        if (hitSparkEffectReference != null)
            hitSparkEffect = await Addressables.LoadAssetAsync<GameObject>(hitSparkEffectReference).Task;
    }
    private void Start()
    {
        trigger = GetComponent<Collider>();
    }

    private void OnDestroy()
    {
        if (hitSparkEffect != null)
            Addressables.Release(hitSparkEffect);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Make sure we have not hit ourself
        if (enableTrigger && !other.isTrigger && other.gameObject != controller && !other.transform.IsChildOf(controller.transform))
        {

            Collider collider = GetComponent<Collider>();
            Ray collisionRay = new Ray(transform.position, transform.forward);

            //Calculate the hit position
            Vector3 hitPosition = collider.bounds.center;

            if (other.Raycast(collisionRay, out RaycastHit hit, Mathf.Infinity))
            {
                hitPosition = hit.point;

                //Apply a small force to the hit object
                if (other.attachedRigidbody != null)
                    other.attachedRigidbody.AddForceAtPosition(hit.normal * 20f, hit.point);
            }

            // Debug.Assert(Physics.ComputePenetration(
            //      collider, transform.position,
            //      transform.rotation,
            //      other, other.transform.position,
            //      other.transform.rotation, out Vector3 direction, out float distance), "Melee collided with object with no intersection", controller);

            // Debug.DrawLine(transform.position, transform.position + direction * distance, Color.red, 10);
            // Debug.Break();

            //Create a spark
            if (hitSparkEffect != null)
                Destroy(Instantiate(hitSparkEffect, hitPosition, Quaternion.identity), 1);

            if (other.TryGetComponent<IAttackable>(out var attackable))
            {
                AttackResult attackResult = attackable.Attack(weaponItem, controller, hitPosition);
                //Attack the object, sending the result of the attack to the event listeners
                onWeaponHit?.Invoke(attackResult);
            }
        }
    }

    private void Update()
    {
        if (enableTrigger)
        {
            Bounds grassBounds = GetComponent<MeshFilter>().sharedMesh.bounds;
            grassBounds.center = transform.position + transform.forward * 0.5f;
            grassBounds.size = new Vector3(grassBounds.size.x, 5, grassBounds.size.z);
            float yRot = transform.eulerAngles.y * Mathf.Deg2Rad;

            GrassController.singleton.DestroyBladesInBounds(grassBounds, yRot);
        }
    }

    private void OnDrawGizmos()
    {
        Bounds grassBounds = GetComponent<MeshFilter>().sharedMesh.bounds;
        grassBounds.center = transform.position + transform.forward * 0.5f;
        grassBounds.size = new Vector3(grassBounds.size.x, 1, grassBounds.size.z);
        float yRot = transform.eulerAngles.y;


        Matrix4x4 mat = Matrix4x4.TRS(grassBounds.center, Quaternion.Euler(0, yRot, 0), grassBounds.size);
        Gizmos.matrix = mat;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

    }
}
