using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
using Yarn.Unity;

[System.Serializable]
public class QuestStatus
{
	public Quest quest => QuestManager.singleton.qdb.quests[questIndex];
	public int questIndex;
	[SerializeField] int _stage = -1;
	public int stage { get => _stage; }


	public QuestStatus(int questIndex)
	{
		this.questIndex = questIndex;
		QuestManager.ProgressQuest(this);
	}
	public QuestStatus(in GameDataReader reader)
	{
		questIndex = reader.ReadInt();
		_stage = reader.ReadInt();
	}

	public void WriteQuestStatus(in GameDataWriter writer)
	{
		writer.Write(questIndex);
		writer.Write(stage);
	}

	public void UpdateTriggerCount(int newAmount)
	{
		//If trigger requirements forfilled
		if (quest.stages[_stage].questTrigger.comparision switch
		{
			Quest.CountComparision.Equals => (newAmount == quest.stages[_stage].questTrigger.requiredTriggerCount),
			Quest.CountComparision.Greater => (newAmount > quest.stages[_stage].questTrigger.requiredTriggerCount),
			Quest.CountComparision.Less => (newAmount < quest.stages[_stage].questTrigger.requiredTriggerCount),
			_ => false,
		})
		{
			QuestManager.ProgressQuest(this);
		}
	}

	public void MoveToNextStage()
	{

		if (_stage >= 0 && _stage < quest.stages.Length)
		{
			//Remove reference from old stage
			if (quest.stages[_stage].type == Quest.QuestType.AwaitTriggerCount)
			{
				quest.stages[_stage].questTrigger.eventChannel.OnEventRaised -= UpdateTriggerCount;
			}
		}

		_stage++;

		Debug.Log($"Moving to stage {_stage} of {quest.title}");

		if (_stage >= 0 && _stage < quest.stages.Length)
		{
			if (quest.stages[_stage].type == Quest.QuestType.AwaitTriggerCount)
			{
				Debug.Log($"Subscribing quest to {quest.stages[_stage].questTrigger.eventChannel.name}");
				quest.stages[_stage].questTrigger.eventChannel.OnEventRaised += UpdateTriggerCount;
			}
		}
	}
}


[System.Serializable]
public class QuestBook
{
	public List<QuestStatus> quests = new List<QuestStatus>();
	public List<QuestStatus> completedQuests = new List<QuestStatus>();
}
[CreateAssetMenu(menuName = "Game/Quest Book")]
public class QuestManager : SaveableSO
{
	public static QuestManager singleton;
	public QuestDatabase qdb;


	public QuestBook questBook;

	public static List<QuestStatus> quests => singleton.questBook.quests;
	public static List<QuestStatus> completedQuests => singleton.questBook.completedQuests;

	public delegate void QuestEvent(Quest quest);

	public event QuestEvent onQuestProgress;
	public event QuestEvent onQuestComplete;

	Dictionary<string, uint> questTriggers = new Dictionary<string, uint>();

	public ItemAddedEventChannelSO onPlayerInventoryItemAdded;


	private void OnEnable()
	{
		if (singleton == null)
		{
			singleton = this;
		}

	}
	private void OnDisable()
	{
		if (singleton == this)
		{
			singleton = null;
		}

		onPlayerInventoryItemAdded.onItemAddedEvent -= OnInventoryItemAdded;
	}


	public override void LoadBlank()
	{
		questBook = new QuestBook();
	}

	public override void SaveBin(in GameDataWriter writer)
	{
		writer.Write(questBook.quests.Count);
		writer.Write(questBook.completedQuests.Count);
		//Debug.Log($"Saving {questBook.quests.Count} quests, {questBook.completedQuests.Count} compelted");
		for (int i = 0; i < questBook.quests.Count; i++)
		{
			questBook.quests[i].WriteQuestStatus(writer);
		}
		for (int i = 0; i < questBook.completedQuests.Count; i++)
		{
			questBook.completedQuests[i].WriteQuestStatus(writer);
		}
	}

	public override void LoadBin(in GameDataReader reader)
	{
		questBook = new QuestBook();
		//Read the number of quests in each list
		int quests = reader.ReadInt();
		questBook.quests = new List<QuestStatus>(quests);
		int completed = reader.ReadInt();
		questBook.completedQuests = new List<QuestStatus>(completed);


		//Debug.Log($"Loading {quests} quests, {completed} compelted");
		//Read all the data for the quests
		for (int i = 0; i < quests; i++)
		{
			questBook.quests.Add(new QuestStatus(reader));
		}
		for (int i = 0; i < completed; i++)
		{
			questBook.completedQuests.Add(new QuestStatus(reader));
		}
	}

