using System.Collections.Generic;
using UnityEngine;

public struct BinaryListSerializer<T> : IGameDataSavable<List<T>> where T : IGameDataSavable<T>, new()
{
	List<T> value;
	public BinaryListSerializer(List<T> value)
	{
		this.value = value;
	}
	public void Write(in GameDataWriter writer)
	{
		writer.WritePrimitive(value.Count);
		for (int i = 0; i < value.Count; i++)
		{
			writer.Write(value[i]);
		}
	}
	public List<T> Read(in GameDataReader reader)
	{
		int count = reader.Read<BinaryIntSerializer>();
		value = new List<T>(count);
		for (int i = 0; i < count; i++)
		{
			value.Add(reader.Read<T>());
		}
		return value;
	}

	public List<T> Init()
	{
		return this;
	}

	public static implicit operator BinaryListSerializer<T>(List<T> value) => new BinaryListSerializer<T>(value);
	public static implicit operator List<T>(BinaryListSerializer<T> value) => value.value;
}


public readonly struct BinaryListAsyncSerializer<T> : IGameDataSavableAsync<BinaryListAsyncSerializer<T>> where T : IGameDataSavableAsync<T>, new()
{
	readonly List<T> list;
	public BinaryListAsyncSerializer(List<T> value)
	{
		this.list = value;
	}
	public void Write(in GameDataWriter writer)
	{
		writer.WritePrimitive(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Write(writer);
		}
	}
	public void Read(in GameDataReader reader, System.Action<BinaryListAsyncSerializer<T>> onDone)
	{
		int count = reader.ReadInt();

		var t = this;
		int done = 0;
		if (count == 0) onDone?.Invoke(this);
		else for (int i = 0; i < count; i++)
			{
				list.Add(default);
				int index = i;

				reader.ReadAsync<T>(item =>
			  {
				  t.list[index] = item;
				  done++;
				  //When every item has been loaded, mark this generator as loaded
				  if (done == count)
				  {
					  onDone?.Invoke(t.list);
				  }
			  });
			}
	}

	public BinaryListAsyncSerializer<T> Init()
	{
		return this;
	}

	public static implicit operator BinaryListAsyncSerializer<T>(List<T> value) => new BinaryListAsyncSerializer<T>(value);
	public static implicit operator List<T>(BinaryListAsyncSerializer<T> value) => value.list;
}