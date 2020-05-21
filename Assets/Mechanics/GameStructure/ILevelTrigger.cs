using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelTrigger
{
    string name { get; }
    CustomYieldInstruction WaitInstrunction();
}

