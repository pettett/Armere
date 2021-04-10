
using UnityEngine;
using Yarn.Unity;

public interface IDialogue
{
	YarnProgram Dialogue { get; }
	string StartNode { get; }
	Transform transform { get; }

	void SetupCommands(DialogueRunner runner);
	void RemoveCommands(DialogueRunner runner);
}