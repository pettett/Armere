using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Yarn.Unity;

[CreateAssetMenu(fileName = "NPC Manager", menuName = "Game/NPCs/Manager", order = 0)]
public class NPCManager : SaveableSO
{
	public static NPCManager singleton;
	private void OnEnable()
	{

		singleton = this;
	}

	private void OnDisable()
	{

		singleton = null;

	}


	public override void LoadBin(in GameDataReader reader)
	{
		int dataCount = reader.ReadInt();
		data = new Dictionary<string, NPCData>(dataCount);
		for (int i = 0; i < dataCount; i++)
		{
			data[reader.ReadString()] = new NPCData(reader.saveVersion, reader);
		}
	}
	public override void SaveBin(in GameDataWriter writer)
	{
		//Debug.Log(writer.writer.BaseStream.Position);

		writer.WritePrimitive(data.Count);
		foreach (KeyValuePair<string, NPCData> kvp in data)
		{
			writer.WritePrimitive(kvp.Key);
			writer.WritePrimitive(kvp.Value.routineIndex);
			writer.WritePrimitive(kvp.Value.spokenTo);
			writer.WritePrimitive(kvp.Value.variables.Count);
			foreach (KeyValuePair<string, Yarn.Value> kvp2 in kvp.Value.variables)
			{
				writer.WritePrimitive(kvp2.Key);
				writer.WritePrimitive((int)kvp2.Value.type);
				switch (kvp2.Value.type)
				{
					case Yarn.Value.Type.Bool:
						writer.WritePrimitive(kvp2.Value.AsBool);
						break;
					case Yarn.Value.Type.String:
						writer.WritePrimitive(kvp2.Value.AsString);
						break;
					case Yarn.Value.Type.Number:
						writer.WritePrimitive(kvp2.Value.AsNumber);
						break;
				}
			}
		}
	}

	public static Yarn.Value FromLoad(Version saveVersion, GameDataReader reader)
	{
		return (Yarn.Value.Type)reader.ReadInt() switch
		{
			Yarn.Value.Type.Bool => new Yarn.Value(reader.ReadBool()),
			Yarn.Value.Type.Number => new Yarn.Value(reader.ReadFloat()),
			Yarn.Value.Type.String => new Yarn.Value(reader.ReadString()),
			_ => null,
		};
	}
	public override void LoadBlank()
	{

	}
	public class NPCData
	{
		public bool spokenTo = false;
		public Dictionary<string, Yarn.Value> variables;
		public int routineIndex = 0;
		public Transform npcInstance;

		public NPCData(Version saveVersion, GameDataReader reader)
		{
			routineIndex = reader.ReadInt();
			spokenTo = reader.ReadBool();
			int varCount = reader.ReadInt();
			variables = new Dictionary<string, Yarn.Value>(varCount);
			for (int i = 0; i < varCount; i++)
			{
				variables[reader.ReadString()] = FromLoad(saveVersion, reader);
			}
		}
		public void AddVariable(string name, object value)
		{
			variables[name] = new Yarn.Value(value);
		}
		public NPCData(NPCTemplate t)
		{
			variables = new Dictionary<string, Yarn.Value>(t.defaultValues.Length);
			foreach (var variable in t.defaultValues)
			{
				//Turn default variable into yarn value then into NPCVariable
				variables[variable.name] = VariableStorage.AddDefault(variable);
			}

			//Set the default index from active quests
			routineIndex = t.GetRoutineIndex();
		}
	}

	[System.NonSerialized] public Dictionary<string, NPCData> data = new Dictionary<string, NPCData>();


}
