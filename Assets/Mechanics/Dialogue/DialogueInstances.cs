using UnityEngine;
using Yarn.Unity;
public class DialogueInstances : MonoBehaviour
{
	public DialogueRunner runner;
	public ArmereDialogueUI ui;
	public VariableStorage variableStorage;

	public static DialogueInstances singleton;

	private void Awake()
	{
		singleton = this;
	}
}