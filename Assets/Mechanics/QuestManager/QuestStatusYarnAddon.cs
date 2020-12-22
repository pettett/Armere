using UnityEngine;
using Yarn;
using Yarn.Unity;

public class QuestStatusYarnAddon : IVariableAddon
{
    const string questStatusPrefix = "$Quest_";

    public string prefix => questStatusPrefix;

    public Value this[string name]
    {
        get
        {
            //it is a quest
            string quest = name.Substring(questStatusPrefix.Length);

            if (QuestManager.TryGetQuest(quest, out var q))

                //there is a quest with this name, return it's current state
                return new Value("Active");
            else if (QuestManager.TryGetCompletedQuest(quest, out var qw))

                //there is a quest with this name, return it's current state
                return new Value("Completed");
            else
                return new Value("Inactive");
        }

        set => throw new System.NotImplementedException("Cannot set status of quest");
    }

}