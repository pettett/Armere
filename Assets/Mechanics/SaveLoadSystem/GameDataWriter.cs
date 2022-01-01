using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;
using System;

public readonly struct GameDataWriter : IDisposable
{
	public readonly BinaryWriter writer;


	readonly void AssertType(PrimitiveCode type)
	{
		writer.Write((byte)type);
	}

	public GameDataWriter(BinaryWriter writer)
	{
		this.writer = writer;

		//Save the version of the game saved
		Write(SaveManager.version);
	}
	public readonly void WriteArrayHeader(int length)
	{
		AssertType(PrimitiveCode.ArrayHeader);
		writer.Write(length);
	}



	public readonly void WritePrimitive(int value)
	{
		AssertType(PrimitiveCode.Int);
		writer.Write(value);
	}
	public readonly void WritePrimitive(long value)
	{
		AssertType(PrimitiveCode.Long);
		writer.Write(value);
	}
	public readonly void WritePrimitive(ushort value)
	{
		AssertType(PrimitiveCode.UShort);
		writer.Write(value);
	}
	public readonly void WritePrimitive(ulong value)
	{
		AssertType(PrimitiveCode.ULong);
		writer.Write(value);
	}
	public readonly void WritePrimitive(bool value)
	{
		AssertType(PrimitiveCode.Bool);
		writer.Write(value);
	}
	public readonly void WritePrimitive(float value)
	{
		AssertType(PrimitiveCode.Float);
		writer.Write(value);
	}
	public readonly void WritePrimitive(uint value)
	{
		AssertType(PrimitiveCode.UInt);
		writer.Write(value);
	}
	public readonly void WritePrimitive(char value)
	{
		AssertType(PrimitiveCode.Char);
		writer.Write(value);
	}
	public readonly void WritePrimitive(byte value)
	{
		AssertType(PrimitiveCode.Byte);
		writer.Write(value);
	}
	public readonly void WritePrimitive(object value)
	{
		switch (value)
		{
			case int v: WritePrimitive(v); return;
			case uint v: WritePrimitive(v); return;
			case ushort v: WritePrimitive(v); return;
			case float v: WritePrimitive(v); return;
			case Vector2 v: WritePrimitive(v); return;
			case Vector3 v: WritePrimitive(v); return;
			case Quaternion v: WritePrimitive(v); return;
			case string v: WritePrimitive(v); return;
			case char v: WritePrimitive(v); return;
			case bool v: WritePrimitive(v); return;

			default:
				throw new System.ArgumentException("Type {value} is not primitive");
		}

	}

	public readonly void WritePrimitive(byte[] value) => writer.Write(value);
	public readonly void WritePrimitive(System.Guid value) => writer.Write(value.ToByteArray());
	public readonly void WritePrimitive(Quaternion value)
	{
		AssertType(PrimitiveCode.Quaternion);
		writer.Write(value.x);
		writer.Write(value.y);
		writer.Write(value.z);
		//writer.Write(value.w);
	}
	public readonly void WritePrimitive(Vector3 value)
	{
		AssertType(PrimitiveCode.Vector3);
		writer.Write(value.x);
		writer.Write(value.y);
		writer.Write(value.z);
	}
	public readonly void WritePrimitive(Vector2 value)
	{
		AssertType(PrimitiveCode.Vector2);
		writer.Write(value.x);
		writer.Write(value.y);
	}



	public readonly void WritePrimitive(string value) => writer.Write(value);


	//Write list functions store some metadata about the list so it can be easily loaded
	public readonly void WriteList(byte[] byteList)
	{
		WritePrimitive(byteList.Length);
		WritePrimitive(byteList);
	}

	public readonly void Write<T>(T value) where T : IGameDataSerializable<T> => value.Write(this);

	public void Dispose()
	{
		writer.Dispose();
	}
}
