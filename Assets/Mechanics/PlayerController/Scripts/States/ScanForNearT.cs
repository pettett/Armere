using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;


namespace Armere.PlayerController
{
    [System.Serializable]
    //Uses the TypeGroup static class in gamestructure
    public class ScanForNear<T> : MovementState where T : IScanable
    {
        public override string StateName => "Scan";
        [NonSerialized] public List<T> nearObjects = new List<T>();
        public float scanDistance = 3;
        public Vector3 scanCenterOffset;
        public bool updateEveryFrame = true;
        float _sqrScanDistance;
        public override void Start()
        {
            scanCenterOffset = Vector3.up * c.walkingHeight;
            _sqrScanDistance = scanDistance * scanDistance;
        }
        public override void Update()
        {
            if (updateEveryFrame) Scan();
        }

        public void Scan()
        {
            nearObjects.Clear();
            //static generic class holds all instances of the class
            for (int i = 0; i < TypeGroup<T>.allObjects.Count; i++)
            {
                //This may be speeded up by multithreading
                if (TypeGroup<T>.allObjects[i].enabled && (TypeGroup<T>.allObjects[i].transform.TransformPoint(TypeGroup<T>.allObjects[i].offset) - transform.position - scanCenterOffset).sqrMagnitude < _sqrScanDistance)
                {
                    nearObjects.Add(TypeGroup<T>.allObjects[i]);
                }
            }
        }

        public override void OnDrawGizmos()
        {
            //Draw all the objects found within radius units
            if (updateEveryFrame)
                for (int i = 0; i < nearObjects.Count; i++)
                {
                    Gizmos.DrawWireSphere(nearObjects[i].transform.TransformPoint(nearObjects[i].offset), 0.5f);
                }
        }
    }
}