using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RotaryHeart;

[CreateAssetMenu(fileName = "NPC Template", menuName = "Game/NPC Template", order = 0)]
public class NPCTemplate : ScriptableObject
{
    public YarnProgram dialogue;
    public Quest[] quests;

    public Yarn.Unity.InMemoryVariableStorage.DefaultVariable[] defaultValues;
    [System.Serializable]
    public class BuyMenuItem
    {
        public ItemName item;
        public uint count;
        public uint stock;
        public uint cost;
    }
    public BuyMenuItem[] buyMenuItems;

}

