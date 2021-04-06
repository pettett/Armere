using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yarn.Unity
{
	public class DialogueInstances : MonoBehaviour
	{
		public static DialogueInstances singleton;
		public DialogueRunner runner;
		public DialogueUI dialogueUI;
		public InMemoryVariableStorage inMemoryVariableStorage;
		private void Awake()
		{
			singleton = this;

			runner.variableStorage = inMemoryVariableStorage;
			runner.dialogueUI = dialogueUI;
		}
	}
}

