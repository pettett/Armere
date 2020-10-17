using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager singleton;
    public QuestDatabase qdb;

    [System.Serializable]
    public class QuestStatus
    {
        public Quest quest;
        public int stage = -1;
        public uint currentTriggerCount;
    }
    public List<QuestStatus> quests;
    public List<QuestStatus> completedQuests;

    public delegate void QuestEvent(Quest quest);

    public event QuestEvent onQuestProgress;
    public event QuestEvent onQuestComplete;

    Dictionary<string, uint> questTriggers = new Dictionary<string, uint>();

    private void Awake()
    {
        singleton = this;
        quests = new List<QuestStatus>();
        completedQuests = new List<QuestStatus>();

    }
    private void Start()
    {
        //Add listener for new item event
        InventoryController.singleton.onItemAdded += OnInventoryItemAdded;
    }


    public static void UpdateTrigger(QuestTrigger trigger)
    {
        singleton.questTriggers[trigger.name] = trigger.triggerCount;

        for (int i = 0; i < singleton.quests.Count; i++)
        {
            if (singleton.quests[i].quest.stages[singleton.quests[i].stage].type == Quest.QuestType.AwaitTriggerCount)
            {
                Quest.QuestTriggerInfo triggerRequirements = singleton.quests[i].quest.stages[singleton.quests[i].stage].questTrigger;
                if (triggerRequirements.name == trigger.name)
                {
                    singleton.quests[i].currentTriggerCount = trigger.triggerCount;
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
        for (int i = 0; i < singleton.quests.Count; i++)
        {
            if (singleton.quests[i].quest.name == questName)
            {
                q = singleton.quests[i];
                return true;
            }
        }
        q = default;
        return false;
    }
    public static bool TryGetCompletedQuest(string questName, out QuestStatus q)
    {
        for (int i = 0; i < singleton.completedQuests.Count; i++)
        {
            if (singleton.completedQuests[i].quest.name == questName)
            {
                q = singleton.completedQuests[i];
                return true;
            }
        }
        q = default;
        return false;
    }
    public static void AddQuest(string name)
    {
        //Try to find the quest
        foreach (Quest q in singleton.qdb.quests)
            if (q.name == name)
            {
                AddQuest(q);
                return;//When found return
            }
        throw new System.ArgumentException("No quest with that name in database", name);
    }

    public static void AddQuest(Quest q)
    {
        singleton.quests.Add(new QuestStatus() { quest = q });
        ProgressQuest(singleton.quests.Count - 1);
    }

    public void OnInventoryItemAdded(ItemName newItem)
    {
        //test if any quests are listening for this event
        for (int i = 0; i < singleton.quests.Count; i++)
        {
            if (singleton.quests[i].quest.stages[singleton.quests[i].stage].type == Quest.QuestType.Acquire && //if is listening for any item
                singleton.quests[i].quest.stages[singleton.quests[i].stage].item == newItem &&  //and is listening for this item
                InventoryController.ItemCount(newItem) >= singleton.quests[i].quest.stages[singleton.quests[i].stage].count //And the player now has at least this many items
            )
            {
                ProgressQuest(i);
            }
        }
    }

    public static void ForfillDeliverQuest(string questName)
    {
        for (int i = 0; i < singleton.quests.Count; i++)
            if (singleton.quests[i].quest.name == questName)
            {
                if (singleton.quests[i].quest.stages[singleton.quests[i].stage].type != Quest.QuestType.Deliver)
                    throw new System.ArgumentException("Quest is not in deliver orstage");
                InventoryController.TakeItem(
                    singleton.quests[i].quest.stages[singleton.quests[i].stage].item,
                    singleton.quests[i].quest.stages[singleton.quests[i].stage].count);
                ProgressQuest(i);
                return;
            }
    }
    public static void ForfillTalkToQuest(string questName)
    {
        print(questName);
        for (int i = 0; i < singleton.quests.Count; i++)
            if (singleton.quests[i].quest.name == questName)
            {
                if (singleton.quests[i].quest.stages[singleton.quests[i].stage].type != Quest.QuestType.TalkTo)
                    throw new System.ArgumentException("Quest is not in talk to stage");
                ProgressQuest(i);
                return;
            }
    }





    public static void ProgressQuest(int index)
    {
        print($"Progressing Quest {singleton.quests[index].quest.name}");

        singleton.quests[index].stage++;
        singleton.quests[index].currentTriggerCount = 0;
        if (singleton.quests[index].stage == singleton.quests[index].quest.stages.Length)
        {
            //quest is complete
            CompleteQuest(index);
        }
        else
        {
            singleton.onQuestProgress?.Invoke(singleton.quests[index].quest);

            //Test to see if the conditions for this new stage have already been met
            Quest.QuestStage stage = singleton.quests[index].quest.stages[singleton.quests[index].stage];
            if (stage.type == Quest.QuestType.Complete)
            {
                for (int i = 0; i < singleton.completedQuests.Count; i++)
                {
                    if (singleton.completedQuests[i].quest.name == stage.quest)
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
                    singleton.quests[index].currentTriggerCount = current;
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

        singleton.completedQuests.Add(singleton.quests[index]);

        singleton.onQuestComplete?.Invoke(singleton.quests[index].quest);

        for (int i = 0; i < singleton.quests.Count; i++)
        {
            //Test if another quest was waiting for this quest to be completed
            if (i != index && singleton.quests[i].quest.stages[singleton.quests[i].stage].type == Quest.QuestType.Complete &&
                singleton.quests[i].quest.stages[singleton.quests[i].stage].quest == singleton.quests[index].quest.name
            )
            {
                //Completed this quest and progressing this next quest
                ProgressQuest(i);
                //Do not break as multiple quests may be waiting for this one
            }
        }

        singleton.quests.RemoveAt(index);
    }
}
