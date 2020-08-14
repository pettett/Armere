using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PlayerController
{



    [RequireComponent(typeof(Rigidbody))]
    public class Player_CharacterController : MonoBehaviour, IAITarget, IWaterObject, IInteractor
    {
        [System.Serializable]
        public class PlayerSaveData
        {
            public MovementState currentState;
            public ParallelState[] parallels;
            public Vector3 position;
            public Quaternion rotation;

            public PlayerSaveData(Player_CharacterController c)
            {

                currentState = c.currentState;
                parallels = c.parallelStates;
                position = c.transform.position;
                rotation = c.transform.rotation;
            }
        }
        public PlayerSaveData CreateSaveData()
        {
            return new PlayerSaveData(this);
        }
        public void RestoreSave(PlayerSaveData data)
        {
            print("Restored state");

            currentState = data.currentState;
            print(currentState);
            parallelStates = data.parallels;
            transform.SetPositionAndRotation(data.position, data.rotation);
            loadedStates = true;
        }

        // public event Action<Player_CharacterController> onDeath;
        // public event Action<Player_CharacterController> onRepawn;
        public delegate bool ProcessPlayerInputDelegate(InputAction.CallbackContext input);
        ///<summary> Returns wether the input should still be processed</summary>
        public event ProcessPlayerInputDelegate onPlayerInput;

        [Serializable]
        public struct InputStatus
        {
            public Vector2 horizontal;
            public Vector2 camera;
            public float vertical;
        }

        public MovementModifiers mod;

        public ItemDatabase db;


        public Transform lookAtTarget;
        [HideInInspector] public PlayerInput playerInput;


        private MovementState currentState;
        public Type CurrentStateType => currentState?.GetType();
        public Type lastStateType = null;

        //Parallel state list:

        //Camera Controller - DONE
        //Weapons - DONE
        //Interaction with objects - DONE

        private ParallelState[] parallelStates = new ParallelState[0];

        public CameraControl cameraController;

        public Yarn.Unity.DialogueRunner runner;

        MovementState[] allStates;

        public Room currentRoom;

        [Header("State Properties")]

        public Walking.WalkingProperties m_walkingProperties = new Walking.WalkingProperties();
        public Freefalling.FreefallingProperties m_freefallingProperties = new Freefalling.FreefallingProperties();
        //public Climbing.ClimbingProperties m_climbingProperties = new Climbing.ClimbingProperties();
        public Shieldsurfing.ShieldsurfingProperties m_shieldsurfingProperties = new Shieldsurfing.ShieldsurfingProperties();

        public float jumpForce = 1000f;


        public float cliffScanningDistance = 0.31f;
        [Range(0, 90)]
        public float minAngleForCliff = 70;
        public Vector3 cliffTopScanOffset = Vector3.up;
        public Vector3 m_cliffScanOffset = Vector3.up * .3f;

        [Header("Ground detection")]

        public float m_maxGroundDistance = 0.1f;
        public float m_groundSphereRadius = 0.3f;
        public float m_groundScanningOffset = 1f; // moves raycast start point up to stop normal of 0 when origin touches hit point
        [Range(0, 90)] public float m_maxGroundAngle = 70;
        [HideInInspector] public float m_maxGroundDot = 0.3f;
        public bool onGround;
        [Header("Movement")]
        [Range(0, 1)]
        public float dynamicFriction = 0.2f;
        public float maxWaterStrideDepth = 1;
        public float waterDrag = 1;
        public float waterMovementForce = 1;
        [Header("Climbing")]
        public float climbingColliderHeight = 1.6f;
        public float climbingSpeed = 4f;
        public float transitionTime = 4f;

        [Header("Weapons")]
        public Transform arrowSpawn;
        public Transform bowPivot;

        [Header("Other")]

        public LayerMask m_groundLayerMask;
        public Rigidbody rb;
        [HideInInspector] public new CapsuleCollider collider;

        [HideInInspector]
        public Animator animator;

        public TMPro.TextMeshProUGUI currentStateText;
        [SerializeField] public InputStatus input = new InputStatus(); //current player input




        public Health health;

        public float currentHeight = 0;
        //raycast hit cached
        private RaycastHit groundRaycastHit;
        private RaycastHit cliffRaycastHit;

        public WeaponGraphicsController weaponGraphicsController;

        public RaycastHit hit;


        public static Player_CharacterController activePlayerController;

        [HideInInspector]
        public AnimationController animationController;

        public LineRenderer projectileTrajectoryRenderer;
        //set capacity to 1 as it is common for the player to be touching the ground in at least one point
        [HideInInspector] public List<ContactPoint> allCPs = new List<ContactPoint>(1);

        DebugMenu.DebugEntry entry;



        Collider IAITarget.collider => collider;
        public bool canBeTargeted => currentState != null && currentState.canBeTargeted;
        public Vector3 velocity => rb.velocity;
        int _engagementCount = 0;

        public Dictionary<string, object> persistentStateData = new Dictionary<string, object>();


        public AnimatorVariables animatorVariables;



        [HideInInspector] public WaterController currentWater;



        bool loadedStates = false;
        // Start is called before the first frame update
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            activePlayerController = this;
            animationController = GetComponent<AnimationController>();
        }
        void OnCollisionEnter(Collision col) => allCPs.AddRange(col.contacts);
        void OnCollisionStay(Collision col) => allCPs.AddRange(col.contacts);
        private void Start()
        {

            animatorVariables.UpdateIDs();

            playerInput = GetComponent<PlayerInput>();

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            collider = GetComponent<CapsuleCollider>();
            if (TryGetComponent<Health>(out health))
                health.onDeath += OnDeath;

            weaponGraphicsController = GetComponent<WeaponGraphicsController>();

            m_maxGroundDot = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad);

            entry = DebugMenu.CreateEntry("Player", "Current State: {0}", "");

            cameraController = new CameraControl();
            cameraController.Init(this);
            cameraController.Start();


            if (loadedStates)
            {
                StartAllStates();
                FillAllStates();
            }
            else
            {
                //start a fresh state
                ChangeToState<Walking>();
            }

            collider.material.dynamicFriction = dynamicFriction;


            GetComponent<PlayerInput>().onActionTriggered += OnActionTriggered;

            GetComponent<Ragdoller>().RagdollEnabled = false;



        }

        public void EnterRoom(Room room)
        {
            currentRoom = room;

            StartCoroutine(CameraVolumeController.s.ApplyOverrideProfile(room == null ? null : room.overrideProfile, 3f));
        }

        private void OnDestroy()
        {
            foreach (var s in allStates) s.End();
        }

        public void OnDeath(GameObject attacker, GameObject victim)
        {
            // The player has died.
            ChangeToState<Dead>();

        }
        bool _paused = false;
        public bool paused
        {
            get => _paused;
            set
            {
                if (_paused != value)
                {
                    //value has changed
                    if (value)
                        Pause();
                    else
                        Play();
                }
            }
        }

        public int engagementCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Pause()
        {
            Time.timeScale = 0;
            _paused = true;
        }
        public void Play()
        {
            Time.timeScale = 1;
            _paused = false;
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
        public bool StateActive(int i) => !paused || paused && allStates[i].updateWhilePaused;

        public void OnActionTriggered(InputAction.CallbackContext action)
        {
            //Convert nullable bool into bool - defaults to true
            if (onPlayerInput?.Invoke(action) ?? true)
            {
                string actionName = action.action.name;

                switch (actionName)
                {
                    case "SelectWeapon":
                        OnSelectWeapon((int)action.ReadValue<float>(), action.phase);
                        break;
                    case "Walk":
                        input.horizontal = action.ReadValue<Vector2>();
                        break;
                    case "Look":
                        input.camera = action.ReadValue<Vector2>();
                        break;
                    case "VerticalMovement":
                        input.vertical = action.ReadValue<float>();
                        break;

                    case "Action":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnInteract(action.phase);
                        break;
                    case "Crouch":
                        UpdateModifier(action.ReadValue<float>() == 1, MovementModifiers.Crouching);
                        break;
                    case "Sprint":
                        UpdateModifier(action.ReadValue<float>() == 1, MovementModifiers.Sprinting);
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnSprint(action.phase);
                        break;
                    case "Jump":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnJump(action.phase);
                        break;
                    case "Attack":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnAttack(action.phase);
                        break;
                    case "AltAttack":
                        for (int i = 0; i < allStates.Length; i++)
                            if (StateActive(i))
                                allStates[i].OnAltAttack(action.phase);
                        break;

                    default:
                        currentState.OnCustomAction(action);
                        break;
                }
            }
        }

        float sqrMagTemp;
        // Update is called once per frame
        private void Update()
        {
            RaycastGround();
            RaycastCliff();
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].Update();

            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].Animate(animatorVariables);


            for (int i = 0; i < PlayerRelativeObject.relativeObjects.Count; i++)
            {
                sqrMagTemp = (transform.position - PlayerRelativeObject.relativeObjects[i].transform.position).sqrMagnitude;
                if (PlayerRelativeObject.relativeObjects[i].enabled && sqrMagTemp > PlayerRelativeObject.relativeObjects[i].disableRange * PlayerRelativeObject.relativeObjects[i].disableRange)
                {
                    //Disable the object - disable range should be larger then enable range to stop object flickering
                    PlayerRelativeObject.relativeObjects[i].OnPlayerOutRange();
                }
                //else, check if it is close enough to enable
                else if (!PlayerRelativeObject.relativeObjects[i].enabled && sqrMagTemp < PlayerRelativeObject.relativeObjects[i].enableRange * PlayerRelativeObject.relativeObjects[i].enableRange)
                {
                    //Enable the object
                    PlayerRelativeObject.relativeObjects[i].OnPlayerInRange();
                }
            }
        }


        private void LateUpdate()
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].LateUpdate();
        }

        void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].OnTriggerEnter(other);
        }
        void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].OnTriggerExit(other);
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].FixedUpdate();

            allCPs.Clear();
        }
        private void OnAnimatorIK(int layerIndex)
        {
            for (int i = 0; i < allStates.Length; i++)
                if (StateActive(i))
                    allStates[i].OnAnimatorIK(layerIndex);
        }
        private void OnSelectWeapon(int index, InputActionPhase phase)
        {
            //print(String.Format("Switched to weapon {0}", index));
            if (phase == InputActionPhase.Started)
                for (int i = 0; i < allStates.Length; i++)
                    if (StateActive(i))
                        allStates[i].OnSelectWeapon(index);
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

        public T ChangeToState<T>(params object[] parameters) where T : MovementState, new()
        {
            return ChangeToState(typeof(T), parameters) as T;
        }
        public MovementState ChangeToState(Type type, params object[] parameters)
        {
            lastStateType = CurrentStateType;
            currentState?.End(); // state specific end method
            currentState = Activator.CreateInstance(type) as MovementState;



            //update f3 information
            entry.values[0] = currentState.StateName;

            //test to see if this state requires any parallel states to be started
            RequiresParallelState[] attributes = type.GetCustomAttributes(typeof(RequiresParallelState), true) as RequiresParallelState[];
            ParallelState[] newStates = new ParallelState[attributes.Length];

            for (int i = 0; i < attributes.Length; i++)
            {
                //test if the desired parallel state is currently active
                newStates[i] = GetParallelState(attributes[i].state);
                if (newStates[i] == null)
                {
                    newStates[i] = Activator.CreateInstance(attributes[i].state) as ParallelState;
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



            StartAllStates(parameters);
            FillAllStates();

            return currentState;
        }
        public void FillAllStates()
        {
            allStates = new MovementState[parallelStates.Length + 2];
            for (int i = 0; i < parallelStates.Length; i++)
            {
                allStates[i] = parallelStates[i];
            }
            allStates[parallelStates.Length] = currentState;
            allStates[parallelStates.Length + 1] = cameraController;
        }
        public void StartAllStates(params object[] parameters)
        {
            for (int i = 0; i < parallelStates.Length; i++)
            {
                parallelStates[i].Init(this);
            }
            currentState.Init(this); //non - overridable init method for reference to controller

            for (int i = 0; i < parallelStates.Length; i++)
            {
                parallelStates[i].Start();
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
            state = default;
            return false;
        }


        private void RaycastGround()
        {

            if (Physics.SphereCast(
                transform.position + Vector3.up * m_groundScanningOffset,
                m_groundSphereRadius,
                Vector3.down,
                out groundRaycastHit,
                Mathf.Infinity,
                m_groundLayerMask,
                QueryTriggerInteraction.Ignore))
            {
                currentHeight = transform.position.y - groundRaycastHit.point.y;

                if (currentHeight < m_maxGroundDistance && Vector3.Angle(groundRaycastHit.normal, Vector3.up) < m_maxGroundAngle)
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
            if (Physics.Raycast(transform.position + m_cliffScanOffset, transform.forward, out cliffRaycastHit, cliffScanningDistance, m_groundLayerMask, QueryTriggerInteraction.Ignore))
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



        public void OnWaterEnter(WaterController waterController)
        {
            currentWater = waterController;
        }

        public void OnWaterExit(WaterController waterController)
        {
            currentWater = null;
        }
    }
}