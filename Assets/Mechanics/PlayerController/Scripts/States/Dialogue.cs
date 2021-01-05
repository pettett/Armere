using System.Collections;
using System.Collections.Generic;
using Armere.PlayerController;
using UnityEngine;
using Yarn.Unity;
using Cinemachine;


//Base class for basic functionality related to yarn
//Should contain - setup for yarn programs
//Camera control
public class Dialogue : MovementState
{
    public override string StateName => "In Dialogue";
    protected DialogueRunner runner => c.runner;
    protected IDialogue dialogue;

    // Start is called before the first frame update
    public override void Start(params object[] args)
    {
        if (args[0] is IDialogue d)
        {
            //Point camera towards the dialogue object
            GameCameras.s.conversationGroup.m_Targets = new CinemachineTargetGroup.Target[2];
            GameCameras.s.conversationGroup.m_Targets[0] = GenerateTarget(transform);
            GameCameras.s.conversationGroup.m_Targets[1] = GenerateTarget(d.transform);
            PointCameraToSpeaker(d.transform);

            if (d is IVariableAddon a)
            {
                (runner.variableStorage as InMemoryVariableStorage).addons.Add(a);
            }

            StartDialogue(d);
        }
        else
        {
            throw new System.Exception("First arg must be dialogue");
        }
    }

    const float cameraAngleOffset = 15f;

    public float GetCameraAngle(Transform target)
    {
        return Quaternion.LookRotation(target.position - transform.position).eulerAngles.y + cameraAngleOffset;
    }

    ///<summary>re-Point the player's conversation camera to this target</summary>
    public void PointCameraToSpeaker(Transform target)
    {
        //Get an angle that looks from the player to the target, at a slight offset
        GameCameras.s.conversationGroup.Transform.rotation = Quaternion.Euler(0, GetCameraAngle(target), 0);
        //Setup the transposer to recenter
        GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.CancelRecentering();
        GameCameras.s.cutsceneCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = true;
    }

    public void StartDialogue(IDialogue d)
    {
        dialogue = d;
        //Setup PlayerController for static conversation
        GameCameras.s.lockingMouse = false;
        c.rb.velocity = Vector3.zero;
        GameCameras.s.DisableControl();
        c.rb.isKinematic = true;
        GameCameras.s.EnableCutsceneCamera();


        GameCameras.s.conversationGroup.Transform.position = Vector3.Lerp(transform.position, dialogue.transform.position, 0.5f);



        SetupRunner();
    }

    public static CinemachineTargetGroup.Target GenerateTarget(Transform transform, float weight = 1, float radius = 1)
    {
        return new CinemachineTargetGroup.Target() { target = transform, weight = weight, radius = radius };
    }

    public virtual void SetupRunner()
    {
        runner.Add(dialogue.Dialogue);
        DialogueUI.singleton.onLineStart.AddListener(OnLineStart);
        DialogueUI.singleton.onDialogueEnd.AddListener(OnDialogueComplete);
        runner.StartDialogue(dialogue.StartNode);

        dialogue.SetupCommands(runner);
    }

    public virtual void CleanUpRunner()
    {
        //Remove all commands from the runner as well as removing dialogue
        runner.Stop();
        runner.Clear();
        runner.ClearStringTable();


        DialogueUI.singleton.onLineStart.RemoveListener(OnLineStart);
        DialogueUI.singleton.onDialogueEnd.RemoveListener(OnDialogueComplete);

        dialogue.RemoveCommands(runner);
    }

    public virtual void OnLineStart(string line)
    {

    }

    public virtual void OnDialogueComplete()
    {
        c.ChangeToState<Walking>();
    }

    // Update is called once per frame
    public override void Update()
    {

    }
    public override void End()
    {
        CleanUpRunner();

        GameCameras.s.lockingMouse = true;
        c.rb.isKinematic = false;
        GameCameras.s.EnableControl();
        GameCameras.s.DisableCutsceneCamera();
    }
}
