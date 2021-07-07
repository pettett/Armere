using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RotaryHeart;
using Armere.Inventory;
using UnityEngine.Assertions;
using Yarn.Unity;
using Yarn;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NPC Template", menuName = "Game/NPCs/NPC Template", order = 0)]
public class NPCTemplate : AIStateTemplate
{
	public AssetReferenceT<YarnProgram> dialogue;
	public NPCManager manager;
	public QuestManager questManager;

	public VariableStorage.DefaultVariable[] defaultValues;

	public BuyMenuItem[] buyMenuItems;

	public ClothesVariation[] clothes;

	public Minigame[] minigames;

	public enum RoutineActivity { None = 0, Stand, Sleep }
	public enum RoutineAnimation { StandingIdle = 0, SittingIdle = 1, MaleSittingIdle = 2 }

	[System.Serializable]
	public class RoutineStage : IComparable<RoutineStage>
	{
		public float endTime;
		public RoutineActivity activity;
		public RoutineAnimation animation;
		public string location;
		public string conversationStartNode = "Start";

		public int CompareTo(RoutineStage compareStage) => endTime.CompareTo(compareStage.endTime);

	}


	public int GetRoutineIndex()
	{
		int d = routines.Length - 1;//Default routine is the last one

		for (int r = 0; r < routines.Length - 1; r++)
		{
			for (int i = 0; i < questManager.completedQuests.Count; i++)
			{
				if (questManager.completedQuests[i].quest == routines[r].activateOnQuestComplete)
				{
					//Return the first routine that fits the current situation
					return r;
				}
				else if (routines[r].activateOnQuestComplete == null)
				{
					//blank routine quests closer to end are better
					d = r;
				}
			}
		}

		return d; //Default routine is the last one

	}

	public override AIState StartState(AIMachine c)
	{
		return new NPCRoutine(c, this);
	}

	[System.Serializable]
	public class Routine
	{
		public Quest activateOnQuestComplete = null;
		public RoutineStage[] stages = new RoutineStage[1];


		///<summary>Get the current routine index that should be active at this time - use when time is not increasing linearly</summary>
		public int GetRoutineStageIndex(float time)
		{
			int routineStage = -1;
			for (int i = 0; i < stages.Length; i++)
			{
				//Go through every stage to find the current one
				if (time < stages[i].endTime)
				{
					routineStage = i;
					break;
				}
			}
			//If no stage ends after hour, loop around to the first stage
			if (routineStage == -1) routineStage = 0;

			return routineStage;
		}

	}

	public Routine[] routines = new Routine[1];
}

public class NPCRoutine : AIState<NPCTemplate>, IDialogue, IVariableAddon
{
	string IVariableAddon.prefix => "$NPC_";
	public int currentRoutineStage;

	public readonly AIDialogue d;
	public NPCManager.NPCData data;

	public int RoutineIndex { get => data.routineIndex; set => data.routineIndex = value; }

	public NPCTemplate.Routine CurrentRoutine => t.routines[RoutineIndex];
	public string StartNode => CurrentRoutine.stages[currentRoutineStage].conversationStartNode;
	public Transform transform => c.transform;
	AssetReferenceT<YarnProgram> IDialogue.Dialogue => t.dialogue;

	readonly AIAmbientThought thought;

	//Conversation currentConv; 
	public static readonly Dictionary<string, NPCRoutine> activeNPCs = new Dictionary<string, NPCRoutine>();

