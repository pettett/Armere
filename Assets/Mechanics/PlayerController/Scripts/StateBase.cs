using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Armere.PlayerController
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

    [Serializable]
    public abstract class MovementState : State, ISaveable
    {
        public bool updateWhilePaused = false;
        public bool canBeTargeted = true;
        public void ChangeToState<T>() where T : MovementState, new() => c.ChangeToState<T>();

        public Transform transform => c.transform;
        public GameObject gameObject => c.gameObject;

        public Animator animator => c.animator;
        [NonSerialized] protected PlayerController c;


        public void Init(PlayerController characterController)
        {
            c = characterController;
        }

        public abstract string StateName { get; }
        public abstract char StateSymbol { get; }

        public virtual void Animate(AnimatorVariables vars) { }
        public virtual void OnAnimatorIK(int layerIndex) { }
        public virtual void OnJump(InputActionPhase phase) { }
        public virtual void OnAttack(InputActionPhase phase) { }
        public virtual void OnAltAttack(InputActionPhase phase) { }
        public virtual void OnSprint(InputActionPhase phase) { }
        public virtual void OnInteract(InputActionPhase phase) { }
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }
        public virtual void OnSelectWeapon(int index, InputActionPhase phase) { }
        public virtual void OnCustomAction(InputAction.CallbackContext action) { }
        protected void print(string format, params object[] args) => Debug.LogFormat(format, args);

        public virtual void SaveBin(GameDataWriter writer) { }
        public virtual void LoadBin(Version saveVersion, GameDataReader reader) { }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequiresParallelState : Attribute
    {
        public readonly System.Collections.ObjectModel.ReadOnlyCollection<Type> states;
        public RequiresParallelState(params Type[] states)
        {
            foreach (var state in states)
                if (!state.IsSubclassOf(typeof(MovementState)))
                    throw new Exception("Must use a parallel State");
            this.states = Array.AsReadOnly(states);
        }
    }

}