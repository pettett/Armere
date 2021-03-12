using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RotaryHeart;
using Armere.Inventory;
using Yarn.Unity;
using Yarn;

[CreateAssetMenu(fileName = "NPC Template", menuName = "Game/NPC Template", order = 0)]
public class NPCTemplate : AIStateTemplate
{
	public YarnProgram dialogue;

	public Yarn.Unity.InMemoryVariableStorage.DefaultVariable[] defaultValues;

	public BuyMenuItem[] buyMenuItems;

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

		for (int r = 0; r < routines.Length - 1; r++)
		{
			for (int i = 0; i < QuestManager.completedQuestsSingleton.Count; i++)
			{
				if (QuestManager.completedQuestsSingleton[i].quest.name == routines[r].activateOnQuestComplete)
				{
					//Return the first routine that fits the current situation
					return r;
				}
			}
		}

		return routines.Length - 1; //Default routine is the last one

	}

	public override AIState StartState(AIHumanoid c)
	{
		return new NPCRoutine(c, this);
	}

	[System.Serializable]
	public class Routine
	{
		public string activateOnQuestComplete = string.Empty;
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

public class NPCRoutine : AIState<NPCTemplate>, IVariableAddon, IDialogue
{
	string IVariableAddon.prefix => "$NPC_";
	public int currentRoutineStage;
	new NPC c => (NPC)base.c;

	public AIDialogue d;

	public int RoutineIndex { get => NPCManager.singleton.data[d.npcName].routineIndex; set => NPCManager.singleton.data[d.npcName].routineIndex = value; }

	public NPCTemplate.Routine CurrentRoutine => t.routines[RoutineIndex];
	public string StartNode => CurrentRoutine.stages[currentRoutineStage].conversationStartNode;
	public Transform transform => c.transform;
	public YarnProgram Dialogue => t.dialogue;
	Transform playerTransform;

	Value IVariableAddon.this[string name]
	{
		//Im sure there are many reasons why this is terrible, but yarn variables are not serializeable so cannot be saved
		get => NPCManager.singleton.data[d.npcName].variables[name];
		set => NPCManager.singleton.data[d.npcName].variables[name] = value;
	}

	public NPCRoutine(AIHumanoid c, NPCTemplate t) : base(c, t)
	{
		//Setup starting point for routine - instant so they start in the proper place
		ChangeRoutineStage(t.routines[RoutineIndex].GetRoutineStageIndex(TimeDayController.singleton.hour), true);
		d = c.GetComponent<AIDialogue>();
		d.target = this;
		d.dialogueAddon = this;

		QuestManager.singleton.onQuestComplete += OnQuestComplete;

		if (!NPCManager.singleton.data.ContainsKey(d.npcName))
		{
			//Only add this data if the NPC has not existed in the save before
			NPCManager.singleton.data[d.npcName] = new NPCManager.NPCData(t);
		}

		//Copy the buy inventory
		d.buyInventory = new BuyMenuItem[t.buyMenuItems.Length];
		System.Array.Copy(t.buyMenuItems, d.buyInventory, d.buyInventory.Length);


		NPCManager.singleton.data[d.npcName].npcInstance = c.transform;

		d.walkingPoints = this.c.spawn.walkingPoints;
		d.conversationGroupOverride = this.c.spawn.conversationGroupTargetsOverride;
		d.focusPoints = this.c.spawn.focusPoints;

		d.onInteract += Interact;
	}


	public override void End()
	{

		d.onInteract -= Interact;
		QuestManager.singleton.onQuestComplete -= OnQuestComplete;
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
			else if (currentRoutineStage == CurrentRoutine.stages.Length - 1)
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
		playerTransform = interactor.transform;



		//Probably quicker to overrite true with true then find the value and
		//check if it is true then find it again to set it
		NPCManager.singleton.data[d.npcName].spokenTo = true;


		c.StartCoroutine(c.TurnToPlayer(playerTransform.transform.position));
	}


	public void ActivateStandRoutine(bool instant)
	{
		Transform target = d.GetTransform(c.spawn.walkingPoints, CurrentRoutine.stages[currentRoutineStage].location);
		if (target != null)
		{
			if (instant)
			{
				c.transform.SetPositionAndRotation(target.position, target.rotation);
			}
			else
			{
				//Rotate to target rotation on finish walking
				c.GoToPosition(target.position, () => c.transform.rotation = target.rotation);
			}
		}
		else
		{
			throw new System.Exception(string.Format("Desired routine location {0} not within walking points array", CurrentRoutine.stages[currentRoutineStage].location));
		}
	}

	public void ChangeRoutineStage(int newStage, bool instant = false)
	{
		currentRoutineStage = newStage;
		c.ambientThoughtText.text = CurrentRoutine.stages[currentRoutineStage].activity.ToString();

		//Apply routine animation
		c.animator.SetInteger("idle_state", (int)CurrentRoutine.stages[currentRoutineStage].animation);

		switch (CurrentRoutine.stages[currentRoutineStage].activity)
		{
			case NPCTemplate.RoutineActivity.Stand:
				ActivateStandRoutine(instant);
				break;
		}
	}

}