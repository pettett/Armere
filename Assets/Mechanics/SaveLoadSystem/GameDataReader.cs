using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;

public readonly struct GameDataReader
{

	public readonly Version saveVersion;
	public readonly BinaryReader reader;

	readonly Stack<(long, long)> regionStack;
	readonly PrimitiveCode ReadType() => (PrimitiveCode)reader.ReadByte();
	readonly void AssertType(PrimitiveCode type)
	{
		PrimitiveCode t = ReadType();
		Assert.IsTrue(t == type, $"Loaded Primitive {t} is not {type}");
	}

	public GameDataReader(BinaryReader reader)
	{
		this.reader = reader;
		saveVersion = default;
		regionStack = new Stack<(long, long)>();
		saveVersion = Read<Version>();
	}

	public readonly void BeginRegion()
	{
		long startingPosition = reader.BaseStream.Position;
		long desiredLength = reader.ReadInt64();
		regionStack.Push((desiredLength, startingPosition));
	}
	public readonly bool EndRegion()
	{
		(long desiredLength, long startingPosition) = regionStack.Pop();
		long endPos = reader.BaseStream.Position;

		long actualLength = endPos - startingPosition;
		if (actualLength != desiredLength)
		{
			Debug.LogError($"Region loaded incorrectly: len{actualLength}, supposed{desiredLength}");
			return false;
		}
		return true;
	}


	public readonly object ReadPrimitive() => ReadType() switch
	{
		PrimitiveCode.Bool => ReadBool(),
		PrimitiveCode.Int => ReadInt(),
		PrimitiveCode.UInt => ReadUInt(),
		PrimitiveCode.Char => ReadChar(),
		PrimitiveCode.UShort => ReadUShort(),
		PrimitiveCode.Vector2 => ReadVector2(),
		PrimitiveCode.Vector3 => ReadVector3(),
		_ => throw new System.Exception("Attempting to load non-primitive")
	};


	public readonly int ReadInt()
	{
		AssertType(PrimitiveCode.Int);
		return reader.ReadInt32();
	}
	public readonly uint ReadUInt()
	{
		AssertType(PrimitiveCode.UInt);
		return reader.ReadUInt32();
	}
	public readonly bool ReadBool()
	{
		AssertType(PrimitiveCode.Bool);
		return reader.ReadBoolean();
	}
	public readonly float ReadFloat()
	{
		AssertType(PrimitiveCode.Float);
		return reader.ReadSingle();
	}
	public readonly string ReadString() => reader.ReadString();
	public readonly char ReadChar()
	{
		AssertType(PrimitiveCode.Char);
		return reader.ReadChar();
	}
	public readonly long ReadLong()
	{
		AssertType(PrimitiveCode.Long);
		return reader.ReadInt64();
	}
	public readonly ulong ReadULong()
	{
		AssertType(PrimitiveCode.ULong);
		return reader.ReadUInt64();
	}
	public readonly byte ReadByte()
	{
		AssertType(PrimitiveCode.Byte);
		return reader.ReadByte();
	}
	public readonly ushort ReadUShort()
	{
		AssertType(PrimitiveCode.UShort);
		return reader.ReadUInt16();
	}
	//Asset reference is based of 32 digit hex guid string

	public readonly System.Guid ReadGuid() => new System.Guid(ReadBytes(16));
	public readonly byte[] ReadBytes(int count) => reader.ReadBytes(count);

	public readonly Vector3 ReadVector3()
	{
		AssertType(PrimitiveCode.Vector3);
		return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
	}
	public readonly Vector2 ReadVector2()
	{
		AssertType(PrimitiveCode.Vector2);
		return new Vector2(reader.ReadSingle(), reader.ReadSingle());
	}
	public readonly Quaternion ReadQuaternion()
	{
		AssertType(PrimitiveCode.Quaternion);
		var x = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);
		//Quaterions are normalized
		x.w = 1 - Mathf.Sqrt(x.x * x.x + x.y * x.y + x.z * x.z);
		return x;
	}

	//Read List functions read a list using metadata also stored
	public readonly byte[] ReadListByte() => ReadBytes(ReadInt());

	public readonly T Read<T>() where T : IBinaryVariableSerializer<T>, new() => (new T()).Read(this);
	public readonly T ReadInto<T>(T data) where T : IBinaryVariableSerializer<T> => data.Read(this);
	public readonly void ReadAsync<T>(System.Action<T> onDone) where T : IBinaryVariableAsyncSerializer<T>, new() => ReadAsyncInto(new T(), onDone);
	public readonly void ReadAsyncInto<T>(T data, System.Action<T> onDone = null) where T : IBinaryVariableAsyncSerializer<T> => data.Read(this, onDone);

}
