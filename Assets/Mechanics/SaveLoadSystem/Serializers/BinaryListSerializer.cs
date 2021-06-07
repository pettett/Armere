using System.Collections.Generic;

public struct BinaryListSerializer<T> : IBinaryVariableSerializer<List<T>> where T : IBinaryVariableSerializer<T>, new()
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

	public static implicit operator BinaryListSerializer<T>(List<T> value) => new BinaryListSerializer<T>(value);
	public static implicit operator List<T>(BinaryListSerializer<T> value) => value.value;
}


public readonly struct BinaryListAsyncSerializer<T> : IBinaryVariableAsyncSerializer<BinaryListAsyncSerializer<T>> where T : IBinaryVariableAsyncSerializer<T>, new()
{
	readonly List<T> value;
	public BinaryListAsyncSerializer(List<T> value)
	{
		this.value = value;
	}
	public void Write(in GameDataWriter writer)
	{
		writer.WritePrimitive(value.Count);
		for (int i = 0; i < value.Count; i++)
		{
			value[i].Write(writer);
		}
	}
	public void Read(in GameDataReader reader, System.Action<BinaryListAsyncSerializer<T>> onDone)
	{
		int count = reader.Read<BinaryIntSerializer>();
		var list = new List<T>(count);
		int done = 0;
		if (count == 0) onDone?.Invoke(this);
		else for (int i = 0; i < count; i++)
			{
				list.Add(default);
				int index = i;
				reader.ReadAsync<T>(item =>
			   {
				   list[index] = item;
				   done++;
				   //When every item has been loaded, mark this generator as loaded
				   if (done == count)
				   {
					   onDone?.Invoke(list);
				   }
			   });
			}
	}


	public static implicit operator BinaryListAsyncSerializer<T>(List<T> value) => new BinaryListAsyncSerializer<T>(value);
	public static implicit operator List<T>(BinaryListAsyncSerializer<T> value) => value.value;
}