using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Yarn.Unity;
public class NPCManager : MonoBehaviour
{
	public static NPCManager singleton;
	public DialogueRunner dialogueRunner;
	public SaveLoadEventChannel npcSaveLoadChannel;
	private void Awake()
	{
		if (singleton != null)
			Destroy(gameObject);
		else
		{
			singleton = this;



			dialogueRunner.variableStorage = DialogueInstances.singleton.inMemoryVariableStorage;
			dialogueRunner.dialogueUI = DialogueInstances.singleton.dialogueUI;

			npcSaveLoadChannel.onLoadBinEvent += LoadBin;
			npcSaveLoadChannel.onLoadBlankEvent += LoadBlank;
			npcSaveLoadChannel.onSaveBinEvent += SaveBin;
		}
	}

	private void OnDestroy()
	{
		if (singleton == this)
		{
			npcSaveLoadChannel.onLoadBinEvent -= LoadBin;
			npcSaveLoadChannel.onLoadBlankEvent -= LoadBlank;
			npcSaveLoadChannel.onSaveBinEvent -= SaveBin;

			singleton = null;
		}
	}


	public void LoadBin(in GameDataReader reader)
	{
		int dataCount = reader.ReadInt();
		data = new Dictionary<string, NPCData>(dataCount);
		for (int i = 0; i < dataCount; i++)
		{
			data[reader.ReadString()] = new NPCData(reader.saveVersion, reader);
		}
	}
	public void SaveBin(in GameDataWriter writer)
	{
		//Debug.Log(writer.writer.BaseStream.Position);

		writer.Write(data.Count);
		foreach (KeyValuePair<string, NPCData> kvp in data)
		{
			writer.Write(kvp.Key);
			writer.Write(kvp.Value.routineIndex);
			writer.Write(kvp.Value.spokenTo);
			writer.Write(kvp.Value.variables.Count);
			foreach (KeyValuePair<string, NPCVariable> kvp2 in kvp.Value.variables)
			{
				writer.Write(kvp2.Key);
				writer.Write((int)kvp2.Value.type);
				switch (kvp2.Value.type)
				{
					case Yarn.Value.Type.Bool:
						writer.Write((bool)kvp2.Value.value);
						break;
					case Yarn.Value.Type.String:
						writer.Write((string)kvp2.Value.value);
						break;
					case Yarn.Value.Type.Number:
						writer.Write((float)kvp2.Value.value);
						break;
				}
			}
		}
	}


	public void LoadBlank()
	{

	}




	[System.Serializable]
	public class NPCVariable
	{
		public object value;
		public Yarn.Value.Type type;

		public static implicit operator Yarn.Value(NPCVariable v)
		{
			if (v != null) return new Yarn.Value(v.value);
			else return null;
		}
		public static implicit operator NPCVariable(Yarn.Value v)
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

		public static NPCVariable FromLoad(Version saveVersion, GameDataReader reader)
		{
			var type = (Yarn.Value.Type)reader.ReadInt();
			switch (type)
			{
				case Yarn.Value.Type.Bool: //BOOL
					return new NPCVariable() { value = reader.ReadBool(), type = Yarn.Value.Type.Bool };
				case Yarn.Value.Type.Number: //FLOAT
					return new NPCVariable() { value = reader.ReadFloat(), type = Yarn.Value.Type.Number };
				case Yarn.Value.Type.String: //STRING
					return new NPCVariable() { value = reader.ReadString(), type = Yarn.Value.Type.String };
			}
			return null;
		}
	}


	public class NPCData
	{
		public bool spokenTo = false;
		public Dictionary<string, NPCVariable> variables;
		public int routineIndex = 0;
		public Transform npcInstance;

		public NPCData(Version saveVersion, GameDataReader reader)
		{
			routineIndex = reader.ReadInt();
			spokenTo = reader.ReadBool();
			int varCount = reader.ReadInt();
			variables = new Dictionary<string, NPCVariable>(varCount);
			for (int i = 0; i < varCount; i++)
			{
				variables[reader.ReadString()] = NPCVariable.FromLoad(saveVersion, reader);
			}
		}

		public NPCData(NPCTemplate t)
		{
			variables = new Dictionary<string, NPCVariable>(t.defaultValues.Length);
			foreach (var variable in t.defaultValues)
			{
				//Turn default variable into yarn value then into NPCVariable
				variables[variable.name] = Yarn.Unity.InMemoryVariableStorage.AddDefault(variable);
			}

			//Set the default index from active quests
			routineIndex = t.GetRoutineIndex();
		}
	}

	[System.NonSerialized] public Dictionary<string, NPCData> data = new Dictionary<string, NPCData>();


}
