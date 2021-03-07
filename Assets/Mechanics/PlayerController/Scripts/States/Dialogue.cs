using System.Collections;
using System.Collections.Generic;
using Armere.PlayerController;
using UnityEngine;
using Yarn.Unity;
using Cinemachine;


//Base class for basic functionality related to yarn
//Should contain - setup for yarn programs
//Camera control
public class Dialogue<TemplateT> : MovementState<TemplateT> where TemplateT : DialogueTemplate
{
	public override string StateName => "In Dialogue";
	protected DialogueRunner runner => c.runner;
	protected readonly IDialogue dialogue;



	const float cameraAngleOffset = 15f;

	public Dialogue(PlayerController c, TemplateT t) : base(c, t)
	{
		dialogue = t.dialogue;
		if (dialogue != null)
		{
			//Point camera towards the dialogue object
			GameCameras.s.conversationGroup.m_Targets = new CinemachineTargetGroup.Target[2];
			GameCameras.s.conversationGroup.m_Targets[0] = GenerateTarget(transform);
			GameCameras.s.conversationGroup.m_Targets[1] = GenerateTarget(dialogue.transform);
			PointCameraToSpeaker(dialogue.transform);

			if (dialogue is IVariableAddon a)
			{
				(runner.variableStorage as InMemoryVariableStorage).addons.Add(a);
			}

			StartDialogue(dialogue);
		}
		else
		{
			throw new System.Exception("Dialogue target must be assigned");
		}
	}

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
		GameCameras.s.conversationCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.CancelRecentering();
		GameCameras.s.conversationCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_RecenterToTargetHeading.m_enabled = true;
	}

	public void StartDialogue(IDialogue d)
	{
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
		c.ChangeToState(c.defaultState);
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
