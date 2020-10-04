using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableTriggerZone : MonoBehaviour, QuestTrigger
{
    public string allowTag;
    public string questTriggerName;
    public uint bodiesInZone;

    string QuestTrigger.name => questTriggerName;
    public uint triggerCount => bodiesInZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.TryGetComponent<HoldableBody>(out var b) && b.holdableTriggerTag == allowTag)
        {
            bodiesInZone++;
            //Update any quests that care about this trigger
            QuestManager.UpdateTrigger(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && other.TryGetComponent<HoldableBody>(out var b) && b.holdableTriggerTag == allowTag)
        {
            bodiesInZone--;
            //Update any quests that care about this trigger
            QuestManager.UpdateTrigger(this);
        }
    }

}
