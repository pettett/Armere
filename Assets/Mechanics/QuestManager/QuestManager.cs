using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
using Yarn.Unity;
using UnityEngine.Events;

[System.Serializable]
public class QuestStatus
{
	public Quest quest => manager.qdb.quests[questIndex];
	public Quest.QuestStage questStage => quest.stages[_stage];
	public int questIndex;
	[SerializeField] int _stage = -1;
	public int stage { get => _stage; }
	QuestManager manager;

	public QuestStatus(QuestManager manager, int questIndex)
	{
		this.questIndex = questIndex;
		this.manager = manager;
		manager.ProgressQuest(this);
	}
	public QuestStatus(QuestManager manager, in GameDataReader reader)
	{
		questIndex = reader.ReadInt();
		_stage = reader.ReadInt();
		this.manager = manager;
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
			manager.ProgressQuest(this);
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



[CreateAssetMenu(menuName = "Game/Quests/Quest Book")]
public class QuestManager : SaveableSO
{
	public static QuestManager singleton;
	public QuestDatabase qdb;
	public InventoryController inventory;
	QuestStatus _selectedQuest = null;
	public QuestStatus selectedQuest
	{
		get => _selectedQuest;
		set
		{
			_selectedQuest = value;
			onSelectedQuestStatusUpdated?.Invoke(value);
		}
	}

	[System.NonSerialized] public List<QuestStatus> quests = new List<QuestStatus>();
	[System.NonSerialized] public List<QuestStatus> completedQuests = new List<QuestStatus>();

	public static List<QuestStatus> questsSingleton => singleton.quests;
	public static List<QuestStatus> completedQuestsSingleton => singleton.completedQuests;


	public event UnityAction<Quest> onQuestProgress;
	public event UnityAction<Quest> onQuestComplete;
	public event UnityAction<QuestStatus> onSelectedQuestStatusUpdated;

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
		quests = new List<QuestStatus>();
		completedQuests = new List<QuestStatus>();
	}

	public override void SaveBin(in GameDataWriter writer)
	{
		if (selectedQuest == null)
		{
			writer.Write(-1);
		}
		else
		{
			writer.Write(selectedQuest.questIndex);
		}
		writer.Write(quests.Count);
		writer.Write(completedQuests.Count);
		//Debug.Log($"Saving {questBook.quests.Count} quests, {questBook.completedQuests.Count} compelted");
		for (int i = 0; i < quests.Count; i++)
		{
			quests[i].WriteQuestStatus(writer);
		}
		for (int i = 0; i < completedQuests.Count; i++)
		{
			completedQuests[i].WriteQuestStatus(writer);
		}
	}

	public override void LoadBin(in GameDataReader reader)
	{
		int selectedQuestIndex = -1;
		if (reader.saveVersion > new Version(0, 0, 1))
		{
			selectedQuestIndex = reader.ReadInt();
		}

		//Read the number of quests in each list
		int questsCount = reader.ReadInt();
		quests = new List<QuestStatus>(questsCount);
		int completed = reader.ReadInt();
		completedQuests = new List<QuestStatus>(completed);


		//Debug.Log($"Loading {quests} quests, {completed} compelted");
		//Read all the data for the quests
		for (int i = 0; i < questsCount; i++)
		{
			quests.Add(new QuestStatus(this, reader));
		}
		for (int i = 0; i < completed; i++)
		{
			completedQuests.Add(new QuestStatus(this, reader));
		}


		//Call this after so that the selected quest actually indexes of a real value in the quests storage
		selectedQuest = quests.Find(x => x.questIndex == selectedQuestIndex);
	}

	private void Start()
	{
		//Add listener for new item event
		onPlayerInventoryItemAdded.onItemAddedEvent += OnInventoryItemAdded;

		DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(new QuestStageYarnAddon());
		DialogueInstances.singleton.inMemoryVariableStorage.addons.Add(new QuestStatusYarnAddon());
	}



	public bool TryGetQuest(string questName, out QuestStatus q)
	{
		for (int i = 0; i < questsSingleton.Count; i++)
		{
			if (questsSingleton[i].quest.name == questName)
			{
				q = questsSingleton[i];
				return true;
			}
		}
		q = default;
		return false;
	}
	public bool TryGetCompletedQuest(string questName, out QuestStatus q)
	{
		for (int i = 0; i < completedQuestsSingleton.Count; i++)
		{
			if (completedQuestsSingleton[i].quest.name == questName)
			{
				q = completedQuestsSingleton[i];
				return true;
			}
		}
		q = default;
		return false;
	}
	public void AddQuest(string name)
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

	public void AddQuest(int questIndex)
	{
		quests.Add(new QuestStatus(this, questIndex));

	}

	public void OnInventoryItemAdded(ItemStackBase stack, ItemType type, int index, bool hiddenAddition)
	{
		//test if any quests are listening for this event
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].quest.stages[quests[i].stage].type == Quest.QuestType.Acquire && //if is listening for any item
				quests[i].quest.stages[quests[i].stage].item == stack.item.itemName &&  //and is listening for this item
				inventory.ItemCount(stack.item.itemName) >= quests[i].quest.stages[quests[i].stage].count //And the player now has at least this many items
			)
			{
				ProgressQuest(quests[i]);
			}
		}
	}

	public void ForfillDeliverQuest(string questName)
	{
		for (int i = 0; i < quests.Count; i++)
			if (quests[i].quest.name == questName)
			{
				if (quests[i].quest.stages[quests[i].stage].type != Quest.QuestType.Deliver)
					throw new System.ArgumentException("Quest is not in delivory stage");
				inventory.TakeItem(
					quests[i].quest.stages[quests[i].stage].item,
					quests[i].quest.stages[quests[i].stage].count);
				ProgressQuest(quests[i]);
				return;
			}
	}
	public void ForfillTalkToQuest(string questName, string talkingNPC)
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





	public void ProgressQuest(QuestStatus questStatus)
	{
		questStatus.MoveToNextStage();


		if (questStatus.stage == questStatus.quest.stages.Length)
		{
			//quest is complete
			CompleteQuest(questStatus);
		}
		else
		{
			onQuestProgress?.Invoke(questStatus.quest);

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

		if (questStatus.questIndex == selectedQuest.questIndex)
		{
			onSelectedQuestStatusUpdated?.Invoke(selectedQuest);
		}
	}


	public void CompleteQuest(QuestStatus questStatus)
	{
		Debug.Log($"Completing Quest {questStatus.quest.name}");

		completedQuestsSingleton.Add(questStatus);

		singleton.onQuestComplete?.Invoke(questStatus.quest);

		for (int i = 0; i < questsSingleton.Count; i++)
		{
			//Test if another quest was waiting for this quest to be completed
			if (questsSingleton[i] != questStatus && questsSingleton[i].quest.stages[questsSingleton[i].stage].type == Quest.QuestType.Complete &&
				questsSingleton[i].quest.stages[questsSingleton[i].stage].quest == questStatus.quest.name
			)
			{
				//Completed this quest and progressing this next quest
				ProgressQuest(questStatus);
				//Do not break as multiple quests may be waiting for this one
			}
		}

		questsSingleton.Remove(questStatus);
	}





}