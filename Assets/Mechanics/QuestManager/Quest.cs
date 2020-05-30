using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Game/Quest", order = 0)]
public class Quest : ScriptableObject
{

    public enum QuestType
    {
        Deliver,
        TalkTo,
        Damage,
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
        public int count = 1;
        public float damage = 20;
    }


    public QuestStage[] stages;

}