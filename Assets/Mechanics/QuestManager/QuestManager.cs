using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager singleton;

    [System.Serializable]
    public class QuestStatus
    {
        public Quest quest;
        public int stage;
    }
    public List<QuestStatus> quests;
    public List<QuestStatus> completedQuests;

    private void Awake()
    {
        singleton = this;
        quests = new List<QuestStatus>();
        completedQuests = new List<QuestStatus>();
    }
    public static bool TryGetQuest(string questName, out Quest q)
    {
        for (int i = 0; i < singleton.quests.Count; i++)
        {
            if (singleton.quests[i].quest.name == questName)
            {
                q = singleton.quests[i].quest;
                return true;
            }
        }
        q = default(Quest);
        return false;
    }
    public static bool TryGetCompletedQuest(string questName, out Quest q)
    {
        for (int i = 0; i < singleton.completedQuests.Count; i++)
        {
            if (singleton.completedQuests[i].quest.name == questName)
            {
                q = singleton.completedQuests[i].quest;
                return true;
            }
        }
        q = default(Quest);
        return false;
    }

    public static void AddQuest(Quest q)
    {
        singleton.quests.Add(new QuestStatus() { quest = q });
    }

    public static void ProgressQuest(string questName)
    {
        for (int i = 0; i < singleton.quests.Count; i++)
        {
            if (singleton.quests[i].quest.name == questName)
            {
                InventoryController.TakeItems(
                    singleton.quests[i].quest.stages[singleton.quests[i].stage].item,
                    singleton.quests[i].quest.stages[singleton.quests[i].stage].count);

                singleton.quests[i].stage++;
                if (singleton.quests[i].stage == singleton.quests[i].quest.stages.Length)
                {
                    //quest is complete
                    CompleteQuest(i);
                }
            }
        }
    }
    public static void CompleteQuest(int index)
    {
        singleton.completedQuests.Add(singleton.quests[index]);
        singleton.quests.RemoveAt(index);
    }
}
