using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using Armere.Inventory;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public struct EquipmentSet<T> : IEnumerable<T>
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
		[System.NonSerialized] public SpawnableBody gameObject;
		HoldableItemData holdable;
		public void Init(Animator a)
		{
			holdPoint.Init(a);
			sheathedPoint.Init(a);

		}
		public void Anchor()
		{
			if (gameObject != null)
				if (sheathed)
					sheathedPoint.Anchor(gameObject.transform);
				else
					holdPoint.Anchor(gameObject.transform);
		}

		public AsyncOperationHandle<GameObject> SetHeld(HoldableItemData holdable)
		{
			this.holdable = holdable;
			if (gameObject != null) gameObject.Destroy();

			Assert.IsTrue(holdable.holdableGameObject.RuntimeKeyIsValid(), "Reference to holdable is null");
			var handle = GameObjectSpawner.Spawn(holdable.holdableGameObject, Vector3.zero, Quaternion.identity, default);
			GameObjectSpawner.OnDone(handle,
			(handle) =>
			{
				gameObject = handle.Result.GetComponent<SpawnableBody>();
				Anchor();
			});
			return handle;
		}

		public void RemoveHeld()
		{
			if (gameObject != null)
				GameObjectSpawner.Despawn(gameObject);
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

	public CharacterMeshController characterMesh;

	public AudioSource source;
	// public HoldableObject weapon;
	// public HoldableObject bow;
	// public HoldableObject sidearm;

	public EquipmentSet<HoldableObject> holdables;


	Animator animator;

	AnimationController animationController;

	public Transform bowLockingTransform;

	public IEnumerator DrawItem(ItemType type, AnimationTransitionSet transitionSet)
	{
		if (type == ItemType.Melee)
		{
			animationController.TriggerTransition(transitionSet.drawSword);
			animator.SetBool("Holding Sword", true);

			animationController.TriggerTransition(transitionSet.swordWalking);
		}
		else if (type == ItemType.Bow)
		{
			//IK the hand to the bow
			animationController.holdPoints.Add(new AnimationController.HoldPoint()
			{
				goal = AvatarIKGoal.RightHand,
				positionWeight = 1,
				rotationWeight = 1,
				gripPoint = holdables.bow.gameObject.GetComponent<Bow>().backHandAnchor
			});

			animationController.holdPoints.Add(new AnimationController.HoldPoint()
			{
				goal = AvatarIKGoal.LeftHand,
				positionWeight = 1,
				rotationWeight = 1,
				gripPoint = bowLockingTransform
			});

			animationController.TriggerTransition(transitionSet.strafingMovement);
		}

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
		else if (type == ItemType.Bow)
		{
			//Remove the string attachment
			animationController.holdPoints.RemoveAll(x => x.goal == AvatarIKGoal.RightHand || x.goal == AvatarIKGoal.LeftHand);
			holdables.bow.gameObject.GetComponent<Bow>().RemoveNotchedArrow();
		}

		animationController.TriggerTransition(transitionSet.freeMovement);

		yield return new WaitForSeconds(0.2f);

		holdables[type].sheathed = true;
	}


	private void Start()
	{
		animationController = GetComponent<AnimationController>();

		animator = GetComponentInChildren<Animator>();

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