using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn;
using TMPro;
using System.Linq;


public class NPC : AIBase, IInteractable, IVariableAddon, IDialogue
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
    public Transform[] conversationGroupOverride;
    //Conversation currentConv;
    public static Dictionary<NPCName, NPC> activeNPCs = new Dictionary<NPCName, NPC>();
    public Animator animator;

    NPCSpawn spawn;
    public BuyMenuItem[] buyInventory;

    public Transform headPosition;

    public int currentRoutineStage;

    Transform playerTransform;
    public YarnProgram Dialogue => t.dialogue;

    public int RoutineIndex { get => NPCManager.singleton.data[npcName].routineIndex; set => NPCManager.singleton.data[npcName].routineIndex = value; }

    public NPCTemplate.Routine CurrentRoutine => t.routines[RoutineIndex];
    public string StartNode => CurrentRoutine.stages[currentRoutineStage].conversationStartNode;

    public string interactionDescription => "Talk";

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

        this.conversationGroupOverride = conversationGroupOverride;



        if (!NPCManager.singleton.data.ContainsKey(npcName))
        {
            //Only add this data if the NPC has not existed in the save before
            NPCManager.singleton.data[npcName] = new NPCManager.NPCData(t);
        }

        //Setup starting point for routine - instant so they start in the proper place
        ChangeRoutineStage(t.routines[RoutineIndex].GetRoutineStageIndex(TimeDayController.singleton.hour), true);
    }

    private void Awake()
    {
        camera = Camera.main;
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();



        QuestManager.singleton.onQuestComplete += OnQuestComplete;
    }
    private void OnDestroy()
    {
        QuestManager.singleton.onQuestComplete -= OnQuestComplete;
    }
    public void OnQuestComplete(Quest quest)
    {
        //Update all of the indexes
        RoutineIndex = t.GetRoutineIndex();
    }



    public void GoToWalkingPoint(string name, System.Action onComplete = null)
    {
        GoToPosition(GetTransform(spawn.walkingPoints, name).position, onComplete);
    }

    private void Update()
    {
        if (inited)
        {
            ambientThought.rotation = camera.transform.rotation;

            //Test if we need to move to the next routine stage
            //Only check for state change when before final end, as it will never change after that
            if (TimeDayController.singleton.hour < CurrentRoutine.stages[CurrentRoutine.stages.Length - 1].endTime)
            {
                if (TimeDayController.singleton.hour > CurrentRoutine.stages[currentRoutineStage].endTime)
                {
                    ChangeRoutineStage(currentRoutineStage + 1);
                }
            }
            else if (currentRoutineStage == CurrentRoutine.stages.Length - 1)
            {
                ChangeRoutineStage(0);
            }
        }
    }

    public void ChangeRoutineStage(int newStage, bool instant = false)
    {
        currentRoutineStage = newStage;
        ambientThoughtText.text = CurrentRoutine.stages[currentRoutineStage].activity.ToString();

        //Apply routine animation
        animator.SetInteger("idle_state", (int)CurrentRoutine.stages[currentRoutineStage].animation);

        switch (CurrentRoutine.stages[currentRoutineStage].activity)
        {
            case NPCTemplate.RoutineActivity.Stand:
                ActivateStandRoutine(instant);
                break;
        }
    }

    public void ActivateStandRoutine(bool instant)
    {
        Transform target = GetTransform(spawn.walkingPoints, CurrentRoutine.stages[currentRoutineStage].location);
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
            throw new System.Exception(string.Format("Desired routine location {0} not within walking points array", CurrentRoutine.stages[currentRoutineStage].location));
        }
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

    public void StartSpeaking(Transform player)
    {
        //Point the player towards the currently speaking npc
        StartCoroutine(RotatePlayerTowardsNPC(player));


        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.blue;
    }

    public void StopSpeaking()
    {
        StopAllCoroutines();
        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.red;
    }

    public void Interact(IInteractor interactor)
    {


        //currentConv = (interactor as Player_CharacterController).ChangeToState<Conversation>();
        playerTransform = interactor.transform;


        //Probably quicker to overrite true with true then find the value and
        //check if it is true then find it again to set it
        NPCManager.singleton.data[npcName].spokenTo = true;


        StartCoroutine(TurnToPlayer(playerTransform.transform.position));
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

    public Transform GetTransform(Transform[] transforms, string name) => transforms.First(t => t.name == name);
    public Transform GetFocusPoint(string name) => GetTransform(spawn.focusPoints, name);



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

    public void SetupCommands(DialogueRunner runner)
    {

    }

    public void RemoveCommands(DialogueRunner runner)
    {

    }
}
