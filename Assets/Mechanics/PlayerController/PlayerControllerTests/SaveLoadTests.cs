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
		var data = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Mechanics/PlayerController/Player.prefab");

		// Use the Assert class to test conditions

		Assert.NotNull(data);

		var playerObj = GameObject.Instantiate(data);

		var p = playerObj.GetComponent<PlayerController>();

		using var s = new MemoryStream();

		using (var w = new BinaryWriter(s))
		{
			p.Write(new GameDataWriter(w));
		}


		using (var r = new BinaryReader(s))
		{
			p.Read(new GameDataReader(r, "Player Test"));
		}

		GameObject.DestroyImmediate(playerObj);

	}

}
