using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerBox : MonoBehaviour
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



}
