using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PlayerController
{
    
    [Serializable]
    public class AnimatorVariables
    {
        public AnimatorVariable surfing = new AnimatorVariable("IsSurfing");
        public AnimatorVariable vertical = new AnimatorVariable("InputVertical");
        public AnimatorVariable horizontal = new AnimatorVariable("InputHorizontal");
        public AnimatorVariable walkingSpeed = new AnimatorVariable("WalkingSpeed");
        public AnimatorVariable isGrounded = new AnimatorVariable("IsGrounded");
        public AnimatorVariable verticalVelocity = new AnimatorVariable("VerticalVelocity");
        public AnimatorVariable groundDistance = new AnimatorVariable("GroundDistance");
        public void UpdateIDs()
        {
            surfing.UpdateID();
            vertical.UpdateID();
            horizontal.UpdateID();
            walkingSpeed.UpdateID();
            isGrounded.UpdateID();
            isGrounded.UpdateID();
            verticalVelocity.UpdateID();
            groundDistance.UpdateID();
        }
    }
    [Serializable]
    public class AnimatorVariable
    {
        public string name;
        [HideInInspector] public int id;
        public AnimatorVariable(string name)
        {
            this.name = name;
        }
        public void UpdateID()
        {
            id = Animator.StringToHash(name);
        }
    }
    [Flags]
    public enum MovementModifiers
    {
        None = 0,
        Sprinting = 1,
        Crouching = 2
    }

    [Serializable]
    public abstract class ParallelState : MovementState
    {

    }

    [Serializable]
    public abstract class MovementState : State
    {
        public bool updateWhilePaused = false;
        public bool canBeTargeted = true;
        public void ChangeToState<T>() where T : MovementState, new() => c.ChangeToState<T>();

        public Transform transform => c.transform;
        public GameObject gameObject => c.gameObject;

        public Animator animator => c.animator;
        [NonSerialized] protected Player_CharacterController c;


        public void Init(Player_CharacterController characterController)
        {
            c = characterController;
        }

        public abstract string StateName { get; }


        public virtual void Animate(AnimatorVariables vars) { }
        public virtual void OnAnimatorIK(int layerIndex) { }
        public virtual void OnCollideGround(RaycastHit hit) { }
        public virtual void OnCollideCliff(RaycastHit hit) { }
        public virtual void OnJump(float state) { }
        public virtual void OnAttack(float state) { }
        public virtual void OnAltAttack(float state) { }
        public virtual void OnSprint(float state) { }
        public virtual void OnInteract(float state) { }
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }
        public virtual void OnGroundedChange() { }
        public virtual void OnSelectWeapon(int index) { }
        public virtual void OnCustomAction(InputAction.CallbackContext action) { }
        protected void print(string format, params object[] args) => Debug.LogFormat(format, args);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiresParallelState : Attribute
    {
        public Type state { get; private set; }
        public RequiresParallelState(Type state)
        {
            if (!state.IsSubclassOf(typeof(ParallelState)))
                throw new Exception("Must use a parallel State");
            this.state = state;
        }
    }

}