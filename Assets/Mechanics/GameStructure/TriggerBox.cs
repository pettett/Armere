using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerBox : MonoBehaviour, ILevelTrigger
{
    public bool onceOff;
    [System.Serializable]
    public class ColliderEvent : UnityEvent<Collider> { }
    public ColliderEvent onTriggerEnter;
    public bool fired = false;
    public string activatorTag = "Player";
    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != activatorTag) return;

        fired = true;
        onTriggerEnter.Invoke(other);
        if (onceOff)
            gameObject.SetActive(false);
    }
    public class WaitForTrigger : CustomYieldInstruction
    {
        readonly TriggerBox trigger;

        public WaitForTrigger(TriggerBox trigger)
        {
            this.trigger = trigger;
            trigger.fired = false;
        }

        public override bool keepWaiting => !trigger.fired;
    }

    public CustomYieldInstruction WaitInstrunction()
    {
        return new WaitForTrigger(this);
    }
}
