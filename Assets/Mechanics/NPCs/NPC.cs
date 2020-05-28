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



    public static Dictionary<NPCName, NPC> activeNPCs = new Dictionary<NPCName, NPC>();

    public void InitNPC(NPCTemplate template, NPCName name)
    {
        t = template;
        runner.Add(t.dialogue);
        npcName = name;
        activeNPCs[npcName] = this;
        speakingNPC = name;
    }

    private void Awake()
    {
        camera = Camera.main;



        runner = GetComponent<Yarn.Unity.DialogueRunner>();
        runner.AddCommandHandler("quest", GiveQuest);
        runner.AddCommandHandler("ProgressQuest", ProgressQuest);
        runner.AddCommandHandler("cameraPan", CameraPan);
        runner.AddCommandHandler("GiveItems", GiveItems);
        runner.AddCommandHandler("TurnPlayerToTarget", TurnPlayerToTarget);
        runner.AddCommandHandler("TurnNPCToTarget", TurnNPCToTarget);
        runner.AddCommandHandler("TurnNPCAndPlayerToTarget", TurnNPCAndPlayerToTarget);

        runner.variableStorage = DialogueInstances.singleton.inMemoryVariableStorage;
        runner.dialogueUI = DialogueInstances.singleton.dialogueUI;
    }
    bool hasSpeaker = false;
    NPCName speakingNPC;

    public void StartNPCSpeaking(string line)
    {
        var currentSpeaker = (NPCName)System.Enum.Parse(typeof(NPCName), line.Split(':')[0]);
        if (!hasSpeaker)
        {
            activeNPCs[currentSpeaker].StartSpeaking(c);
            hasSpeaker = true;
        }
        else if (speakingNPC != currentSpeaker)
        {
            activeNPCs[speakingNPC].StopSpeaking();
            activeNPCs[currentSpeaker].StartSpeaking(c);
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


    public void StartSpeaking(Player_CharacterController c)
    {
        c.conversationGroup.m_Targets[1].target = transform;


        StartCoroutine(RotatePlayerTowardsNPC(c.transform));

        c.conversationGroup.Transform.rotation = Quaternion.LookRotation(c.transform.position - transform.position) * Quaternion.Euler(0, 90, 0);

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

    Player_CharacterController c;
    public void Interact(Player_CharacterController c)
    {
        this.c = c;
        c.ChangeToState<Conversation>();
        c.conversationGroup.Transform.position = Vector3.Lerp(c.transform.position, transform.position, 0.5f);
        StartCoroutine(TurnToPlayer(c.transform.position));
    }
    Vector3 focusPoint;
    IEnumerator TurnToPlayer(Vector3 playerPosition)
    {

        Quaternion desiredRot = Quaternion.LookRotation(playerPosition - transform.position);
        while (Quaternion.Angle(desiredRot, transform.rotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, Time.deltaTime * 800);
            yield return new WaitForEndOfFrame();
        }

        DialogueUI.singleton.onLineStart.AddListener(StartNPCSpeaking);
        DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
        runner.StartDialogue("Start");
    }

    void OnDialogueComplete()
    {
        activeNPCs[speakingNPC].StopSpeaking();
        hasSpeaker = false;
        c.ChangeToState<Player_CharacterController.Walking>();
        DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);
        DialogueUI.singleton.onLineStart.RemoveListener(StartNPCSpeaking);
        ResetCamera();
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
