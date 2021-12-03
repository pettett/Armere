using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using Armere.PlayerController;
using System.IO;

public class SaveLoadTests
{
	// A Test behaves as an ordinary method
	[Test]
	public void SaveLoadTestsSimplePasses()
	{
		var data = AssetDatabase.LoadAssetAtPath<PlayerSaveData>("Assets/Mechanics/PlayerController/Channels/Player Save Data.asset");

		// Use the Assert class to test conditions

		Assert.NotNull(data);


		using var s = new MemoryStream();

		using (var w = new BinaryWriter(s))
		{
			data.SaveBin(new GameDataWriter(w));
		}


		using (var r = new BinaryReader(s))
		{
			data.LoadBin(new GameDataReader(r));
		}



	}

}
