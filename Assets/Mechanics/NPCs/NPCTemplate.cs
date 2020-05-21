using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NPC Template", menuName = "Game/NPC Template", order = 0)]
public class NPCTemplate : ScriptableObject
{
    public NPCName NPCName;
    public YarnProgram dialogue;
    public Quest[] quests;

    [System.Serializable] public class Dict : RotaryHeart.Lib.SerializableDictionary.SerializableDictionaryBase<string, Vector3> { }
    public Dict focusPoints;
}

