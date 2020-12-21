using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RotaryHeart;

[CreateAssetMenu(fileName = "NPC Template", menuName = "Game/NPC Template", order = 0)]
public class NPCTemplate : ScriptableObject
{
    public YarnProgram dialogue;

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


    public int GetRoutineIndex()
    {

        for (int r = 0; r < routines.Length - 1; r++)
        {
            for (int i = 0; i < QuestManager.completedQuests.Count; i++)
            {
                if (QuestManager.completedQuests[i].quest.name == routines[r].activateOnQuestComplete)
                {
                    //Return the first routine that fits the current situation
                    return r;
                }
            }
        }

        return routines.Length - 1; //Default routine is the last one

    }


    [System.Serializable]
    public class Routine
    {
        public string activateOnQuestComplete = string.Empty;
        public RoutineStage[] stages = new RoutineStage[1];


        ///<summary>Get the current routine index that should be active at this time - use when time is not increasing linearly</summary>
        public int GetRoutineStageIndex(float time)
        {
            int routineStage = -1;
            for (int i = 0; i < stages.Length; i++)
            {
                //Go through every stage to find the current one
                if (time < stages[i].endTime)
                {
                    routineStage = i;
                    break;
                }
            }
            //If no stage ends after hour, loop around to the first stage
            if (routineStage == -1) routineStage = 0;

            return routineStage;
        }

    }

    public Routine[] routines = new Routine[1];
}

