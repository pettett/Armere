using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Armere.Inventory;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
[RequireComponent(typeof(AIMachine))]
[RequireComponent(typeof(NavMeshAgent), typeof(CharacterMeshController))]
public class AIHumanoid : Character
{

	public enum SightMode { View, Range }

	public TMPro.TMP_Text debugText;

	[System.NonSerialized] public NavMeshAgent agent;
	[System.NonSerialized] public Transform lookingAtTarget;
	[System.NonSerialized] public Ragdoller ragdoller;
	[System.NonSerialized] public CharacterMeshController meshController;

	public AIStateTemplate defaultState => machine.defaultState;




	public event System.Action<AIHumanoid> onPlayerDetected;

	public void OnPlayerDetected()
	{
		onPlayerDetected?.Invoke(this);
	}
	[System.NonSerialized] public Spawner spawner;

	[System.NonSerialized] new public Collider collider;
	public override Bounds bounds => collider.bounds;

	public override Vector3 velocity => agent.velocity;

	[System.NonSerialized] public Plane[] viewPlanes = new Plane[6];
	[Header("Weapons")]
	public BoundsFloatEventChannelSO destroyGrassInBoundsEventChannel;

	public AIMachine machine;

	// Start is called before the first frame update
	public override void Start()
	{
		base.Start();
		agent = GetComponent<NavMeshAgent>();
		ragdoller = GetComponent<Ragdoller>();
		meshController = GetComponent<CharacterMeshController>();
		collider = GetComponent<Collider>();

		Assert.IsNotNull(agent);
		Assert.IsNotNull(ragdoller);
		Assert.IsNotNull(collider);
		Assert.IsNotNull(machine);

		ragdoller.RagdollEnabled = false;
	}

	public void Spawn(Spawner spawner)
	{
		this.spawner = spawner;
	}

	public IEnumerator DrawItem(ItemType type)
	{
		yield return weaponGraphics.DrawItem(type, transitionSet);
	}

	public IEnumerator SheathItem(ItemType type)
	{
		yield return weaponGraphics.SheathItem(type, transitionSet);
	}


	public IEnumerator GoToPositionRoutine(Vector3 position)
	{
		agent.SetDestination(position);
		yield return new WaitUntil(() => gameObject == null || !agent.pathPending && agent.remainingDistance < agent.stoppingDistance * 2 + 0.01f);
	}
	public void GoToPosition(Vector3 position, System.Action onComplete)
	{
		agent.SetDestination(position);
		StartCoroutine(WaitForAgent(onComplete));
	}
	public void GoToPosition(Vector3 position)
	{
		agent.SetDestination(position);
	}


	public IEnumerator PickupItemRoutine(InteractableItem item, System.Action<bool> onItemPickupEnd = null)
	{
		//Put world space item into inventory
		GoToPosition(item.transform.position);
		while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance * 2 + 0.01f)
		{
			yield return null;
			if (item == null)
			{
				//item destroyed or taken
				onItemPickupEnd?.Invoke(false); // Fail
				yield break;
			}
		}
		Debug.Log("reached dest");

		//now here
		if (inventory.TryAddItem(item.item, item.count, true))
		{
			Destroy(item.gameObject);
			onItemPickupEnd?.Invoke(true); // success
		}

		onItemPickupEnd?.Invoke(false); // Fail
	}

	public AnimationTransitionSet transitionSet;
	public AIWaypointGroup waypointGroup;
	public IEnumerator GoToWaypoint(int index) => GoToTransform(waypointGroup[index]);

	public int GetClosestWaypoint()
	{
		//Pick the closest waypoint by non -pathed distance
		if (waypointGroup == null || waypointGroup.Length == 0) return -1;

		int waypoint = 0;
		for (int i = 1; i < waypointGroup.Length; i++)
		{
			if ((transform.position - waypointGroup[waypoint].position).sqrMagnitude > (transform.position - waypointGroup[i].position).sqrMagnitude)
				waypoint = i;
		}
		return waypoint;
	}

	protected IEnumerator GoToTransform(Transform t)
	{
		yield return GoToPositionRoutine(t.position);
		yield return RotateTo(t.rotation, agent.angularSpeed);
	}

	public override void Knockout(float time)
	{
		Debug.LogWarning("Cannot be knocked out", gameObject);
	}

	public IEnumerator RotateTo(Quaternion rotation, float angularSpeed)
	{
		float t = 0;
		Quaternion start = transform.rotation;

		float time = Quaternion.Angle(start, rotation) / angularSpeed;

		while (t < 1)
		{
			yield return null;
			transform.rotation = Quaternion.Slerp(start, rotation, t);
			t += Time.deltaTime / time;
		}
		transform.rotation = rotation;
	}

	public IEnumerator SwingSword()
	{
		MeleeWeaponItemData heldWeapon = inventory.SelectedMeleeWeapon;

		agent.isStopped = true; //Stop the player moving
								//swing the sword

		//This is easier. Animation graphs suck
		animationController.TriggerTransition(transitionSet.swingSword);

		WeaponTrigger trigger = null;
		void ClearUp()
		{
			animationController.onSwingStart -= AddTrigger;
			animationController.onSwingEnd -= RemoveTrigger;

		}

		void AddTrigger()
		{
			//Add collider and trigger logic to the blade object
			trigger = weaponGraphics.holdables.melee.gameObject.GetComponent<WeaponTrigger>();

			trigger.enableTrigger = true;
			trigger.destroyGrassInBounds = destroyGrassInBoundsEventChannel;

			if (!trigger.inited)
			{
				trigger.Init(heldWeapon.hitSparkEffect);
				trigger.weaponItem = heldWeapon;
				trigger.controller = gameObject;
			}
		}

		void RemoveTrigger()
		{
			//Clean up the trigger detection of the sword

			trigger.enableTrigger = false;
			ClearUp();

		}

		//Triggered by animations
		animationController.onSwingStart += AddTrigger;
		animationController.onSwingEnd += RemoveTrigger;

		yield return new WaitForSeconds(1);
	}


	public static IEnumerator WaitForAgent(NavMeshAgent agent, System.Action onComplete)
	{
		yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < agent.stoppingDistance * 2 + 0.01f);
		onComplete?.Invoke();
	}

	IEnumerator WaitForAgent(System.Action onComplete)
	{
		yield return WaitForAgent(agent, onComplete);
	}




	public void LookAtPlayer(Vector3 playerPos)
	{
		animationController.anim.SetLookAtPosition(playerPos);
		animationController.anim.SetLookAtWeight(1, 0, 1, 1, 0.2f);


		Vector3 flatDir = Vector3.Scale(transform.position, new Vector3(1, 0, 1)) - Vector3.Scale(playerPos, new Vector3(1, 0, 1));
		float angle = Vector3.Angle(transform.forward, flatDir);
		//If above angle threshold
		if (angle > 20)
		{

		}

	}
	public void LookAway()
	{
		animationController.anim.SetLookAtWeight(0);
	}


	public AsyncOperationHandle<GameObject> SetHeldMelee(HoldableItemData weapon)
	{
		return weaponGraphics.holdables.melee.SetHeld(weapon);
	}
	public AsyncOperationHandle<GameObject> SetHeldBow(HoldableItemData weapon)
	{
		return weaponGraphics.holdables.bow.SetHeld(weapon);
	}



	public AIState currentState;

}
