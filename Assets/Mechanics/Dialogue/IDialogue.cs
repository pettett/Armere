
using UnityEngine;
using Yarn.Unity;
using UnityEngine.AddressableAssets;
public interface IDialogue
{
	AssetReferenceT<YarnProgram> Dialogue { get; }
	string StartNode { get; }
	Transform transform { get; }

	void SetupCommands(DialogueRunner runner);
	void RemoveCommands(DialogueRunner runner);
}