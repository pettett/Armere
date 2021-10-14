using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Armere.Inventory;
using UnityEngine;
using UnityEngine.Assertions;
public enum Team
{
	Good = 0,
	Evil = 1,
	Neutral = 2,
}


[RequireComponent(typeof(SpawnableBody))]
public abstract class Character : ArmereBehaviour
{


	public enum BuffType
	{
		Burning,
		Frozen,
		Strength,
		Speed,
		Stealth,
	}

	public class Buff
	{
		public BuffType buff;
		public float remainingTime;
	}
	public static readonly Team[][] enemies = new Team[][]{
		new Team[]{ Team.Evil}, //Good
		new Team[]{ Team.Good}, //Evil
		new Team[]{}, //Neutral
	};

	public static readonly Dictionary<Team, List<Character>> teams = new Dictionary<Team, List<Character>>();

	public List<Buff> buffs = new List<Buff>();
	[NonSerialized] public GameObjectInventory inventoryHolder;
	public InventoryController inventory => inventoryHolder.inventory;
	public bool hasInventory => inventoryHolder != null;

	public OnBuffChangedEventChannel onBuffAdded;
	[MyBox.ButtonMethod]
	public void AddRandomBuff()
	{
		AddBuff(new Buff() { buff = (BuffType)UnityEngine.Random.Range(0, 5), remainingTime = UnityEngine.Random.Range(10, 400) });
	}
	public void AddBuff(Buff buff)
	{
		buffs.Add(buff);
		onBuffAdded?.RaiseEvent(buff);
	}
	protected virtual void Update()
	{
		for (int i = 0; i < buffs.Count; i++)
		{
			buffs[i].remainingTime -= Time.deltaTime;
		}
	}
	public Team team;
	public CharacterProfile profile;
	public bool SameTeamAs(Character other) => !enemies[(int)team].Contains(other.team);
	public Team[] Enemies() => enemies[(int)team];

	public abstract Bounds bounds { get; }
	public abstract Vector3 velocity { get; }

	public static Character playerCharacter;

	[System.NonSerialized] public WeaponGraphicsController weaponGraphics;
	[System.NonSerialized] public AnimationController animationController;
	[System.NonSerialized] public SpawnableBody spawnableBody;
	public bool inited => spawnableBody.inited;
	public Animator animator => animationController.anim;

	public abstract void Knockout(float time);



	public virtual void Start()
	{
		FindComponent(out animationController);
		TryGetComponent(out weaponGraphics);
		TryGetComponent(out inventoryHolder);
		AssertComponent(out spawnableBody);
	}
	public virtual void OnEnable()
	{
		if (!teams.ContainsKey(team))
		{
			teams[team] = new List<Character>(1);
		}
		teams[team].Add(this);
	}
	public virtual void OnDisable()
	{
		teams[team].Remove(this);
	}
	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody != null)
		{
			//Test which body impacted the other
			Vector3 otherVel = collision.relativeVelocity - velocity;

			if (otherVel.sqrMagnitude < 0.1)
			{
				return; //Other body not moving
			}
			float dot = Vector3.Dot(otherVel, velocity);
			if (dot < 0)
			{
				//moving in the same direction
				return;
			}

			float koTime = collision.relativeVelocity.magnitude * collision.rigidbody.mass - profile.m_minKnockoutForce;

			if (koTime > 0)
			{
				koTime = profile.m_knockoutTimePerMassPerSpeed.Evaluate(koTime);
				Debug.Log($"KO For {koTime}s");
				Knockout(koTime);
			}
		}
	}

}
