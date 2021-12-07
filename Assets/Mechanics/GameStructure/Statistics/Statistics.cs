using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct EntityInteractionData : IGameDataSavable<EntityInteractionData>
{
	public uint kills;
	public uint deaths;
	public string Format(string name)
	{
		return $"Killed {name} {kills} times, died {deaths} times";
	}

	public EntityInteractionData Init()
	{
		return this;
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
public class Statistics : ScriptableObject, IGameDataSavable<Statistics>
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


	public Statistics Read(in GameDataReader reader)
	{
		int itemCount = reader.ReadInt();
		for (int i = 0; i < itemCount; i++)
		{
			stats.Add(reader.ReadString(), reader.ReadPrimitive());
		}
		return this;
	}

	public void Write(in GameDataWriter writer)
	{
		writer.WritePrimitive(stats.Count);
		foreach (var item in stats)
		{
			writer.WritePrimitive(item.Key);
			writer.WritePrimitive(item.Value);
		}
	}

	public Statistics Init()
	{
		return this;
	}
}
