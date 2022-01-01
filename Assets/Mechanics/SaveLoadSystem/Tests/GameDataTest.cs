using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameDataTest
{
	struct TestObject : IGameDataSavable<TestObject>
	{
		public float x;
		public TestObject Init()
		{
			return this;
		}

		public TestObject Read(in GameDataReader reader)
		{
			x = reader.ReadFloat();
			return this;
		}

		public void Write(in GameDataWriter writer)
		{
			writer.WritePrimitive(x);
		}
	}

	// A Test behaves as an ordinary method
	[Test]
	public void GameDataTestSimplePasses()
	{
		// Use the Assert class to test conditions

		using Stream s = new MemoryStream();

		BinaryWriter w = new BinaryWriter(s);
		w.Write(65);
		BinaryReader r = new BinaryReader(s);
		Debug.Log(r.Read());
	}

	// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
	// `yield return null;` to skip a frame.
	[UnityTest]
	public IEnumerator GameDataTestWithEnumeratorPasses()
	{
		// Use the Assert class to test conditions.
		// Use yield to skip a frame.
		yield return null;
	}
}
