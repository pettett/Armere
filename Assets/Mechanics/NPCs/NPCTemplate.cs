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


}

