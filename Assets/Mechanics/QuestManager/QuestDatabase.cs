using UnityEngine;

[CreateAssetMenu(fileName = "Quest Database", menuName = "Game/Quests/Quest Database", order = 0)]
public class QuestDatabase : ScriptableObject
{
    public Quest[] quests;
}