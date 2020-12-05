using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{

    [Serializable]
    public class Dead : MovementState
    {
        public override string StateName => "Dead";

        public override void Start()
        {
            canBeTargeted = false;
            c.cameraController.EnableControl();
            c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = true;
        }
    }
}