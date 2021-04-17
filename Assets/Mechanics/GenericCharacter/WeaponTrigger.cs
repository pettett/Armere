using UnityEngine;
using UnityEngine.AddressableAssets;
using Armere.Inventory;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(SpawnableBody))]
public class WeaponTrigger : MonoBehaviour
{
	public MeleeWeaponItemData weaponItem;
	[System.NonSerialized] public GameObject controller;
	[System.NonSerialized] public Collider trigger;
	AsyncOperationHandle<GameObject> hitSparkEffect;
	public event System.Action<AttackResult> onWeaponHit;
	public TrailRenderer weaponTrail;
	bool _enableTrigger = false;
	public bool inited { get; private set; }

	public float scale = 1;

	public bool enableTrigger
	{
		get => _enableTrigger;
		set
		{
			if (weaponTrail != null)
				weaponTrail.emitting = value;
			trigger.enabled = _enableTrigger = value;
		}
	}

	public void Init(AssetReferenceGameObject hitSparkEffectReference)
	{
		inited = true;
		if (hitSparkEffectReference != null)
			Spawner.LoadAsset(hitSparkEffectReference, (x) =>
				{
					hitSparkEffect = x;
				});
	}
	private void Start()
	{
		trigger = GetComponent<Collider>();
		if (weaponTrail != null)
			weaponTrail.emitting = false;
	}

	private void OnDestroy()
	{
		Spawner.ReleaseAsset(hitSparkEffect);
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
			if (hitSparkEffect.Result != null)
				Destroy(Instantiate(hitSparkEffect.Result, hitPosition, Quaternion.identity), 1);

			if (other.TryGetComponent<IAttackable>(out var attackable))
			{
				AttackResult attackResult = attackable.Attack(
					weaponItem.attackFlags, weaponItem.damage, controller, hitPosition);
				//Attack the object, sending the result of the attack to the event listeners
				onWeaponHit?.Invoke(attackResult);
			}
		}
	}

	private void Update()
	{
		if (enableTrigger)
		{

			(var grassBounds, var yRot) = GetDestructionBounds();

			GrassController.singleton?.DestroyBladesInBounds(grassBounds, yRot);
		}
	}
	public System.ValueTuple<Bounds, float> GetDestructionBounds()
	{
		if (!TryGetComponent<MeshFilter>(out MeshFilter filter))
		{
			filter = GetComponentInChildren<MeshFilter>();
		}

		var grassBounds = filter.sharedMesh.bounds;
		grassBounds.center = transform.position + transform.forward * 0.5f;
		grassBounds.size = new Vector3(grassBounds.size.x * 2, 5, grassBounds.size.z * 3);
		float yRot = transform.eulerAngles.y * Mathf.Deg2Rad;

		return (grassBounds, yRot);
	}

	private void OnDrawGizmos()
	{

		(var grassBounds, var yRot) = GetDestructionBounds();

		Matrix4x4 mat = Matrix4x4.TRS(grassBounds.center, Quaternion.Euler(0, yRot, 0), grassBounds.size);
		Gizmos.matrix = mat;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

	}
}
