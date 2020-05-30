using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerController
{

    [Serializable]
    public class Dead : MovementState
    {
        public float respawnTime = 4f;
        public override string StateName => "Dead";

        public override void Start()
        {
            canBeTargeted = false;
            c.cameraController.EnableControl();
            c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = true;
            c.GetComponent<AnimationController>().thirdPerson = true;
            c.StartCoroutine(WaitForRespawn());
        }

        public IEnumerator WaitForRespawn()
        {
            yield return new WaitForSeconds(respawnTime);
            Respawn();
        }

        public void Respawn()
        {
            //transform.position = LevelController.respawnPoint.position;
            //transform.rotation = LevelController.respawnPoint.rotation;
            c.gameObject.GetComponent<Ragdoller>().RagdollEnabled = false;
            c.GetComponent<AnimationController>().thirdPerson = false;
            c.health?.Respawn();
            //go back to the spawn point
            ChangeToState<Walking>();
        }

        //place this in end to make sure it always returns camera control even if state is externally changed
        public override void End()
        {
            c.cameraController.EnableControl();
        }
    }
}