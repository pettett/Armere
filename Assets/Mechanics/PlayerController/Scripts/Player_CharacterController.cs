using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PlayerController
{
    [RequireComponent(typeof(Rigidbody))]
    public class Player_CharacterController : MonoBehaviour, IAITarget
    {


        public event Action<Player_CharacterController> onDeath;
        public event Action<Player_CharacterController> onRepawn;

        public event Action<InputAction.CallbackContext> onPlayerInput;

        [System.Serializable]
        public struct InputStatus
        {
            public Vector2 inputWalk;
            public Vector2 inputCamera;
            public bool sprinting;
            public bool crouching;
        }
        [System.Flags]
        public enum MovementModifiers
        {
            None = 0,
            Sprinting = 1,
            Crouching = 2
        }

        public MovementModifiers mod;

        public Transform cameraTransform;

        public CameraControlSettings playerCameraSettings;

        [HideInInspector] public bool controllingCamera = true;

        Camera playerCamera;
        [ReadOnly]
        public Vector2 cameraLookOffset = Vector2.zero;
        //used to change how the height of the camera will change for a short time
        public float cameraStepHeightOffset;
        public float cameraStepDistance;
        public float currentCameraStepTime = 1;
        //TODO - turn camera controls into a parallel state





        private MovementState currentState;

        //Parallel state list:

        //Camera Controller - NOT DONE
        //Weapons - DONE
        //Interaction with objects - NOT DONE

        private ParallelState[] parallelStates = new ParallelState[0];

        [Header("State Properties")]

        [SerializeField] Walking.WalkingProperties walkingProperties = new Walking.WalkingProperties();
        [SerializeField] Freefalling.FreefallingProperties freefallingProperties = new Freefalling.FreefallingProperties();
        [SerializeField] Climbing.ClimbingProperties climbingProperties = new Climbing.ClimbingProperties();
        [SerializeField] Shieldsurfing.ShieldsurfingProperties shieldsurfingProperties = new Shieldsurfing.ShieldsurfingProperties();

        public float jumpForce = 1000f;


        public float cliffScanningDistance = 0.31f;
        [Range(0, 90)]
        public float minAngleForCliff = 70;
        public Vector3 cliffTopScanOffset = Vector3.up;
        public Vector3 cliffScanOffset = Vector3.up * .3f;

        [Header("Ground detection")]

        public float maxGroundDistance = 0.1f;
        public float groundSphereRadius = 0.3f;
        public float groundScanningOffset = 1f; // moves raycast start point up to stop normal of 0 when origin touches hit point
        [Range(0, 90)] public float maxGroundAngle = 70;
        private float maxGroundDot = 0.3f;
        public bool onGround;


        [Header("Other")]

        public LayerMask groundLayerMask;
        public LayerMask shootingCollisionLayerMask;
        public Rigidbody rb;
        private new CapsuleCollider collider;

        [HideInInspector]
        public Animator animator;
        public TMPro.TextMeshProUGUI currentStateText;
        [SerializeField] public InputStatus input = new InputStatus(); //current player input




        public Health health;

        private float currentHeight = 0;
        //raycast hit cached
        private RaycastHit groundRaycastHit;
        private RaycastHit cliffRaycastHit;

        public WeaponGraphicsController weaponGraphicsController;

        public RaycastHit hit;
        private Vector3 rigidbodyAccelerationRequiredForce;//cache vector so it is not recreated every update
        public bool weaponGizmos = true;
        [Header("Enabled States")]
        public bool climbing;
        public bool surfing;


        public static Player_CharacterController activePlayerController;

        [HideInInspector]
        public AnimationController animationController;


        //set capacity to 1 as it is common for the player to be touching the ground in at least one point
        List<ContactPoint> allCPs = new List<ContactPoint>(1);

        DebugMenu.DebugEntry entry;

        void OnCollisionEnter(Collision col) => allCPs.AddRange(col.contacts);
        void OnCollisionStay(Collision col) => allCPs.AddRange(col.contacts);


        Collider IAITarget.collider => collider;

        public bool canBeTargeted => currentState != null && currentState.canBeTargeted;

        public Vector3 velocity => rb.velocity;
        int _engagementCount = 0;
        public int engagementCount { get => _engagementCount; set => _engagementCount = value; }

        public Dictionary<string, object> persistentStateData = new Dictionary<string, object>();

        // Start is called before the first frame update
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {

            activePlayerController = this;
            animationController = GetComponent<AnimationController>();
        }
        bool loadedStates = false;
        private void Start()
        {


            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            collider = GetComponent<CapsuleCollider>();
            if (TryGetComponent<Health>(out health))
                health.onDeath += OnDeath;

            weaponGraphicsController = GetComponent<WeaponGraphicsController>();
            if (cameraTransform == null)
            {
                playerCamera = Camera.main;
                cameraTransform = playerCamera.transform;
            }
            else
            {
                playerCamera = cameraTransform.GetComponent<Camera>();
            }

            maxGroundDot = Mathf.Cos(maxGroundAngle);
            entry = DebugMenu.CreateEntry("Player", "Current State: {0}", "");


            if (loadedStates)
            {
                currentState.Init(this);
                currentState.Start();
                for (int i = 0; i < parallelStates.Length; i++)
                {
                    parallelStates[i].Init(this);
                    parallelStates[i].Start();
                }
            }
            else
            {
                //start a fresh state
                ChangeToState<Walking>();
            }

            GetComponent<PlayerInput>().onActionTriggered += OnActionTriggered;

            GetComponent<Ragdoller>().RagdollEnabled = false;


        }

        private void OnDestroy()
        {

            currentState.End();
        }

        public void OnDeath(GameObject attacker, GameObject victim)
        {
            // The player has died.
            ChangeToState<Dead>();

        }

        public void UpdateModifier(bool enabled, MovementModifiers modifier)
        {
            if (enabled && !mod.HasFlag(modifier))
            {
                mod |= modifier;
            }
            else if (!enabled && mod.HasFlag(modifier))
            {
                mod &= ~modifier;
            }
        }

        public void OnActionTriggered(InputAction.CallbackContext action)
        {


            if (action.phase == InputActionPhase.Disabled || action.phase == InputActionPhase.Started)
                return;

            string selectWeaponPrefix = "SelectWeapon";

            string actionName = action.action.name;

            onPlayerInput?.Invoke(action);

            if (actionName.StartsWith(selectWeaponPrefix) && action.ReadValue<float>() == 1)
            {
                var buttonNumber = int.Parse(actionName.Remove(0, selectWeaponPrefix.Length));
                buttonNumber--;
                if (buttonNumber == -1) buttonNumber = 9;
                OnSelectWeapon(buttonNumber);
            }

            switch (actionName)
            {
                case "Walk":
                    input.inputWalk = action.ReadValue<Vector2>();
                    break;
                case "Look":
                    input.inputCamera = action.ReadValue<Vector2>();
                    break;
                case "Action":
                    currentState.OnInteract(action.ReadValue<float>());
                    for (int i = 0; i < parallelStates.Length; i++)
                        parallelStates[i].OnInteract(action.ReadValue<float>());
                    break;
                case "Crouch":
                    input.crouching = action.ReadValue<float>() == 1;
                    UpdateModifier(action.ReadValue<float>() == 1, MovementModifiers.Crouching);
                    break;
                case "Sprint":
                    UpdateModifier(action.ReadValue<float>() == 1, MovementModifiers.Sprinting);
                    currentState.OnSprint(action.ReadValue<float>());
                    for (int i = 0; i < parallelStates.Length; i++)
                        parallelStates[i].OnSprint(action.ReadValue<float>());
                    break;
                case "Jump":
                    currentState.OnJump(action.ReadValue<float>());
                    for (int i = 0; i < parallelStates.Length; i++)
                        parallelStates[i].OnJump(action.ReadValue<float>());
                    break;
                case "Attack":
                    currentState.OnAttack(action.ReadValue<float>());
                    for (int i = 0; i < parallelStates.Length; i++)
                        parallelStates[i].OnAttack(action.ReadValue<float>());
                    break;
                case "AltAttack":
                    currentState.OnAltAttack(action.ReadValue<float>());
                    for (int i = 0; i < parallelStates.Length; i++)
                        parallelStates[i].OnAltAttack(action.ReadValue<float>());
                    break;

                default:
                    currentState.OnCustomAction(action);
                    break;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            RaycastGround();
            RaycastCliff();
            currentState.Update();
            for (int i = 0; i < parallelStates.Length; i++)
                parallelStates[i].Update();
            currentState.Animate();

        }
        private void LateUpdate()
        {
            currentState.LateUpdate();
            for (int i = 0; i < parallelStates.Length; i++)
                parallelStates[i].LateUpdate();
        }

        void OnTriggerEnter(Collider other)
        {
            currentState.OnTriggerEnter(other);
            for (int i = 0; i < parallelStates.Length; i++)
                parallelStates[i].OnTriggerEnter(other);
        }
        void OnTriggerExit(Collider other)
        {
            currentState.OnTriggerExit(other);
            for (int i = 0; i < parallelStates.Length; i++)
                parallelStates[i].OnTriggerExit(other);
        }

        private void FixedUpdate()
        {
            currentState.FixedUpdate();
            for (int i = 0; i < parallelStates.Length; i++)
                parallelStates[i].FixedUpdate();

            allCPs.Clear();
        }



        private void AccelerateRigidbody(Vector3 targetVelocity, float maxAcceleration, float dt)
        {
            //scale required velocity by current speed
            rigidbodyAccelerationRequiredForce = targetVelocity - rb.velocity;
            rigidbodyAccelerationRequiredForce.y = 0;
            rigidbodyAccelerationRequiredForce = Vector3.ClampMagnitude(rigidbodyAccelerationRequiredForce, maxAcceleration * dt);
            //rotate the target based on the ground the player is standing on

            rb.AddForce(rigidbodyAccelerationRequiredForce, ForceMode.VelocityChange);
        }

        /* Old Input handing for mode send messages on player input

                private void OnWalk(InputValue value) => input.inputWalk = value.Get<Vector2>();

                private void OnCrouch(InputValue value)
                {

                }
                private void OnCamera(InputValue value) => input.inputCamera = value.Get<Vector2>();
                private void OnShield(InputValue value) => input.shielding = value.Get<float>() > 0.5f;

                private void OnAction(InputValue value)
                {
                    input.actioning = value.Get<float>() == 1;
                    currentState.OnInteract(value.Get<float>());
                }
                private void OnSprint(InputValue value)
                {
                    input.sprinting = value.Get<float>() == 1;
                    currentState.OnSprint(value.Get<float>());
                }
                private void OnJump(InputValue value) => currentState.OnJump(value.Get<float>());

                private void OnAttack(InputValue value) => currentState.OnAttack(value.Get<float>());
                private void OnAltAttack(InputValue value) => currentState.OnAltAttack(value.Get<float>());


                private void OnSelectWeapon1(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(0); }
                private void OnSelectWeapon2(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(1); }
                private void OnSelectWeapon3(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(2); }
                private void OnSelectWeapon4(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(3); }
                private void OnSelectWeapon5(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(4); }
                private void OnSelectWeapon6(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(5); }
                private void OnSelectWeapon7(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(6); }
                private void OnSelectWeapon8(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(7); }
                private void OnSelectWeapon9(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(8); }
                private void OnSelectWeapon10(InputValue value) { if (value.Get<float>() == 1) OnSelectWeapon(9); }
        */

        private void OnSelectWeapon(int index)
        {
            //print(String.Format("Switched to weapon {0}", index));

            currentState.OnSelectWeapon(index);
            for (int i = 0; i < parallelStates.Length; i++)
                parallelStates[i].OnSelectWeapon(index);
        }


        private void StartControllerRumble(float duration)
        {
            StartCoroutine(RumbleController(duration));
        }

        private IEnumerator RumbleController(float duration)
        {
            SetRumbleFrequency(0.5f, 0.5f);
            yield return new WaitForSeconds(duration);
            SetRumbleFrequency(0, 0);
        }

        private void SetRumbleFrequency(float low, float high)
        {
            foreach (var gamepad in Gamepad.all)
            {
                gamepad.SetMotorSpeeds(low, high);
            }
        }

        public void ChangeToState<T>(params object[] parameters) where T : MovementState, new()
        {

            currentState?.End(); // state specific end method
            currentState = new T();
            currentState.Init(this); //non - overridable init method for reference to controller


            //update f3 information
            entry.values[0] = currentState.StateName;

            //test to see if this state requires any parallel states to be started
            RequiresParallelState[] attributes = typeof(T).GetCustomAttributes(typeof(RequiresParallelState), true) as RequiresParallelState[];
            ParallelState[] newStates = new ParallelState[attributes.Length];

            for (int i = 0; i < attributes.Length; i++)
            {
                //test if the desired parallel state is currently active
                newStates[i] = GetParallelState(attributes[i].state);
                if (newStates[i] == null)
                {
                    newStates[i] = Activator.CreateInstance(attributes[i].state) as ParallelState;
                    newStates[i].Init(this);
                    newStates[i].Start();
                }
            }

            //go through and end all the parallel states not used by this state
            for (int i = 0; i < parallelStates.Length; i++)
            {
                bool continues = false;
                for (int j = 0; j < attributes.Length; j++)
                {
                    if (parallelStates[i].GetType() == attributes[j].state)
                    {
                        continues = true;
                    }
                }
                if (!continues)
                {
                    parallelStates[i].End();
                }
            }
            parallelStates = newStates;

            if (currentStateText != null)
            {
                currentStateText.text = currentState.StateName;
            }

            // start all the states after everything has been constructed
            currentState.Start(parameters);
        }

        public ParallelState GetParallelState(Type t)
        {
            for (int i = 0; i < parallelStates.Length; i++)
            {
                if (parallelStates[i].GetType() == t)
                {
                    return parallelStates[i];
                }
            }
            return null;
        }
        public bool TryGetParallelState<T>(out T state) where T : ParallelState
        {
            for (int i = 0; i < parallelStates.Length; i++)
            {
                if (parallelStates[i].GetType() == typeof(T))
                {
                    state = parallelStates[i] as T;
                    return true;
                }
            }
            state = default(T);
            return false;
        }


        private void RaycastGround()
        {

            if (Physics.SphereCast(
                transform.position + Vector3.up * groundScanningOffset,
                groundSphereRadius,
                Vector3.down,
                out groundRaycastHit,
                Mathf.Infinity,
                groundLayerMask,
                QueryTriggerInteraction.Ignore))
            {
                currentHeight = transform.position.y - groundRaycastHit.point.y;

                if (currentHeight < maxGroundDistance && Vector3.Angle(groundRaycastHit.normal, Vector3.up) < maxGroundAngle)
                {
                    //hit ground
                    currentState.OnCollideGround(groundRaycastHit);

                    if (!onGround)
                    {
                        onGround = true;
                        currentState.OnGroundedChange();
                    }
                }
                else
                {
                    if (onGround)
                    {
                        onGround = false;
                        currentState.OnGroundedChange();
                    }
                }
            }
        }

        private void RaycastCliff()
        {
            if (Physics.Raycast(transform.position + cliffScanOffset, transform.forward, out cliffRaycastHit, cliffScanningDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.rigidbody == null) // do not climb on moveable objects
                {
                    currentState.OnCollideCliff(cliffRaycastHit);
                }
            }
        }
        void OnDrawGizmos()
        {
            if (currentState != null)
                currentState.OnDrawGizmos();
        }

        //show the sound on the minimap?
        void IAITarget.HearSound(IAITarget source, float volume, ref bool responded) { }

        [Serializable]
        public class Dead : MovementState
        {
            public float respawnTime = 4f;
            public override string StateName => "Dead";

            public override void Start()
            {
                canBeTargeted = false;
                c.controllingCamera = false;
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
                c.controllingCamera = true;

            }
        }
        // [Serializable]
        // //allow weapons to be used while walking and falling
        // public class UseWeaponry : ParallelState
        // {
        //     public override string StateName => "Using Weaponry";
        //     int selectedWeapon = 0;

        //     Vector2 currentRecoil = Vector2.zero;
        //     [NonSerialized] Weapon[] weapons;
        //     bool holdingFire = false;


        //     public override void Start()
        //     {
        //         weapons = c.walkingProperties.playerWeapons.GetComponents<Weapon>();
        //         ItemSelect.SetInstanceItem("Weapon", selectedWeapon);
        //         weapons[selectedWeapon].Initialize(c, true);
        //     }

        //     public override void End()
        //     {
        //         //stop the player firing weapons
        //         if (holdingFire)
        //         {
        //             OnAttack(0);
        //         }
        //         c.weaponGraphicsController.RemoveWeapon();
        //     }
        //     public override void OnSelectWeapon(int index)
        //     {
        //         if (index == selectedWeapon || index >= weapons.Length)
        //         {
        //             return;
        //         }
        //         weapons[selectedWeapon].OnWeaponDeEquip();

        //         selectedWeapon = index;
        //         ItemSelect.SetInstanceItem("Weapon", index);
        //         weapons[selectedWeapon].Initialize(c, true);

        //     }

        //     public override void Update()
        //     {
        //         currentRecoil = c.cameraLookOffset;

        //         if (holdingFire)
        //         {
        //             weapons[selectedWeapon].WhileWeaponHeld(ref currentRecoil);
        //         }
        //         weapons[selectedWeapon].WeaponUpdate(ref currentRecoil);

        //         c.cameraLookOffset = currentRecoil;
        //     }


        //     public override void OnAttack(float state)
        //     {
        //         if (state == 0 && holdingFire)
        //         {
        //             holdingFire = false;
        //             weapons[selectedWeapon].OnWeaponRelease();
        //         }
        //         else if (state == 1)
        //         {
        //             holdingFire = true;
        //             weapons[selectedWeapon].OnWeaponFire();
        //         }
        //     }

        //     public override void OnAltAttack(float state)
        //     {
        //         if (state == 0)
        //         {
        //             weapons[selectedWeapon].OnWeaponAltRelease();
        //         }
        //         else
        //         {
        //             weapons[selectedWeapon].OnWeaponAltFire();
        //         }
        //     }


        //     public override void OnDrawGizmos()
        //     {
        //         weapons[selectedWeapon].OnDrawGizmos();
        //     }
        // }


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
        [Serializable]
        [RequiresParallelState(typeof(ToggleMenus))]
        [RequiresParallelState(typeof(CameraControl))]
        [RequiresParallelState(typeof(Interact))]
        public class Walking : MovementState
        {
            public override string StateName => "Walking";
            [System.Serializable]
            public struct WalkingProperties
            {
                public float walkingSpeed;
                public float runningSpeed;
                public float crouchingSpeed;

                public float walkingHeight;
                public float crouchingHeight;

                public float groundClamp;
                public float maxAcceleration;
                public float maxStepHeight;
                public float stepSearchOvershoot;
                public float jumpForce;
                public GameObject playerWeapons;
            }
            WalkingProperties p => c.walkingProperties;
            Vector3 currentGroundNormal = new Vector3();

            Vector3 requiredForce;
            [SerializeField] Vector3 desiredVelocity;
            //used to continue momentum when the controller hits a stair
            Vector3 lastVelocity;
            CameraControl cc;
            Vector3 groundVelocity;
            //shooting variables for gizmos

            [NonSerialized] public DebugMenu.DebugEntry entry;

            public override void Start()
            {
                entry = DebugMenu.CreateEntry("Player", "Velocity: {0:0.0}", 0);

                c.controllingCamera = false; // debug for camera parallel state

                //c.transform.up = Vector3.up;

                c.rb.isKinematic = false;
                c.TryGetParallelState(out cc);
            }

            public bool CanSprint => c.input.sprinting && c.input.inputWalk.y > 0.5f;
            bool grounded;

            bool crouching;
            [NonSerialized] Collider[] crouchTestColliders = new Collider[2];
            public override void FixedUpdate()
            {
                if (c.onGround == false)
                {
                    c.ChangeToState<Freefalling>();
                    return;
                }

                Vector3 velocity = c.rb.velocity;
                Vector3 playerDirection = cc.TransformInput(c.input.inputWalk);

                grounded = FindGround(out ContactPoint groundCP, c.allCPs);

                if (grounded)
                {

                    //step up onto the stair, reseting the velocity to what it was
                    if (FindStep(out Vector3 stepUpOffset, c.allCPs, groundCP, playerDirection))
                    {
                        transform.position += stepUpOffset;
                        c.rb.velocity = lastVelocity;

                        c.cameraStepDistance = stepUpOffset.y;
                        c.currentCameraStepTime = 0;

                        //c.StartCoroutine(StepToPoint(transform.position + stepUpOffset, lastVelocity));
                    }
                }
                else
                {
                    if (!c.onGround)
                    {
                        c.ChangeToState<Freefalling>();
                    }
                }




                //c.transform.rotation = Quaternion.Euler(0, cc.camRotation.x, 0);




                if (c.mod.HasFlag(MovementModifiers.Crouching))
                {
                    c.collider.height = p.crouchingHeight;
                    crouching = true;
                }
                else if (crouching)
                {
                    //crouch button not pressed but still crouching
                    Vector3 p1 = transform.position + Vector3.up * p.walkingHeight * 0.05F;
                    Vector3 p2 = transform.position + Vector3.up * p.walkingHeight;
                    Physics.OverlapCapsuleNonAlloc(p1, p2, c.collider.radius, crouchTestColliders, c.groundLayerMask, QueryTriggerInteraction.Ignore);
                    if (crouchTestColliders[1] == null)
                        //There is no collider intersecting other then the player
                        crouching = false;
                    else crouchTestColliders[1] = null;
                }

                if (!crouching)
                    c.collider.height = p.walkingHeight;

                c.collider.center = Vector3.up * c.collider.height * 0.5f;

                //TODO - more variability
                cc.SetVerticalOffset(c.collider.height - 0.2f);



                Vector3 desiredVelocity;

                if (playerDirection.sqrMagnitude > 0.1f)
                {
                    Quaternion walkingAngle = Quaternion.LookRotation(playerDirection);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, walkingAngle, Time.deltaTime * 800);
                    if (Quaternion.Angle(transform.rotation, walkingAngle) > 30f)
                    {
                        //Only allow the player to walk forward if they have finished turning to the direction
                        //But do allow the player to run at a slight angle
                        desiredVelocity = Vector3.zero;
                    }
                    else
                    {
                        //Let the player move in the direction they are pointing

                        //scale required velocity by current speed
                        //only allow sprinting if the play is moving forward
                        float speed;
                        if (crouching)
                            speed = p.crouchingSpeed;
                        else if (c.mod.HasFlag(MovementModifiers.Sprinting))
                            speed = p.runningSpeed;
                        else
                            speed = p.walkingSpeed;

                        desiredVelocity = playerDirection * speed;
                    }

                }
                else
                {
                    desiredVelocity = Vector3.zero;
                }



                requiredForce = desiredVelocity - c.rb.velocity;
                requiredForce.y = 0;

                requiredForce = Vector3.ClampMagnitude(requiredForce, p.maxAcceleration * Time.fixedDeltaTime);

                //rotate the target based on the ground the player is standing on

                requiredForce = Vector3.ProjectOnPlane(requiredForce, currentGroundNormal);

                requiredForce -= currentGroundNormal * p.groundClamp;

                c.rb.AddForce(requiredForce, ForceMode.VelocityChange);

                lastVelocity = velocity;

                entry.values[0] = c.rb.velocity.magnitude;
            }

            /// Finds the MOST grounded (flattest y component) ContactPoint
            /// \param allCPs List to search
            /// \param groundCP The contact point with the ground
            /// \return If grounded
            public static bool FindGround(out ContactPoint groundCP, List<ContactPoint> allCPs)
            {
                groundCP = default(ContactPoint);
                bool found = false;
                foreach (ContactPoint cp in allCPs)
                {
                    //Pointing with some up direction
                    if (cp.normal.y > 0.0001f && (found == false || cp.normal.y > groundCP.normal.y))
                    {
                        groundCP = cp;
                        found = true;
                    }
                }

                return found;
            }
            /// Find the first step up point if we hit a step
            /// \param allCPs List to search
            /// \param stepUpOffset A Vector3 of the offset of the player to step up the step
            /// \return If we found a step
            bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allCPs, ContactPoint groundCP, Vector3 currVelocity)
            {
                stepUpOffset = default(Vector3);

                //No chance to step if the player is not moving
                Vector2 velocityXZ = new Vector2(currVelocity.x, currVelocity.z);
                if (velocityXZ.sqrMagnitude < 0.0001f)
                    return false;
                for (int i = 0; i < allCPs.Count; i++)// test if every point is suitable for a step up
                {
                    if (ResolveStepUp(out stepUpOffset, allCPs[i], groundCP, currVelocity))
                        return true;
                }
                return false;
            }
            /// Takes a contact point that looks as though it's the side face of a step and sees if we can climb it
            /// \param stepTestCP ContactPoint to check.
            /// \param groundCP ContactPoint on the ground.
            /// \param stepUpOffset The offset from the stepTestCP.point to the stepUpPoint (to add to the player's position so they're now on the step)
            /// \return If the passed ContactPoint was a step
            bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestCP, ContactPoint groundCP, Vector3 velocity)
            {
                stepUpOffset = default(Vector3);
                Collider stepCol = stepTestCP.otherCollider;

                //( 1 ) Check if the contact point normal matches that of a step (y close to 0)
                // if (Mathf.Abs(stepTestCP.normal.y) >= 0.01f)
                // {
                //     return false;
                // }

                //if the step and the ground are too close, do not count
                if (Vector3.Dot(stepTestCP.normal, groundCP.normal) > 0.95f)
                {
                    return false;
                }

                //( 2 ) Make sure the contact point is low enough to be a step
                if (!(stepTestCP.point.y - groundCP.point.y < c.walkingProperties.maxStepHeight))
                {
                    return false;
                }





                //( 2.5 ) Make sure the step is in the direction the player is moving
                Vector3 stepDirection = stepTestCP.point - transform.position;
                if (Vector3.Dot(stepDirection.normalized, velocity.normalized) < 0.01f)
                {
                    //not pointing in the general direction of movement - fail
                    return false;
                }


                //( 3 ) Check to see if there's actually a place to step in front of us
                //Fires one Raycast
                RaycastHit hitInfo;
                float stepHeight = groundCP.point.y + c.walkingProperties.maxStepHeight + 0.0001f;

                Vector3 stepTestInvDir = new Vector3(-stepTestCP.normal.x, 0, -stepTestCP.normal.z).normalized;

                //check forward based off the direction the player is walking

                Vector3 origin = new Vector3(stepTestCP.point.x, stepHeight, stepTestCP.point.z) + (stepTestInvDir * c.walkingProperties.stepSearchOvershoot);
                Vector3 direction = Vector3.down;
                if (!(stepCol.Raycast(new Ray(origin, direction), out hitInfo, c.walkingProperties.maxStepHeight)))
                {
                    return false;
                }

                //We have enough info to calculate the points
                Vector3 stepUpPoint = new Vector3(stepTestCP.point.x, hitInfo.point.y + 0.0001f, stepTestCP.point.z) + (stepTestInvDir * c.walkingProperties.stepSearchOvershoot);
                Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestCP.point.x, groundCP.point.y, stepTestCP.point.z);

                //We passed all the checks! Calculate and return the point!
                stepUpOffset = stepUpPointOffset;
                return true;
            }

            // IEnumerator StepToPoint(Vector3 point, Vector3 lastVelocity)
            // {
            //     c.rb.isKinematic = true;
            //     Vector3 start = transform.position;
            //     Vector3 pos = Vector3.zero;
            //     Vector2 xzStart = new Vector2(start.x, start.z);
            //     Vector2 xzEnd = new Vector2(point.x, point.z);
            //     Vector2 xz;
            //     float t = 0;
            //     while (t < 1)
            //     {
            //         t += Time.deltaTime;
            //         t = Mathf.Clamp01(t);
            //         //lerp y values
            //         //first quarter of sin graph is quick at first but slower later
            //         pos.y = Mathf.Lerp(start.y, point.y, Mathf.Sin(t * Mathf.PI * 0.5f));
            //         //lerp xz values
            //         xz = Vector2.Lerp(xzStart, xzEnd, t);
            //         pos.x = xz.x;
            //         pos.z = xz.y;
            //         yield return new WaitForEndOfFrame();
            //     }
            //     c.rb.isKinematic = false;
            //     c.rb.velocity = lastVelocity;
            // }


            public override void Animate()
            {
                c.animator.SetBool("IsSurfing", false);

                c.animator.SetFloat("InputVertical", c.input.inputWalk.magnitude);
                //c.animator.SetFloat("InputHorizontal", c.input.inputWalk.x);
                c.animator.SetBool("IsGrounded", true);
                c.animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
                c.animator.SetFloat("GroundDistance", c.currentHeight);
            }

            public override void OnJump(float state)
            {
                if (state == 1 && grounded)
                {
                    //use acceleration to give constant upwards force regardless of mass
                    Vector3 v = c.rb.velocity;
                    v.y = c.walkingProperties.jumpForce;
                    c.rb.velocity = v;

                    ChangeToState<Freefalling>();
                }
            }


            public override void OnCollideGround(RaycastHit hit)
            {
                currentGroundNormal = hit.normal;
                //Make the player stand on a platform if it is kinematic
                if (hit.rigidbody != null && hit.rigidbody.isKinematic)
                {
                    groundVelocity = hit.rigidbody.velocity;
                    transform.SetParent(hit.transform, true);
                }
                else
                {
                    transform.SetParent(null, true);
                }


                //attempt to lock the player to the ground while walking

            }
            public override void OnCollideCliff(RaycastHit hit)
            {
                if (c.climbing && hit.rigidbody != null && hit.rigidbody.isKinematic == true && Vector3.Dot(-hit.normal, cc.TransformInput(c.input.inputWalk)) > 0.5f)
                {
                    if (Vector3.Angle(Vector3.up, hit.normal) > c.minAngleForCliff)
                    {
                        ChangeToState<Climbing>();
                    }
                    else
                    {
                        print("did not engage climb as {0} is too shallow", Vector3.Angle(Vector3.up, hit.normal));
                    }
                }
            }


            public override void End()
            {
                transform.SetParent(null, true);
                DebugMenu.RemoveEntry(entry);

                //make sure the collider is left correctly
                c.collider.height = p.walkingHeight;
                c.collider.center = Vector3.up * c.collider.height * 0.5f;
            }
            public override void OnDrawGizmos()
            {
                for (int i = 0; i < c.allCPs.Count; i++)
                {
                    //draw positions the ground is touching
                    Gizmos.DrawWireSphere(c.allCPs[i].point, 0.05f);
                    Gizmos.DrawLine(c.allCPs[i].point, c.allCPs[i].point + c.allCPs[i].normal * 0.1f);
                }

                Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * c.walkingProperties.maxStepHeight, Quaternion.identity, new Vector3(1, 0, 1));
                Gizmos.color = Color.yellow;
                //draw a place to reprosent max step height
                Gizmos.DrawWireSphere(Vector3.zero, c.walkingProperties.stepSearchOvershoot + 0.25f);
            }
        }
        [Serializable]
        //[RequiresParallelState(typeof(UseWeaponry))]
        [RequiresParallelState(typeof(CameraControl))]
        public class Freefalling : MovementState
        {

            [System.Serializable]
            public struct FreefallingProperties
            {
                public int airJumps;
                public float airJumpVelocity;
                public float airJumpAngleFromVertical;
            }



            FreefallingProperties p => c.freefallingProperties;

            Vector3 desiredVelocity;

            int airJumps;

            CameraControl cc;

            public override void FixedUpdate()
            {
                desiredVelocity = cc.TransformInput(c.input.inputWalk);

                c.rb.AddForce(desiredVelocity);

                //only change back when the body is actually touching the ground

            }


            public override void OnCollideGround(RaycastHit hit)
            {
                //Only go to walking if they player is not moving upwards
                if (Vector3.Dot(hit.normal, Vector3.up) > c.maxGroundDot && c.rb.velocity.y <= 0)
                {
                    ChangeToState<Walking>();
                }
            }

            public override void Animate()
            {
                animator.SetBool("IsSurfing", false);
                animator.SetFloat("InputVertical", c.input.inputWalk.magnitude * (c.input.sprinting ? c.walkingProperties.runningSpeed : c.walkingProperties.walkingSpeed));
                animator.SetBool("IsGrounded", c.onGround);
                animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
                animator.SetFloat("GroundDistance", c.currentHeight);
            }


            public override void OnInteract(float state)
            {
                if (c.surfing)
                {
                    if (state == 1)//shield surfing combo - shield, jump, interact
                    {
                        ChangeToState<Shieldsurfing>();
                    }
                }
            }

            public override void OnJump(float state)
            {
                if (airJumps > 0 && state == 1)
                {
                    airJumps--;
                    c.rb.AddForce(Vector3.up * (p.airJumpVelocity - c.rb.velocity.y), ForceMode.VelocityChange);
                }
            }
            public override void OnCollideCliff(RaycastHit hit)
            {
                if (c.input.inputWalk.sqrMagnitude > 0.5f)
                {
                    ChangeToState<Climbing>();
                }
            }
            public override string StateName => "Falling";

            public override void Start()
            {
                airJumps = p.airJumps;
                c.TryGetParallelState(out cc);
            }
        }
        [Serializable]
        [RequiresParallelState(typeof(CameraControl))]
        public class Climbing : MovementState
        {
            [System.Serializable]
            public struct ClimbingProperties
            {
                public float speed;
                public float distanceFromCliffFace;
            }
            ClimbingProperties p => c.climbingProperties;

            Vector3 currentCliffNormal;
            Vector3 currentCliffPoint;

            Vector3 scanOffsetOffset;

            public override string StateName => "Climbing";

            public override void Start()
            {
                c.rb.isKinematic = true;
            }
            public override void End()
            {
                c.rb.isKinematic = false;
            }

            public override void OnCollideCliff(RaycastHit hit)
            {
                currentCliffNormal = hit.normal.normalized;
                currentCliffPoint = hit.point;


                if (Vector3.Angle(Vector3.up, currentCliffNormal) < c.minAngleForCliff)
                {
                    c.transform.up = Vector3.up;
                    ChangeToState<Walking>();
                }
                c.transform.forward = -currentCliffNormal;

                scanOffsetOffset = -c.transform.up;
                scanOffsetOffset.Scale(c.cliffScanOffset);

                c.transform.position =
                    currentCliffPoint + scanOffsetOffset
                + currentCliffNormal * p.distanceFromCliffFace
                + c.transform.up * c.input.inputWalk.y * Time.deltaTime * p.speed
              + c.transform.right * c.input.inputWalk.x * Time.deltaTime * p.speed;



                if (c.input.inputWalk.y > 0f)//player moving up,
                {
                    //scan to see if the top of the cliff has been reached
                    if (!Physics.Raycast(c.transform.position + c.cliffTopScanOffset, c.transform.forward, c.cliffScanningDistance, c.groundLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        c.transform.position += c.transform.forward * c.cliffScanningDistance + c.cliffTopScanOffset;
                        ChangeToState<Walking>();
                    }
                }

            }
            public override void OnCollideGround(RaycastHit hit)
            {
                //allow the climb to be cancelled when payer is moving down toward ground
                if (c.input.inputWalk.y < -0.5)
                {
                    ChangeToState<Walking>();
                }
            }
            public override void Animate()
            {
                c.animator.SetBool("IsSurfing", false);
                c.animator.SetFloat("InputVertical", c.input.inputWalk.magnitude * (c.input.sprinting ? c.walkingProperties.runningSpeed : c.walkingProperties.walkingSpeed));
                c.animator.SetBool("IsGrounded", c.onGround);
                c.animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
                c.animator.SetFloat("GroundDistance", c.currentHeight);
            }
            public override void OnJump(float state)
            {
                if (state == 1)//make player jump away from 
                {
                    c.rb.isKinematic = false;
                    c.rb.AddForce(currentCliffNormal * c.jumpForce, ForceMode.Acceleration);
                    ChangeToState<Freefalling>();
                }
            }

            public override void OnDrawGizmos()
            {
                Gizmos.DrawLine(currentCliffPoint, currentCliffPoint + currentCliffNormal);
            }
        }
        [Serializable]
        [RequiresParallelState(typeof(CameraControl))]
        public class Shieldsurfing : MovementState
        {
            CameraControl cc;
            public override void Start()
            {
                originalMaterial = c.collider.material;
                c.collider.material = p.surfPhysicMat;
                c.TryGetParallelState(out cc);
            }
            public override string StateName => "Shield Surfing";
            [System.Serializable]
            public struct ShieldsurfingProperties
            {
                public float turningTorqueForce;
                public float minSurfingSpeed;
                public float turningAngle;

                public PhysicMaterial surfPhysicMat;
            }
            ShieldsurfingProperties p => c.shieldsurfingProperties;
            float turning;

            PhysicMaterial originalMaterial;
            float currentSpeed;

            public override void FixedUpdate()
            {
                currentSpeed = c.rb.velocity.magnitude;
                if (currentSpeed <= p.minSurfingSpeed)
                {
                    c.ChangeToState<Walking>();
                }

                turning = c.input.inputWalk.x * Time.fixedDeltaTime * p.turningTorqueForce;

                c.rb.velocity = Quaternion.Euler(0, turning, 0) * c.rb.velocity;

                //set player orientation
                transform.forward = c.rb.velocity;
                transform.rotation *= Quaternion.Euler(0, 0, c.input.inputWalk.x * -p.turningAngle);
            }
            public override void Animate()
            {
                animator.SetBool("IsSurfing", true);
                animator.SetBool("IsGrounded", c.onGround);
                animator.SetFloat("InputHorizontal", cc.TransformInput(c.input.inputWalk).x);
                animator.SetFloat("InputVertical", cc.TransformInput(Vector2.up * c.rb.velocity.z).z);//set it to forward velocity
                animator.SetFloat("VerticalVelocity", c.rb.velocity.y);
            }
            public override void OnJump(float state)
            {
                if (state == 1)
                {
                    c.rb.AddForce(Vector3.up * c.jumpForce, ForceMode.Acceleration);
                }
            }

            public override void OnSprint(float state)
            {
                if (state == 1)
                {
                    c.ChangeToState<Walking>();
                }
            }



            public override void End()
            {
                c.collider.material = originalMaterial;
            }
        }


    }


    [Serializable]
    public abstract class ParallelState : MovementState
    {

    }

    [Serializable]
    public abstract class MovementState : State
    {
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


        public virtual void Animate() { }
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
}