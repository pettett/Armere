using UnityEngine;

public interface QuestTrigger
{
    string name { get; }
    uint triggerCount { get; }
}


[CreateAssetMenu(fileName = "Quest", menuName = "Game/Quest", order = 0)]
public class Quest : ScriptableObject
{

    public enum QuestType
    {
        Deliver, //Used to request items to an npc
        Acquire, //Used to request items to the player
        TalkTo, //Talk to an npc
        Damage, //
        Complete, //complete another quest
        Kill,
        AwaitTriggerCount, //Used to await a quest trigger specified in name
    }

    public string title;

    public enum CountComparision
    {
        Greater,
        Equals,
        Less,
    }
    [System.Serializable]
    public class QuestTriggerInfo : QuestTrigger
    {
        public string name;
        [System.NonSerialized] public uint currentTriggerCount;
        public uint requiredTriggerCount;
        public CountComparision comparision;

        string QuestTrigger.name => name;

        uint QuestTrigger.triggerCount => requiredTriggerCount;
    }

    [System.Serializable]
    public class QuestStage
    {
        [TextArea]
        [SerializeField] public string description;
        public QuestType type;
        [MyBox.ConditionalField("type", false, QuestType.Deliver, QuestType.TalkTo)] public NPCName receiver;
        [MyBox.ConditionalField("type", false, QuestType.Deliver, QuestType.Acquire)] public ItemName item;
        [MyBox.ConditionalField("type", false, QuestType.Deliver, QuestType.Acquire)] public uint count = 1; //Cannot take negative amount of items
        [MyBox.ConditionalField("type", false, QuestType.Deliver, QuestType.Damage)] public float damage = 20;
        [MyBox.ConditionalField("type", false, QuestType.Complete)] public string quest;
        [MyBox.ConditionalField("type", false, QuestType.AwaitTriggerCount)] public QuestTriggerInfo questTrigger; //Used to connect to an arbitary trigger in the world
    }



    public QuestStage[] stages;

}