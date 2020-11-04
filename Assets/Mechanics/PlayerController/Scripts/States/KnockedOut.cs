using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{

    [Serializable]
    public class KnockedOut : MovementState
    {
        public float knockoutTime = 4f;
        public override string StateName => "Knocked Out";

        public override void Start()
        {
            canBeTargeted = false;

            c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = true;
            c.StartCoroutine(WaitForRespawn());
        }

        public IEnumerator WaitForRespawn()
        {
            yield return new WaitForSeconds(knockoutTime);
            WakeUp();
        }

        public void WakeUp()
        {
            //transform.position = LevelController.respawnPoint.position;
            //transform.rotation = LevelController.respawnPoint.rotation;
            c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = false;



            //go back to the spawn point
            ChangeToState<Walking>();
        }

        //place this in end to make sure it always returns camera control even if state is externally changed
        public override void End()
        {

        }
    }
}