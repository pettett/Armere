using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
using Yarn.Unity;

[System.Serializable]
public class QuestStatus
{
	public Quest quest => QuestManager.singleton.qdb.quests[questIndex];
	public readonly int questIndex;
	public int stage = -1;
	public uint currentTriggerCount;

	public QuestStatus(int questIndex)
	{
		this.questIndex = questIndex;
	}
}


[System.Serializable]
public class QuestBook
{
	public List<QuestStatus> quests = new List<QuestStatus>();
	public List<QuestStatus> completedQuests = new List<QuestStatus>();
}

public class QuestManager : MonoSaveable
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

	private void Awake()
	{
		if (singleton == null)
			singleton = this;
	}

	public override void LoadBlank()
	{
		questBook = new QuestBook();
	}
	public void WriteQuestStatus(in GameDataWriter writer, QuestStatus status)
	{
		writer.Write(status.questIndex);
		writer.Write(status.stage);
		writer.Write(status.currentTriggerCount);
	}
	public QuestStatus ReadQuestStatus(in GameDataReader reader)
	{
		return new QuestStatus(reader.ReadInt())
		{
			stage = reader.ReadInt(),
			currentTriggerCount = reader.ReadUInt()
		};
	}
	public override void SaveBin(in GameDataWriter writer)
	{
		writer.Write(questBook.quests.Count);
		writer.Write(questBook.completedQuests.Count);
		for (int i = 0; i < questBook.quests.Count; i++)
		{
			WriteQuestStatus(writer, questBook.quests[i]);
		}
		for (int i = 0; i < questBook.completedQuests.Count; i++)
		{
			WriteQuestStatus(writer, questBook.completedQuests[i]);
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
		//Read all the data for the quests
		for (int i = 0; i < quests; i++)
		{
			questBook.quests.Add(ReadQuestStatus(reader));
		}
		for (int i = 0; i < completed; i++)
		{
			questBook.completedQuests.Add(ReadQuestStatus(reader));
		}
	}

	private void Start()
	{
		//Add listener for new item event
		onPlayerInventoryItemAdded.onItemAddedEvent += OnInventoryItemAdded;

		DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(new QuestStageYarnAddon());
		DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(new QuestStatusYarnAddon());
	}
	private void OnDestroy()
	{
		onPlayerInventoryItemAdded.onItemAddedEvent -= OnInventoryItemAdded;
	}

	public static void UpdateTrigger(QuestTrigger trigger)
	{
		singleton.questTriggers[trigger.name] = trigger.triggerCount;

		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].quest.stages[quests[i].stage].type == Quest.QuestType.AwaitTriggerCount)
			{
				Quest.QuestTriggerInfo triggerRequirements = quests[i].quest.stages[quests[i].stage].questTrigger;
				if (triggerRequirements.name == trigger.name)
				{
					quests[i].currentTriggerCount = trigger.triggerCount;
					//Try to complete this stage
					if (QuestTriggerForfilled(triggerRequirements, trigger.triggerCount))
					{
						ProgressQuest(i);
					}
				}
			}
		}
	}
	public static bool QuestTriggerForfilled(Quest.QuestTriggerInfo triggerRequirements, uint current)
	{
		switch (triggerRequirements.comparision)
		{
			case Quest.CountComparision.Equals:
				return (current == triggerRequirements.requiredTriggerCount);
			case Quest.CountComparision.Greater:
				return (current > triggerRequirements.requiredTriggerCount);
			case Quest.CountComparision.Less:
				return (current < triggerRequirements.requiredTriggerCount);
			default: return false;
		}
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

		ProgressQuest(quests.Count - 1);
	}

	public void OnInventoryItemAdded(ItemStackBase stack, ItemType type, int index, bool hiddenAddition)
	{
		//test if any quests are listening for this event
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].quest.stages[quests[i].stage].type == Quest.QuestType.Acquire && //if is listening for any item
				quests[i].quest.stages[quests[i].stage].item == stack.name &&  //and is listening for this item
				InventoryController.ItemCount(stack.name) >= quests[i].quest.stages[quests[i].stage].count //And the player now has at least this many items
			)
			{
				ProgressQuest(i);
			}
		}
	}

	public static void ForfillDeliverQuest(string questName)
	{
		for (int i = 0; i < quests.Count; i++)
			if (quests[i].quest.name == questName)
			{
				if (quests[i].quest.stages[quests[i].stage].type != Quest.QuestType.Deliver)
					throw new System.ArgumentException("Quest is not in deliver orstage");
				InventoryController.TakeItem(
					quests[i].quest.stages[quests[i].stage].item,
					quests[i].quest.stages[quests[i].stage].count);
				ProgressQuest(i);
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
				ProgressQuest(i);
				return;
			}
		Debug.LogError("Trying to progress quest that is not in quest book");
	}





	public static void ProgressQuest(int index)
	{
		print($"Progressing Quest {quests[index].quest.name}");

		quests[index].stage++;
		quests[index].currentTriggerCount = 0;
		if (quests[index].stage == quests[index].quest.stages.Length)
		{
			//quest is complete
			CompleteQuest(index);
		}
		else
		{
			singleton.onQuestProgress?.Invoke(quests[index].quest);

			//Test to see if the conditions for this new stage have already been met
			Quest.QuestStage stage = quests[index].quest.stages[quests[index].stage];
			if (stage.type == Quest.QuestType.Complete)
			{
				for (int i = 0; i < completedQuests.Count; i++)
				{
					if (completedQuests[i].quest.name == stage.quest)
					{
						//This quest is already completed, progress
						ProgressQuest(index);
						return;
					}
				}
			}
			else if (stage.type == Quest.QuestType.AwaitTriggerCount)
			{
				//Test if the trigger has already been forfilled
				Quest.QuestTriggerInfo triggerInfo = stage.questTrigger;
				if (singleton.questTriggers.TryGetValue(triggerInfo.name, out uint current))
				{
					quests[index].currentTriggerCount = current;
					if (QuestTriggerForfilled(triggerInfo, current))
					{
						print("Quest trigger already satisfied");
						ProgressQuest(index);
					}
				}
			}
		}
	}


	public static void CompleteQuest(int index)
	{
		print($"Completing Quest {index}");

		completedQuests.Add(quests[index]);

		singleton.onQuestComplete?.Invoke(quests[index].quest);

		for (int i = 0; i < quests.Count; i++)
		{
			//Test if another quest was waiting for this quest to be completed
			if (i != index && quests[i].quest.stages[quests[i].stage].type == Quest.QuestType.Complete &&
				quests[i].quest.stages[quests[i].stage].quest == quests[index].quest.name
			)
			{
				//Completed this quest and progressing this next quest
				ProgressQuest(i);
				//Do not break as multiple quests may be waiting for this one
			}
		}

		quests.RemoveAt(index);
	}





}