	Yarn.Value IVariableAddon.this[string name]
	{
		get
		{
			if (data.variables.TryGetValue(name, out Value value))
				return value;
			else
				throw new System.ArgumentException($"NPC {d.npcName} does not have variable {value}");
		}
		set => data.variables[name] = value;
	}
	IEnumerator RotatePlayerTowardsNPC(Transform playerTransform)
	{
		var dir = (transform.position - playerTransform.position);
		dir.y = 0;
		Quaternion desiredRot = Quaternion.LookRotation(dir);


		while (Quaternion.Angle(desiredRot, playerTransform.rotation) > 1f)
		{
			playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, desiredRot, Time.deltaTime * 800);
			yield return null;
		}
	}




	public IEnumerator TurnToPlayer(Vector3 playerPosition)
	{
		var dir = playerPosition - transform.position;
		dir.y = 0;

		//Debug.Log($"{d.npcName} turning to player");

		Quaternion desiredRot = Quaternion.LookRotation(dir);
		while (Quaternion.Angle(desiredRot, transform.rotation) > 1f)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, Time.deltaTime * 800);
			yield return null;
		}

	}


	public void StartSpeaking(Transform player)
	{
		c.StopAllCoroutines();
		//Point the player towards the currently speaking npc
		c.StartCoroutine(RotatePlayerTowardsNPC(player));

		c.StartCoroutine(TurnToPlayer(player.position));
	}

	public void StopSpeaking()
	{
		c.StopAllCoroutines();
	}

	readonly NPCSpawn spawn;

	public NPCRoutine(AIMachine machine, NPCTemplate t) : base(machine, t)
	{
		Assert.IsNotNull(t);
		Assert.IsNotNull(t.routines);
		Assert.IsNotNull(t.manager);
		Assert.IsNotNull(TimeDayController.singleton);

		spawn = c.spawner as NPCSpawn;

		Assert.IsNotNull(spawn);

		d = c.GetComponent<AIDialogue>();

		d.npcName = t.name;
		thought = c.GetComponent<AIAmbientThought>();


		if (!t.manager.data.ContainsKey(d.npcName))
		{
			//Only add this data if the NPC has not existed in the save before
			data = new NPCManager.NPCData(t);
			t.manager.data[d.npcName] = data;
		}
		else
		{
			data = t.manager.data[d.npcName];
		}

		for (int i = 0; i < t.clothes.Length; i++)
		{
			Assert.IsNotNull(t.clothes[i].clothes, $"Clothes on {t.name} null");
			//Apply all the clothes in the template
			c.meshController.SetClothing(t.clothes[i]);
		}

		//Setup starting point for routine - instant so they start in the proper place

		ChangeRoutineStage(t.routines[RoutineIndex].GetRoutineStageIndex(TimeDayController.singleton.hour), true);



		d.target = this;
		d.dialogueAddon = this;

		activeNPCs[d.npcName] = this;

		t.questManager.onQuestComplete += OnQuestComplete;

		//Copy the buy inventory
		d.buyInventory = new BuyMenuItem[t.buyMenuItems.Length];
		System.Array.Copy(t.buyMenuItems, d.buyInventory, d.buyInventory.Length);


		d.minigames = t.minigames;

		t.manager.data[d.npcName].npcInstance = c.transform;

		d.walkingPoints = spawn.walkingPoints;
		d.conversationGroupOverride = spawn.conversationGroupTargetsOverride;
		d.focusPoints = spawn.focusPoints;

		d.onInteract += Interact;
	}


	public override void End()
	{

		d.onInteract -= Interact;
		t.questManager.onQuestComplete -= OnQuestComplete;
	}

	public void OnQuestComplete(Quest quest)
	{
		//Update all of the indexes
		RoutineIndex = t.GetRoutineIndex();
	}
	public override void Update()
	{
		if (c.inited)
		{
			//Test if we need to move to the next routine stage
			//Only check for state change when before final end, as it will never change after that
			if (TimeDayController.singleton.hour < CurrentRoutine.stages[CurrentRoutine.stages.Length - 1].endTime)
			{
				if (TimeDayController.singleton.hour > CurrentRoutine.stages[currentRoutineStage].endTime)
				{
					ChangeRoutineStage(currentRoutineStage + 1);
				}
			}
			else if (currentRoutineStage == CurrentRoutine.stages.Length - 1 && currentRoutineStage != 0)
			{
				ChangeRoutineStage(0);
			}
		}
	}


	public void SetupCommands(DialogueRunner runner)
	{

	}

	public void RemoveCommands(DialogueRunner runner)
	{

	}

	public void Interact(IInteractor interactor)
	{
		//currentConv = (interactor as Player_CharacterController).ChangeToState<Conversation>();


		//Probably quicker to overrite true with true then find the value and
		//check if it is true then find it again to set it
		t.manager.data[d.npcName].spokenTo = true;

	}


	public void ActivateStandRoutine(bool instant)
	{
		Transform target = d.GetTransform(spawn.walkingPoints, CurrentRoutine.stages[currentRoutineStage].location);
		if (target != null)
		{
			if (instant)
			{
				c.transform.SetPositionAndRotation(target.position, target.rotation);
			}
			else
			{
				//Rotate to target rotation on finish walking
				c.GoToPosition(target.position, () =>
				{

					c.transform.rotation = target.rotation;
				});
			}
		}
		else
		{
			throw new System.Exception($"Desired routine location {CurrentRoutine.stages[currentRoutineStage].location} not within walking points array");
		}
	}

	public void ChangeRoutineStage(int newStage, bool instant = false)
	{
		currentRoutineStage = newStage;
		thought.ambientThoughtText.text = CurrentRoutine.stages[currentRoutineStage].activity.ToString();

		//Apply routine animation
		c.animationController.anim.SetInteger("idle_state", (int)CurrentRoutine.stages[currentRoutineStage].animation);

		switch (CurrentRoutine.stages[currentRoutineStage].activity)
		{
			case NPCTemplate.RoutineActivity.Stand:
				ActivateStandRoutine(instant);
				break;
		}
	}

	public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
	{
		return t.manager.data[d.npcName].variables.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}