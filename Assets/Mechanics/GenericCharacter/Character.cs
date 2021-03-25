using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(WeaponGraphicsController), typeof(AnimationController))]
public abstract class Character : SpawnableBody
{
	public enum Team
	{
		Good = 0,
		Evil = 1,
		Neutral = 2,
	}



	public static Team[][] enemies = new Team[][]{
		new Team[]{ Team.Evil}, //Good
		new Team[]{ Team.Good}, //Evil
		new Team[0], //Neutral
	};

	public static readonly Dictionary<Team, List<Character>> teams = new Dictionary<Team, List<Character>>();

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

	public List<Buff> buffs = new List<Buff>();

	public OnBuffChangedEventChannel onBuffAdded;
	[MyBox.ButtonMethod]
	public void AddRandomBuff()
	{
		AddBuff(new Buff() { buff = (BuffType)Random.Range(0, 5), remainingTime = Random.Range(10, 400) });
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
	public bool SameTeamAs(Character other) => !enemies[(int)team].Contains(other.team);
	public Team[] Enemies() => enemies[(int)team];

	public abstract Bounds bounds { get; }

	public static Character playerCharacter;

	[System.NonSerialized] public WeaponGraphicsController weaponGraphics;
	[System.NonSerialized] public AnimationController animationController;

	public abstract void Knockout(float time);



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


}
