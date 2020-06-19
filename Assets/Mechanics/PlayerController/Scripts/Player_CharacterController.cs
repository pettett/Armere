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


        // public event Action<Player_CharacterController> onDeath;
        //public event Action<Player_CharacterController> onRepawn;

        public event Action<InputAction.CallbackContext> onPlayerInput;

        [Serializable]
        public struct InputStatus
        {
            public Vector2 inputWalk;
            public Vector2 inputCamera;
        }

        public MovementModifiers mod;
        public ItemDatabase db;
        public Transform cameraTransform;
        public Cinemachine.CinemachineFreeLook freeLook;
        public Cinemachine.CinemachineTargetGroup conversationGroup;
        public Cinemachine.CinemachineVirtualCamera cutsceneCamera;
        public Transform lookAtTarget;
        Camera playerCamera;

        private MovementState currentState;
        public Type CurrentStateType => currentState?.GetType();
        public Type lastStateType = null;

        //Parallel state list:

        //Camera Controller - DONE
        //Weapons - DONE
        //Interaction with objects - DONE

        private ParallelState[] parallelStates = new ParallelState[0];

        public CameraControl cameraController;

        MovementState[] allStates;

        [Header("State Properties")]

        public Walking.WalkingProperties m_walkingProperties = new Walking.WalkingProperties();
        public Freefalling.FreefallingProperties m_freefallingProperties = new Freefalling.FreefallingProperties();
        public Climbing.ClimbingProperties m_climbingProperties = new Climbing.ClimbingProperties();
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


        //set capacity to 1 as it is common for the player to be touching the ground in at least one point
        [HideInInspector] public List<ContactPoint> allCPs = new List<ContactPoint>(1);

        DebugMenu.DebugEntry entry;

        void OnCollisionEnter(Collision col) => allCPs.AddRange(col.contacts);
        void OnCollisionStay(Collision col) => allCPs.AddRange(col.contacts);

        Collider IAITarget.collider => collider;
        public bool canBeTargeted => currentState != null && currentState.canBeTargeted;
        public Vector3 velocity => rb.velocity;
        int _engagementCount = 0;

        public Dictionary<string, object> persistentStateData = new Dictionary<string, object>();


        public AnimatorVariables animatorVariables;

        public static List<PlayerRelativeObject> relativeObjects = new List<PlayerRelativeObject>();


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
            animatorVariables.UpdateIDs();



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

            m_maxGroundDot = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad);

            entry = DebugMenu.CreateEntry("Player", "Current State: {0}", "");

            cameraController = new CameraControl();
            cameraController.Init(this);
            cameraController.Start();


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
                    for (int i = 0; i < allStates.Length; i++)
                        if (StateActive(i))
                            allStates[i].OnInteract(action.ReadValue<float>());
                    break;
                case "Crouch":
                    UpdateModifier(action.ReadValue<float>() == 1, MovementModifiers.Crouching);
                    break;
                case "Sprint":
                    UpdateModifier(action.ReadValue<float>() == 1, MovementModifiers.Sprinting);
                    for (int i = 0; i < allStates.Length; i++)
                        if (StateActive(i))
                            allStates[i].OnSprint(action.ReadValue<float>());
                    break;
                case "Jump":
                    for (int i = 0; i < allStates.Length; i++)
                        if (StateActive(i))
                            allStates[i].OnJump(action.ReadValue<float>());
                    break;
                case "Attack":
                    for (int i = 0; i < allStates.Length; i++)
                        if (StateActive(i))
                            allStates[i].OnAttack(action.ReadValue<float>());
                    break;
                case "AltAttack":
                    for (int i = 0; i < allStates.Length; i++)
                        if (StateActive(i))
                            allStates[i].OnAltAttack(action.ReadValue<float>());
                    break;

                default:
                    currentState.OnCustomAction(action);
                    break;
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


            for (int i = 0; i < relativeObjects.Count; i++)
            {
                sqrMagTemp = (transform.position - relativeObjects[i].transform.position).sqrMagnitude;
                if (relativeObjects[i].enabled && sqrMagTemp > relativeObjects[i].disableRange * relativeObjects[i].disableRange)
                {
                    //Disable the object - disable range should be larger then enable range to stop object flickering
                    relativeObjects[i].Disable();
                }
                //else, check if it is close enough to enable
                else if (!relativeObjects[i].enabled && sqrMagTemp < relativeObjects[i].enableRange * relativeObjects[i].enableRange)
                {
                    //Enable the object
                    relativeObjects[i].Enable();
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
        private void OnSelectWeapon(int index)
        {
            //print(String.Format("Switched to weapon {0}", index));

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
            currentState.Init(this); //non - overridable init method for reference to controller


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

            allStates = new MovementState[parallelStates.Length + 2];
            for (int i = 0; i < parallelStates.Length; i++)
            {
                allStates[i] = parallelStates[i];
            }
            allStates[parallelStates.Length] = currentState;
            allStates[parallelStates.Length + 1] = cameraController;


            return currentState;
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

    }
}