using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yarn.Unity
{
    public class DialogueInstances : MonoBehaviour
    {
        public static DialogueInstances singleton;
        public DialogueUI dialogueUI;
        public InMemoryVariableStorage inMemoryVariableStorage;
        private void Awake()
        {
            print("Set singleton");
            singleton = this;
        }
    }
}

