using System.Collections.Generic;
using UnityEngine;

public struct BinaryDictionarySerializer<TKey, TValue> :
	IGameDataSavable<Dictionary<TKey, TValue>>,
	IGameDataSavable<BinaryDictionarySerializer<TKey, TValue>>

	where TKey : IGameDataSavable<TKey>, new()
	where TValue : IGameDataSavable<TValue>, new()
{
	Dictionary<TKey, TValue> value;
	public BinaryDictionarySerializer(Dictionary<TKey, TValue> value)
	{
		this.value = value;
	}
	public void Write(in GameDataWriter writer)
	{
		writer.WritePrimitive(value.Count);
		foreach (var kvp in value)
		{
			writer.Write(kvp.Key);
			writer.Write(kvp.Value);
		}

	}
	public Dictionary<TKey, TValue> Read(in GameDataReader reader)
	{
		int count = reader.Read<BinaryIntSerializer>();
		value = new Dictionary<TKey, TValue>(count);

		for (int i = 0; i < count; i++)
		{
			value.Add(reader.Read<TKey>(), reader.Read<TValue>());
		}
		return value;
	}

	BinaryDictionarySerializer<TKey, TValue> IGameDataSavable<BinaryDictionarySerializer<TKey, TValue>>.Read(in GameDataReader reader)
	{
		return new BinaryDictionarySerializer<TKey, TValue>(Read(in reader));
	}

	public BinaryDictionarySerializer<TKey, TValue> Init()
	{
		return this;
	}

	Dictionary<TKey, TValue> IGameDataSerializable<Dictionary<TKey, TValue>>.Init()
	{
		return this;
	}

	public static implicit operator BinaryDictionarySerializer<TKey, TValue>(Dictionary<TKey, TValue> value) => new BinaryDictionarySerializer<TKey, TValue>(value);
	public static implicit operator Dictionary<TKey, TValue>(BinaryDictionarySerializer<TKey, TValue> value) => value.value;
}


