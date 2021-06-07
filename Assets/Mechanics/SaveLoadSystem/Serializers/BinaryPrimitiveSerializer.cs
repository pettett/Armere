public readonly struct BinaryIntSerializer : IBinaryVariableSerializer<BinaryIntSerializer>
{
	readonly int value;
	public BinaryIntSerializer(int value)
	{
		this.value = value;
	}
	public void Write(in GameDataWriter writer) => writer.WritePrimitive(value);
	public BinaryIntSerializer Read(in GameDataReader reader) => reader.ReadInt();


	public override string ToString()
	{
		return value.ToString();
	}

	public static implicit operator BinaryIntSerializer(int value) => new BinaryIntSerializer(value);
	public static implicit operator int(BinaryIntSerializer value) => value.value;
}
public readonly struct BinaryULongSerializer : IBinaryVariableSerializer<BinaryULongSerializer>
{
	readonly ulong value;
	public BinaryULongSerializer(ulong value)
	{
		this.value = value;
	}
	public BinaryULongSerializer(BinaryULongSerializer value)
	{
		this.value = value.value;
	}
	public void Write(in GameDataWriter writer) => writer.WritePrimitive(value);
	public BinaryULongSerializer Read(in GameDataReader reader) => reader.ReadULong();

	public static implicit operator BinaryULongSerializer(ulong value) => new BinaryULongSerializer(value);
	public static implicit operator ulong(BinaryULongSerializer value) => value.value;
}

