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
        public int stage;
    }
    public List<QuestStatus> quests;
    public List<QuestStatus> completedQuests;

    public delegate void QuestEvent(Quest quest);

    public event QuestEvent onQuestProgress;
    public event QuestEvent onQuestComplete;

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
        for (int i = 0; i < singleton.quests.Count; i++)
        {
            if (singleton.quests[i].quest.stages[singleton.quests[i].stage].type == Quest.QuestType.AwaitTriggerCount)
            {
                Quest.QuestTriggerInfo info = singleton.quests[i].quest.stages[singleton.quests[i].stage].questTrigger;
                if (info.name == trigger.name)
                {
                    //Try to complete this stage
                    info.currentTriggerCount = trigger.triggerCount;
                    switch (info.comparision)
                    {
                        case Quest.CountComparision.Equals:
                            if (info.currentTriggerCount == info.requiredTriggerCount)
                                ProgressQuest(i);
                            break;
                        case Quest.CountComparision.Greater:
                            if (info.currentTriggerCount > info.requiredTriggerCount)
                                ProgressQuest(i);
                            break;
                        case Quest.CountComparision.Less:
                            if (info.currentTriggerCount < info.requiredTriggerCount)
                                ProgressQuest(i);
                            break;
                    }
                }
            }
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
                singleton.quests.Add(new QuestStatus() { quest = q });
                return;//When found return
            }
        throw new System.ArgumentException("No quest with that name in database", name);
    }

    public static void AddQuest(Quest q)
    {
        singleton.quests.Add(new QuestStatus() { quest = q });
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
        print($"Progressing Quest {index}");

        singleton.quests[index].stage++;
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
