using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Cinemachine;
using Yarn;
using TMPro;
using System.Linq;


public class NPC : AIBase, IInteractable, IVariableAddon
{
    bool IInteractable.canInteract { get => enabled; set => enabled = value; }
    string IVariableAddon.prefix => "$NPC_";
    Value IVariableAddon.this[string name]
    {
        //Im sure there are many reasons why this is terrible, but yarn variables are not serializeable so cannot be saved
        get => NPCManager.NPCVariable.ToYarnEquiv(NPCManager.singleton.data[npcName].variables[name]);
        set => NPCManager.singleton.data[npcName].variables[name] = NPCManager.NPCVariable.FromYarnEquiv(value);
    }
    [Range(0, 360)]
    public float requiredLookAngle = 180;
    public float requiredLookDot => Mathf.Cos(requiredLookAngle);
    public NPCName npcName;
    public Transform ambientThought;
    public TextMeshPro ambientThoughtText;
    new Camera camera;
    public NPCTemplate t;
    Transform[] conversationGroupOverride;
    //Conversation currentConv;
    public static Dictionary<NPCName, NPC> activeNPCs = new Dictionary<NPCName, NPC>();
    public Animator animator;
    public bool hasSpeaker = false;
    public NPCName speakingNPC;
    NPCSpawn spawn;
    public BuyMenuItem[] buyInventory;

    public Transform headPosition;

    public int currentRoutineStage;

    Transform playerTransform;

    public void InitNPC(NPCTemplate template, NPCSpawn spawn, Transform[] conversationGroupOverride)
    {
        t = template;

        this.spawn = spawn;
        //Copy the buy inventory
        buyInventory = new BuyMenuItem[t.buyMenuItems.Length];
        for (int i = 0; i < buyInventory.Length; i++)
        {
            //Copy all the data
            buyInventory[i].item = t.buyMenuItems[i].item;
            buyInventory[i].cost = t.buyMenuItems[i].cost;
            buyInventory[i].count = t.buyMenuItems[i].count;
            buyInventory[i].stock = t.buyMenuItems[i].stock;
        }

        npcName = spawn.spawnedNPCName;
        activeNPCs[npcName] = this;
        speakingNPC = npcName;
        this.conversationGroupOverride = conversationGroupOverride;
    }

    private void Awake()
    {
        camera = Camera.main;
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        if (!NPCManager.singleton.data.ContainsKey(npcName))
        {
            //Only add this data if the NPC has not existed in the save before
            NPCManager.singleton.data[npcName] = new NPCManager.NPCData(t);
        }

        //Setup starting point for routine - instant so they start in the proper place
        ChangeRoutineStage(GetRoutineIndex(TimeDayController.singleton.hour), true);
    }
    ///<summary>Get the current routine index that should be active at this time - use when time is not increasing linearly</summary>
    int GetRoutineIndex(float time)
    {
        int routineStage = -1;
        for (int i = 0; i < t.routine.Length; i++)
        {
            //Go through every stage to find the current one
            if (time < t.routine[i].endTime)
            {
                routineStage = i;
                break;
            }
        }
        //If no stage ends after hour, loop around to the first stage
        if (routineStage == -1) routineStage = 0;

        return routineStage;
    }

    public void GoToWalkingPoint(string name, System.Action onComplete = null)
    {
        GoToPosition(GetTransform(spawn.walkingPoints, name).position, onComplete);
    }
    private void Update()
    {
        ambientThought.rotation = camera.transform.rotation;

        //Test if we need to move to the next routine stage
        //Only check for state change when before final end, as it will never change after that
        if (TimeDayController.singleton.hour < t.routine[t.routine.Length - 1].endTime)
        {
            if (TimeDayController.singleton.hour > t.routine[currentRoutineStage].endTime)
            {
                ChangeRoutineStage(currentRoutineStage + 1);
            }
        }
        else if (currentRoutineStage == t.routine.Length - 1)
        {
            ChangeRoutineStage(0);
        }
    }

    public void ChangeRoutineStage(int newStage, bool instant = false)
    {
        currentRoutineStage = newStage;
        ambientThoughtText.text = t.routine[currentRoutineStage].activity.ToString();

        //Apply routine animation
        animator.SetInteger("idle_state", (int)t.routine[currentRoutineStage].animation);

        switch (t.routine[currentRoutineStage].activity)
        {
            case NPCTemplate.RoutineActivity.Stand:
                ActivateStandRoutine(instant);
                break;
        }
    }


    public void ActivateStandRoutine(bool instant)
    {
        Transform target = GetTransform(spawn.walkingPoints, t.routine[currentRoutineStage].location);
        if (target != null)
        {
            if (instant)
            {
                transform.SetPositionAndRotation(target.position, target.rotation);
            }
            else
            {
                //Rotate to target rotation on finish walking
                GoToPosition(target.position, () => transform.rotation = target.rotation);
            }
        }
        else
        {
            throw new System.Exception(string.Format("Desired routine location {0} not within walking points array", t.routine[currentRoutineStage].location));
        }

    }

    public string ConversationStartNode => t.routine[currentRoutineStage].conversationStartNode;




    public void StartNPCSpeaking(string line)
    {
        if (line == null) return;

        NPCName currentSpeaker;
        try
        {
            currentSpeaker = (NPCName)System.Enum.Parse(typeof(NPCName), line.Split(':')[0]);
        }
        catch (System.Exception ex)
        {
            print(line);
            throw ex;
        }

        if (!hasSpeaker)
        {
            activeNPCs[currentSpeaker].StartSpeaking(playerTransform.transform, true);
            hasSpeaker = true;
        }
        else if (speakingNPC != currentSpeaker)
        {
            activeNPCs[speakingNPC].StopSpeaking();
            activeNPCs[currentSpeaker].StartSpeaking(playerTransform.transform, false);
        }
        speakingNPC = currentSpeaker;
    }


