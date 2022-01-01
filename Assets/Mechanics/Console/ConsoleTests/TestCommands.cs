using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Armere.Console;

public class TestCommands
{
	// A Test behaves as an ordinary method
	[Test]
	public void TestCommandsArgs()
	{
		// Use the Assert class to test conditions
		bool success = false;

		Console.RegisterCommand("testargs", (object[] args) =>
		{
			success = true;
			Assert.IsNotNull(args);
			Assert.IsNotEmpty(args);
			Assert.AreEqual(args[0], 1);
		},
		"i32");

		Assert.IsTrue(Console.ExecuteCommand("testargs 1"), "Command not found");

		Assert.IsTrue(success, "Command not executed");

	}

	[Test]
	public void TestCommandsNoArgs()
	{
		// Use the Assert class to test conditions
		bool success = false;

		Console.RegisterCommand("testnoargs", (object[] args) =>
		{
			success = true;
			Assert.IsNotNull(args);
			Assert.IsEmpty(args);
		});

		Assert.IsTrue(Console.ExecuteCommand("testnoargs"), "Command not found");

		Assert.IsTrue(success, "Command not executed");

	}


}
