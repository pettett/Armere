using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Game/Quest", order = 0)]
public class Quest : ScriptableObject
{

    public enum QuestType
    {
        Deliver,
        TalkTo
    }

    public string title;

    [System.Serializable]
    public class QuestStage
    {

        [TextArea]
        public string description;
        public QuestType type;
        public NPCName receiver;
        public ItemName item;
        public int count = 1;
    }
    public QuestStage[] stages;
}