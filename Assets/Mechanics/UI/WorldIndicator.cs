using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldIndicator : MonoBehaviour
{
    Transform target = null;
    new public Camera camera;
    new RectTransform transform;
    [System.Serializable]
    public class StringEvent : UnityEvent<string>{}
    public StringEvent onIndicate;
    public UnityEvent onEndIndicate;
    Vector3 worldOffset;
    public void StartIndication(Transform target,string title,Vector3 worldOffset=default)
    {
        this.target = target;
        this.worldOffset = worldOffset;
        onIndicate?.Invoke(title);
        gameObject.SetActive(true);
    }
    public void EndIndication()
    {
        target = null;
        onEndIndicate?.Invoke();
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        transform = (RectTransform)base.transform;
        camera = Camera.main;
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            //position self on target
            Vector3 screenPos = camera.WorldToScreenPoint(target.position+worldOffset);
            transform.position = screenPos;
        }
    }
}
