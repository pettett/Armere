//Case where variables serialize to themselves
public struct BinaryTupleSerializer<T1, T2> : IGameDataSavable<(T1, T2)>
	where T1 : IGameDataSavable<T1>, new()
	where T2 : IGameDataSavable<T2>, new()
{
	(T1, T2) data;

	public BinaryTupleSerializer((T1, T2) data)
	{
		this.data = data;
	}

	public void Write(in GameDataWriter writer)
	{
		data.Item1.Write(writer);
		data.Item2.Write(writer);
	}
	public (T1, T2) Read(in GameDataReader reader)
	{
		return ((new T1()).Read(reader), (new T2()).Read(reader));
	}

	public (T1, T2) Init()
	{
		return this;
	}

	public static implicit operator BinaryTupleSerializer<T1, T2>((T1, T2) value) => new BinaryTupleSerializer<T1, T2>(value);
	public static implicit operator (T1, T2)(BinaryTupleSerializer<T1, T2> value) => value.data;
}
