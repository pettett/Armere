using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerController;
using Yarn.Unity;

using Cinemachine;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCName npcName;
    public Transform ambientThought;
    Camera camera;
    DialogueRunner runner;
    NPCTemplate t;
    Transform[] conversationGroupOverride;
    public static Dictionary<NPCName, NPC> activeNPCs = new Dictionary<NPCName, NPC>();

    public Animator animator;
    bool hasSpeaker = false;
    NPCName speakingNPC;

    public void InitNPC(NPCTemplate template, NPCName name, Transform[] conversationGroupOverride)
    {
        t = template;
        runner.Add(t.dialogue);
        npcName = name;
        activeNPCs[npcName] = this;
        speakingNPC = name;
        this.conversationGroupOverride = conversationGroupOverride;
    }
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        camera = Camera.main;

        runner = GetComponent<DialogueRunner>();
        runner.AddCommandHandler("quest", GiveQuest);
        runner.AddCommandHandler("ProgressQuest", ProgressQuest);
        runner.AddCommandHandler("cameraPan", CameraPan);
        runner.AddCommandHandler("GiveItems", GiveItems);
        runner.AddCommandHandler("TurnPlayerToTarget", TurnPlayerToTarget);
        runner.AddCommandHandler("TurnNPCToTarget", TurnNPCToTarget);
        runner.AddCommandHandler("TurnNPCAndPlayerToTarget", TurnNPCAndPlayerToTarget);
        runner.AddCommandHandler("Animate", Animate);

        runner.variableStorage = DialogueInstances.singleton.inMemoryVariableStorage;
        runner.dialogueUI = DialogueInstances.singleton.dialogueUI;
    }

    public void StartNPCSpeaking(string line)
    {
        var currentSpeaker = (NPCName)System.Enum.Parse(typeof(NPCName), line.Split(':')[0]);
        if (!hasSpeaker)
        {
            activeNPCs[currentSpeaker].StartSpeaking(c, true);
            hasSpeaker = true;
        }
        else if (speakingNPC != currentSpeaker)
        {
            activeNPCs[speakingNPC].StopSpeaking();
            activeNPCs[currentSpeaker].StartSpeaking(c, false);
        }
        speakingNPC = currentSpeaker;
    }




    IEnumerator RotatePlayerTowardsNPC(Transform playerTransform)
    {
        Quaternion desiredRot = Quaternion.LookRotation(transform.position - playerTransform.position);
        while (Quaternion.Angle(desiredRot, playerTransform.rotation) > 1f)
        {
            playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, desiredRot, Time.deltaTime * 800);
            yield return new WaitForEndOfFrame();
        }
    }


    public void StartSpeaking(Player_CharacterController c, bool first)
    {
        if (!first)
            //Make the camera look at this npc
            PointCameraToSpeaker(c);
        else
            c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(c);


        //Point the player towards the currently speaking npc
        StartCoroutine(RotatePlayerTowardsNPC(c.transform));


        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.blue;
    }
    ///<summary>Point the player's conversation camera to this npc</summary>
    void PointCameraToSpeaker(Player_CharacterController c)
    {
        //Get an angle that looks from the player to the npc, at a slight offset
        c.conversationGroup.Transform.rotation = Quaternion.Euler(0, GetCameraAngle(c), 0);
        //Setup the transposer to recenter
        c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.CancelRecentering();
        c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = true;
    }


    public void StopSpeaking()
    {
        StopAllCoroutines();
        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var mat in r.materials)
                mat.color = Color.red;
    }

    Player_CharacterController c;

    public float GetCameraAngle(Player_CharacterController c)
    {
        return Quaternion.LookRotation(transform.position - c.transform.position).eulerAngles.y + 15f;
    }

    public void Interact(Player_CharacterController c)
    {
        this.c = c;
        c.ChangeToState<Conversation>();

        c.conversationGroup.Transform.position = Vector3.Lerp(c.transform.position, transform.position, 0.5f);

        int targets = Mathf.Max(2, conversationGroupOverride.Length + 1);
        //Add all targets including the player
        c.conversationGroup.m_Targets = new CinemachineTargetGroup.Target[targets];
        c.conversationGroup.m_Targets[0] = GenerateTarget(c.transform);
        if (conversationGroupOverride.Length != 0)
        {
            for (int i = 0; i < conversationGroupOverride.Length; i++)
            {
                c.conversationGroup.m_Targets[i + 1] = GenerateTarget(conversationGroupOverride[i]);
            }
        }
        else
        {
            c.conversationGroup.m_Targets[1] = GenerateTarget(transform);
        }

        c.conversationGroup.DoUpdate();

        c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = false;
        //  c.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = GetCameraAngle(c);

        StartCoroutine(TurnToPlayer(c.transform.position));
    }

    CinemachineTargetGroup.Target GenerateTarget(Transform transform, float weight = 1, float radius = 1)
    {
        return new CinemachineTargetGroup.Target() { target = transform, weight = weight, radius = radius };
    }



    Vector3 focusPoint;
    IEnumerator TurnToPlayer(Vector3 playerPosition)
    {
        DialogueUI.singleton.onLineStart.AddListener(StartNPCSpeaking);
        DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
        runner.StartDialogue("Start");

        Quaternion desiredRot = Quaternion.LookRotation(playerPosition - transform.position);
        while (Quaternion.Angle(desiredRot, transform.rotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, Time.deltaTime * 800);
            yield return new WaitForEndOfFrame();
        }


    }

    void OnDialogueComplete()
    {
        activeNPCs[speakingNPC].StopSpeaking();
        hasSpeaker = false;
        c.ChangeToState<Walking>();
        DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
        DialogueUI.singleton.onLineStart.RemoveListener(StartNPCSpeaking);
        ResetCamera();
    }


    void Animate(string[] arg, System.Action onComplete)
    {
        string animName = arg[0];
        if (arg.Length > 1)
        {
            bool wait = bool.Parse(arg[1]);
        }
        animator.SetTrigger(animName);
        onComplete?.Invoke();
    }

    void GiveQuest(string[] arg)
    {
        string questName = arg[0];
        for (int i = 0; i < t.quests.Length; i++)
        {
            if (t.quests[i].name == questName)
            {
                QuestManager.AddQuest(t.quests[i]);
            }
        }
    }
    void ProgressQuest(string[] arg)
    {
        string questName = arg[0];
        QuestManager.ProgressQuest(questName);
    }
    private void CameraPan(string[] arg, System.Action onComplete)
    {
        //pan the camera to the target destination
        StartCoroutine(TurnCameraToTarget(t.focusPoints[arg[0]], onComplete));
    }


    void TurnPlayerToTarget(string[] arg, System.Action onComplete)
    {
        Vector3 target = t.focusPoints[arg[0]];
        target.y = c.transform.position.y;
        c.transform.LookAt(target);
        onComplete?.Invoke();
    }
    void TurnNPCToTarget(string[] arg, System.Action onComplete)
    {
        Vector3 target = t.focusPoints[arg[0]];
        target.y = transform.position.y;
        transform.LookAt(target);
        onComplete?.Invoke();
    }
    void TurnNPCAndPlayerToTarget(string[] arg, System.Action onComplete)
    {
        Vector3 target = t.focusPoints[arg[0]];
        target.y = transform.position.y;
        transform.LookAt(target);
        target.y = c.transform.position.y;
        c.transform.LookAt(target);
        onComplete?.Invoke();
    }

    void GiveItems(string[] arg, System.Action onComplete)
    {
        ItemName item = (ItemName)System.Enum.Parse(typeof(ItemName), arg[0]);
        int count;
        if (arg.Length == 1)
            count = 1;
        else
            count = int.Parse(arg[1]);

        //give [count] items of type [item]
        NewItemPrompt.singleton.ShowPrompt(item, onComplete);
    }

    IEnumerator TurnCameraToTarget(Vector3 target, System.Action onComplete)
    {
        //Orbit around the focus point
        //
        Quaternion targetRotation = Quaternion.LookRotation((target - focusPoint + Vector3.up));

        //Vector3 pos = focusPoint + Vector3.up + targetRotation * Vector3.back * 2;

        //yield return LerpCameraToPositionAndRotation(pos, targetRotation, 0.3f);

        c.cutsceneCamera.LookAt = c.lookAtTarget;
        c.lookAtTarget.position = target;

        yield return new WaitForSeconds(0.5f);

        onComplete?.Invoke();
    }

    void ResetCamera()
    {
        c.cutsceneCamera.LookAt = c.conversationGroup.Transform;
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


    private void Update()
    {
        ambientThought.rotation = camera.transform.rotation;
    }
}
