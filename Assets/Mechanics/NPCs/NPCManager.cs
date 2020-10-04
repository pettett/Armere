using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Yarn.Unity;
public class NPCManager : MonoBehaviour
{
    public static NPCManager singleton;
    public DialogueRunner dialogueRunner;

    private void Awake()
    {
        if (singleton != null)
            Destroy(gameObject);
        else
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;



            dialogueRunner.variableStorage = DialogueInstances.singleton.inMemoryVariableStorage;
            dialogueRunner.dialogueUI = DialogueInstances.singleton.dialogueUI;

        }
    }




    [System.Serializable]
    public class NPCSaveData : Dictionary<NPCName, NPCData>
    {
        public NPCSaveData() : base() { }
        protected NPCSaveData(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [System.Serializable]
    public class NPCVariable
    {
        public object value;
        public Yarn.Value.Type type;

        public static Yarn.Value ToYarnEquiv(NPCVariable v)
        {
            if (v != null) return new Yarn.Value(v.value);
            else return null;
        }
        public static NPCVariable FromYarnEquiv(Yarn.Value v)
        {
            switch (v.type)
            {
                case Yarn.Value.Type.Bool: //BOOL
                    return new NPCVariable() { value = v.AsBool, type = Yarn.Value.Type.Bool };
                case Yarn.Value.Type.Number: //FLOAT
                    return new NPCVariable() { value = v.AsNumber, type = Yarn.Value.Type.Number };
                case Yarn.Value.Type.String: //STRING
                    return new NPCVariable() { value = v.AsString, type = Yarn.Value.Type.String };
            }
            return null;
        }
    }


    [System.Serializable]
    public class NPCData
    {
        public bool spokenTo = false;
        public Dictionary<string, NPCVariable> variables;
        public int routineIndex = 0;


        public NPCData(NPCTemplate t)
        {
            variables = new Dictionary<string, NPCVariable>(t.defaultValues.Length);
            foreach (var variable in t.defaultValues)
            {
                //Turn default variable into yarn value then into NPCVariable
                variables[variable.name] = NPCVariable.FromYarnEquiv(Yarn.Unity.InMemoryVariableStorage.AddDefault(variable));
            }

            //Set the default index from active quests
            routineIndex = t.GetRoutineIndex();
        }

    }

    [HideInInspector] public NPCSaveData data = new NPCSaveData();


}
