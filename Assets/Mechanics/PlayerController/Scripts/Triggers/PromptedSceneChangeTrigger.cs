using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.PlayerController;
using Yarn;
using Yarn.Unity;
using UnityEngine.AddressableAssets;

public class PromptedSceneChangeTrigger : SceneConnector, IDialogue, IVariableAddon
{
	public AssetReferenceT<YarnProgram> dialogue;

	public string startNode;


	public Value this[string name]
	{
		get
		{
			switch (name)
			{
				case "ToLevel":
					return new Value(changeToLevel.ToString());
				default:
					return null;
			};
		}
		set { }
	}

	public string prefix => "$Scene_";

	AssetReferenceT<YarnProgram> IDialogue.Dialogue => dialogue;

	string IDialogue.StartNode => startNode;

	PlayerController p;
	Dialogue<DialogueTemplate> d;
	public DialogueTemplate dialogueTemplate;
	public override void OnPlayerTrigger(PlayerController player)
	{
		d = (Dialogue<DialogueTemplate>)player.machine.ChangeToState(dialogueTemplate.Interact(this));
		p = player; //hold for if player confirms
		print("Entered scene change prompt");

	}

	public void ConfirmLoad(string[] arg)
	{

		ResetTriggerTimer();
		StartSceneChange(p);
	}
	public void CancelLoad(string[] arg)
	{
		ResetTriggerTimer();
		p = null;
		d = null;
	}

	public void SetupCommands(DialogueRunner runner)
	{
		runner.AddCommandHandler("ConfirmLoad", ConfirmLoad);
		runner.AddCommandHandler("CancelLoad", CancelLoad);
	}
	public void RemoveCommands(DialogueRunner runner)
	{
		runner.RemoveCommandHandler("ConfirmLoad");
		runner.RemoveCommandHandler("CancelLoad");
	}

	public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
	{
		yield return new KeyValuePair<string, Value>("ToLevel", new Value(changeToLevel.ToString()));
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