    public void FinishSpeaking()
    {
        activeNPCs[speakingNPC].StopSpeaking();
        hasSpeaker = false;
    }

    IEnumerator RotatePlayerTowardsNPC(Transform playerTransform)
    {
        var dir = (transform.position - playerTransform.position);
        dir.y = 0;
        Quaternion desiredRot = Quaternion.LookRotation(dir);
        while (Quaternion.Angle(desiredRot, playerTransform.rotation) > 1f)
        {
            playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, desiredRot, Time.deltaTime * 800);
            yield return new WaitForEndOfFrame();
        }
    }


    public void StartSpeaking(Transform player, bool first)
    {
        if (!first)
            //Make the camera look at this npc
            PointCameraToSpeaker(player);
        else
            GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(playerTransform);


        //Point the player towards the currently speaking npc
        StartCoroutine(RotatePlayerTowardsNPC(player));


        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.blue;
    }
    ///<summary>Point the player's conversation camera to this npc</summary>
    void PointCameraToSpeaker(Transform player)
    {
        //Get an angle that looks from the player to the npc, at a slight offset
        GameCameras.s.conversationGroup.Transform.rotation = Quaternion.Euler(0, GetCameraAngle(player), 0);
        //Setup the transposer to recenter
        GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.CancelRecentering();
        GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = true;
    }


    public void StopSpeaking()
    {
        StopAllCoroutines();
        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.red;
    }



    public float GetCameraAngle(Transform target)
    {
        return Quaternion.LookRotation(transform.position - target.position).eulerAngles.y + 15f;
    }

    public void Interact(IInteractor interactor)
    {


        //currentConv = (interactor as Player_CharacterController).ChangeToState<Conversation>();
        playerTransform = interactor.transform;


        //Probably quicker to overrite true with true then find the value and
        //check if it is true then find it again to set it
        NPCManager.singleton.data[npcName].spokenTo = true;


        GameCameras.s.conversationGroup.Transform.position = Vector3.Lerp(playerTransform.transform.position, transform.position, 0.5f);

        int targets = Mathf.Max(2, conversationGroupOverride.Length + 1);
        //Add all targets including the player
        GameCameras.s.conversationGroup.m_Targets = new CinemachineTargetGroup.Target[targets];
        GameCameras.s.conversationGroup.m_Targets[0] = GenerateTarget(playerTransform.transform);

        if (conversationGroupOverride.Length != 0)
        {
            for (int i = 0; i < conversationGroupOverride.Length; i++)
            {
                GameCameras.s.conversationGroup.m_Targets[i + 1] = GenerateTarget(conversationGroupOverride[i]);
            }
        }
        else
        {
            GameCameras.s.conversationGroup.m_Targets[1] = GenerateTarget(transform);
        }

        GameCameras.s.conversationGroup.DoUpdate();

        //Add the variables for this NPC



        GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = false;
        //  c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(c);

        StartCoroutine(TurnToPlayer(playerTransform.transform.position));
    }



    CinemachineTargetGroup.Target GenerateTarget(Transform transform, float weight = 1, float radius = 1)
    {
        return new CinemachineTargetGroup.Target() { target = transform, weight = weight, radius = radius };
    }



    IEnumerator TurnToPlayer(Vector3 playerPosition)
    {
        var dir = playerPosition - transform.position;
        dir.y = 0;

        Quaternion desiredRot = Quaternion.LookRotation(dir);
        while (Quaternion.Angle(desiredRot, transform.rotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, Time.deltaTime * 800);
            yield return new WaitForEndOfFrame();
        }

        print("Finished rotating");

    }




    public Transform GetTransform(Transform[] transforms, string name) => transforms.FirstOrDefault(t => t.name == name);
    public Transform GetFocusPoint(string name) => GetTransform(spawn.focusPoints, name);



    void ResetCamera()
    {
        GameCameras.s.cutsceneCamera.LookAt = GameCameras.s.conversationGroup.Transform;
    }


    IEnumerator LerpCameraToPositionAndRotation(Vector3 targetPosition, Quaternion targetRotation, float time)
    {
        //Orbit around the focus point
        Vector3 vel = Vector3.zero;
        float angleVel = 0;
        float delta;
        float t = 0;
        float elapsedTime = 0;
        Quaternion startRot = camera.transform.rotation;
        do
        {
            elapsedTime += Time.deltaTime * 0.25f;
            delta = Quaternion.Angle(camera.transform.rotation, targetRotation);
            t = Mathf.SmoothDampAngle(t, 1, ref angleVel, time, Mathf.Infinity, Time.deltaTime);

            camera.transform.SetPositionAndRotation(
                Vector3.SmoothDamp(camera.transform.position, targetPosition, ref vel, time, Mathf.Infinity, Time.deltaTime),
                Quaternion.Slerp(startRot, targetRotation, t)
            );

            yield return new WaitForEndOfFrame();
        } while ((delta > 0.1 || (camera.transform.position - targetPosition).sqrMagnitude > 0.01f) && elapsedTime < time);

        camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
    }



    public void OnStartHighlight()
    {
        //show arrow
        UIController.singleton.npcIndicator.StartIndication(
            transform,
            NPCManager.singleton.data[npcName].spokenTo ? npcName.ToString() : "", //Do not show the npc name if the player has never spoken to them
            Vector3.up * 2);
    }

    public void OnEndHighlight()
    {
        //remove arrow
        UIController.singleton.npcIndicator.EndIndication();
    }
}
