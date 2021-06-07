using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AutoSerializerTest
{
	public struct AutoSerializeClass
	{
		[AutoSerializeField(typeof(BinaryIntSerializer))] public int intValue;
		[AutoSerializeField(typeof(BinaryULongSerializer))] public ulong uLongValue;
		[AutoSerializeField(typeof(BinaryULongSerializer))] public BinaryULongSerializer uLongValue2;
		public BinaryIntSerializer intBinary;
	}

	// A Test behaves as an ordinary method
	[Test]
	public void AutoSerializerTestSimplePasses()
	{
		// Use the Assert class to test conditions
		AutoSerializeClass test = new AutoSerializeClass()
		{
			intValue = 2,
			uLongValue = 5564465,
			uLongValue2 = 55644665657214805,
			intBinary = 64,
		};

		using (MemoryStream stream = new MemoryStream())
		{
			GameDataWriter w = new GameDataWriter(new BinaryWriter(stream));


			AutoSerializer.AutoWrite(w, test);

			stream.Position = 0;


			GameDataReader r = new GameDataReader(new BinaryReader(stream));
			AutoSerializeClass test2 = AutoSerializer.AutoRead<AutoSerializeClass>(r);


			Assert.AreEqual(test, test2);

		}

	}



}
