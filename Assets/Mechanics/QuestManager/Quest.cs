using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Game/Quest", order = 0)]
public class Quest : ScriptableObject
{

    public enum QuestType
    {
        Deliver,
        Acquire,
        TalkTo,
        Damage,
        Complete,
        Kill
    }

    public string title;


    [System.Serializable]
    public class QuestStage
    {
        [TextArea]
        [SerializeField] public string description;
        public QuestType type;
        public NPCName receiver;
        public ItemName item;
        public uint count = 1; //Cannot take negative amount of items
        public float damage = 20;
        public string quest;
    }


    public QuestStage[] stages;

}