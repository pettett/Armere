using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RotaryHeart;

[CreateAssetMenu(fileName = "NPC Template", menuName = "Game/NPC Template", order = 0)]
public class NPCTemplate : ScriptableObject
{
    public YarnProgram dialogue;
    public Quest[] quests;

    public Yarn.Unity.InMemoryVariableStorage.DefaultVariable[] defaultValues;

    public BuyMenuItem[] buyMenuItems;

    public enum RoutineActivity { None = 0, Stand, Sleep }
    public enum RoutineAnimation { StandingIdle = 0, SittingIdle = 1, MaleSittingIdle = 2 }
    [System.Serializable]
    public class RoutineStage : IComparable<RoutineStage>
    {
        public float endTime;
        public RoutineActivity activity;
        public RoutineAnimation animation;
        public string location;
        public string conversationStartNode = "Start";

        public int CompareTo(RoutineStage compareStage) => endTime.CompareTo(compareStage.endTime);

    }


    public RoutineStage[] routine = new RoutineStage[1];
}