	private void Start()
	{
		//Add listener for new item event
		onPlayerInventoryItemAdded.onItemAddedEvent += OnInventoryItemAdded;

		DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(new QuestStageYarnAddon());
		DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(new QuestStatusYarnAddon());
	}




	public static bool TryGetQuest(string questName, out QuestStatus q)
	{
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].quest.name == questName)
			{
				q = quests[i];
				return true;
			}
		}
		q = default;
		return false;
	}
	public static bool TryGetCompletedQuest(string questName, out QuestStatus q)
	{
		for (int i = 0; i < completedQuests.Count; i++)
		{
			if (completedQuests[i].quest.name == questName)
			{
				q = completedQuests[i];
				return true;
			}
		}
		q = default;
		return false;
	}
	public static void AddQuest(string name)
	{
		//Try to find the quest
		for (int i = 0; i < singleton.qdb.quests.Length; i++)
		{
			if (singleton.qdb.quests[i].name == name)
			{
				AddQuest(i);
				return;//When found return
			}
		}

		throw new System.ArgumentException("No quest with that name in database", name);
	}

	public static void AddQuest(int questIndex)
	{
		quests.Add(new QuestStatus(questIndex));

	}

	public void OnInventoryItemAdded(ItemStackBase stack, ItemType type, int index, bool hiddenAddition)
	{
		//test if any quests are listening for this event
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].quest.stages[quests[i].stage].type == Quest.QuestType.Acquire && //if is listening for any item
				quests[i].quest.stages[quests[i].stage].item == stack.item.itemName &&  //and is listening for this item
				InventoryController.singleton.ItemCount(stack.item.itemName) >= quests[i].quest.stages[quests[i].stage].count //And the player now has at least this many items
			)
			{
				ProgressQuest(quests[i]);
			}
		}
	}

	public static void ForfillDeliverQuest(string questName)
	{
		for (int i = 0; i < quests.Count; i++)
			if (quests[i].quest.name == questName)
			{
				if (quests[i].quest.stages[quests[i].stage].type != Quest.QuestType.Deliver)
					throw new System.ArgumentException("Quest is not in delivory stage");
				InventoryController.singleton.TakeItem(
					quests[i].quest.stages[quests[i].stage].item,
					quests[i].quest.stages[quests[i].stage].count);
				ProgressQuest(quests[i]);
				return;
			}
	}
	public static void ForfillTalkToQuest(string questName, NPCName talkingNPC)
	{
		for (int i = 0; i < quests.Count; i++)
			if (quests[i].quest.name == questName)
			{
				var stage = quests[i].quest.stages[quests[i].stage];
				if (stage.type != Quest.QuestType.TalkTo)
					throw new System.ArgumentException("Quest is not in talk to stage");
				else if (stage.receiver != talkingNPC)
					throw new System.ArgumentException("Trying to progress quest with incorrect NPC");
				ProgressQuest(quests[i]);
				return;
			}
		Debug.LogError("Trying to progress quest that is not in quest book");
	}





	public static void ProgressQuest(QuestStatus questStatus)
	{
		questStatus.MoveToNextStage();

		if (questStatus.stage == questStatus.quest.stages.Length)
		{
			//quest is complete
			CompleteQuest(questStatus);
		}
		else
		{
			singleton.onQuestProgress?.Invoke(questStatus.quest);

			//Test to see if the conditions for this new stage have already been met
			Quest.QuestStage stage = questStatus.quest.stages[questStatus.stage];
			if (stage.type == Quest.QuestType.Complete)
			{
				for (int i = 0; i < completedQuests.Count; i++)
				{
					if (completedQuests[i].quest.name == stage.quest)
					{
						//This quest is already completed, progress
						ProgressQuest(questStatus);
						return;
					}
				}
			}

		}
	}


	public static void CompleteQuest(QuestStatus questStatus)
	{
		Debug.Log($"Completing Quest {questStatus.quest.name}");

		completedQuests.Add(questStatus);

		singleton.onQuestComplete?.Invoke(questStatus.quest);

		for (int i = 0; i < quests.Count; i++)
		{
			//Test if another quest was waiting for this quest to be completed
			if (quests[i] != questStatus && quests[i].quest.stages[quests[i].stage].type == Quest.QuestType.Complete &&
				quests[i].quest.stages[quests[i].stage].quest == questStatus.quest.name
			)
			{
				//Completed this quest and progressing this next quest
				ProgressQuest(questStatus);
				//Do not break as multiple quests may be waiting for this one
			}
		}

		quests.Remove(questStatus);
	}





}