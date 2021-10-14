using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct EntityInteractionData : IBinaryVariableSerializer<EntityInteractionData>
{
	public uint kills;
	public uint deaths;
	public string Format(string name)
	{
		return $"Killed {name} {kills} times, died {deaths} times";
	}

	public EntityInteractionData Read(in GameDataReader reader)
	{
		kills = reader.ReadUInt();
		deaths = reader.ReadUInt();
		return this;
	}

	public void Write(in GameDataWriter writer)
	{
		writer.WritePrimitive(kills);
		writer.WritePrimitive(deaths);
	}
}


[CreateAssetMenu(menuName = "Game/Statistics")]
public class Statistics : SaveableSO
{
	static readonly Dictionary<string, object> stats = new Dictionary<string, object>();

	public void IncrementIntStat(string name)
	{
		EditStat<int>(name, (ref int i) => i++);
	}
	public delegate void StatEdit<T>(ref T value) where T : struct;
	public void EditStat<T>(string name, StatEdit<T> edit) where T : struct
	{
		T value = default;
		if (stats.TryGetValue(name, out var v))
		{
			value = (T)v;
		}
		edit(ref value);
		stats[name] = value;
	}

	public override void LoadBin(in GameDataReader reader)
	{
		int itemCount = reader.ReadInt();
		for (int i = 0; i < itemCount; i++)
		{
			stats.Add(reader.ReadString(), reader.ReadPrimitive());
		}
	}

	public override void LoadBlank()
	{

	}

	public override void SaveBin(in GameDataWriter writer)
	{
		writer.WritePrimitive(stats.Count);
		foreach (var item in stats)
		{
			writer.WritePrimitive(item.Key);
			writer.WritePrimitive(item.Value);
		}
	}
}
