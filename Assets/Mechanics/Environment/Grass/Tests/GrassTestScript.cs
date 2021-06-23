using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

public class GrassTestScript
{
	public void SetupParallelPrefixBuffers(
		int testSize,
		out ComputeShader prefixScanCompute,
		out ComputeBuffer source,
		out ComputeBuffer destination,
		out ComputeBuffer workBuffer,
		out uint[] correctData,
		out uint[] input)
	{
		source = new ComputeBuffer(testSize, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
		destination = new ComputeBuffer(testSize, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Dynamic);
		workBuffer = new ComputeBuffer(testSize, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Dynamic);


		prefixScanCompute = (ComputeShader)Resources.Load("Environment/Grass/PrefixScanKernals");

		var a = source.BeginWrite<uint>(0, testSize);

		correctData = new uint[testSize];
		input = new uint[testSize];
		uint runningTotal = 0;

		for (int i = 0; i < testSize; i++)
		{
			uint value = (uint)UnityEngine.Random.Range(0, 2);
			a[i] = value;
			input[i] = value;
			correctData[i] = runningTotal;
			runningTotal += value;
		}
		source.EndWrite<uint>(testSize);
	}

	public void SetupGrassCullBuffers(
		int testSize,
		out Matrix4x4 cameraMat,
		out ComputeShader cullGrassCompute,
		out ComputeBuffer source,
		out ComputeBuffer cullResult,
		out uint[] correctData)
	{
		source = new ComputeBuffer(testSize, GrassController.MeshProperties.size, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
		cullResult = new ComputeBuffer(testSize, sizeof(uint), ComputeBufferType.Default, ComputeBufferMode.Dynamic);


		cameraMat = Matrix4x4.Perspective(80f, 1f, 0.1f, 10f);
		cullGrassCompute = (ComputeShader)Resources.Load("Environment/Grass/CullGrassCompute");

		var a = source.BeginWrite<GrassController.MeshProperties>(0, testSize);

		correctData = new uint[testSize];

		for (int i = 0; i < testSize; i++)
		{
			uint chunk = (uint)UnityEngine.Random.Range(0, 2);
			var pos = Random.onUnitSphere;
			a[i] = new GrassController.MeshProperties() { chunkID = chunk, position = pos };

			var p = cameraMat.MultiplyPoint(pos);
			bool inBounds = Mathf.Abs(p.x) < 1 && Mathf.Abs(p.y) < 1 && Mathf.Abs(p.z) < 1;

			correctData[i] = chunk != 0 && inBounds ? 1u : 0u;
		}
		source.EndWrite<GrassController.MeshProperties>(testSize);
	}

	public void EndParallelPrefixTest(ComputeBuffer source, ComputeBuffer destination, ComputeBuffer workBuffer, uint[] correctData, uint[] input)
	{
		uint[] result = new uint[destination.count];
		destination.GetData(result);

		bool equal = Enumerable.SequenceEqual(result, correctData);
		if (!equal)
		{
			Debug.Log(string.Join(",", input));
			Debug.Log(string.Join(",", result));
			Debug.Log(string.Join(",", correctData));
		}
		source.Release();
		destination.Release();
		workBuffer.Release();


		Assert.IsTrue(equal);
	}

	// A Test behaves as an ordinary method
	[Test]
	[TestCase(20)]
	[TestCase(500)]
	[TestCase(1500)]
	[TestCase(15000)]
	public void TestPrefixSum(int testSize)
	{
		// Use the Assert class to test conditions

		// //This is all witchcraft :(

		SetupParallelPrefixBuffers(testSize, out var prefixScanCompute, out var source, out var destination, out var workBuffer, out var correctData, out var input);

		GrassLayerInstance.GetPrefixData(testSize, out int blocks, out int scanBlocks);

		//Clean buffer
		prefixScanCompute.SetBuffer(3, "dst", destination);
		prefixScanCompute.SetBuffer(3, "sumBuffer", workBuffer);


		prefixScanCompute.SetBuffer(0, GrassController.ID_Grass, source);
		prefixScanCompute.SetBuffer(0, "dst", destination);
		prefixScanCompute.SetBuffer(0, "sumBuffer", workBuffer);

		prefixScanCompute.SetBuffer(1, "dst", workBuffer);


		prefixScanCompute.SetInt("m_numElems", testSize);
		prefixScanCompute.SetInt("m_numBlocks", blocks); //Number of sharedmemory blokes
		prefixScanCompute.SetInt("m_numScanBlocks", scanBlocks); //Number of thread groups

		prefixScanCompute.Dispatch(3, blocks, 1, 1);

		prefixScanCompute.Dispatch(0, blocks, 1, 1);

		prefixScanCompute.Dispatch(1, 1, 1, 1);

		if (blocks > 1)
		{
			prefixScanCompute.SetBuffer(2, "dst", destination);
			prefixScanCompute.SetBuffer(2, "blockSum2", workBuffer);
			prefixScanCompute.Dispatch(2, (blocks - 1), 1, 1);
		}

		EndParallelPrefixTest(source, destination, workBuffer, correctData, input);
	}

	[Test]
	[TestCase(20)]
	[TestCase(500)]
	[TestCase(1500)]
	[TestCase(15000)]
	public void TestPrefixSumCommandBuffer(int testSize)
	{

		SetupParallelPrefixBuffers(testSize, out var prefixScanCompute, out var source, out var destination, out var workBuffer, out var correctData, out var input);

		CommandBuffer cmd = new CommandBuffer();

		GrassLayerInstance.RunPrefixSum(cmd, prefixScanCompute, destination, workBuffer, source);

		Graphics.ExecuteCommandBuffer(cmd);

		EndParallelPrefixTest(source, destination, workBuffer, correctData, input);
	}
	public void EndGrassCullTest(ComputeBuffer source, ComputeBuffer cullData, uint[] correct)
	{

		uint[] result = new uint[source.count];
		cullData.GetData(result);

		bool equal = Enumerable.SequenceEqual(result, correct);

		if (!equal)
		{
			Debug.Log(string.Join(",", result));
			Debug.Log(string.Join(",", correct));
		}

		source.Dispose();
		cullData.Dispose();
		Assert.IsTrue(equal);
	}

	[Test]
	[TestCase(20)]
	[TestCase(500)]
	[TestCase(1500)]
	[TestCase(15000)]
	public void TestGrassCull(int count)
	{

		int threadGroups = GrassLayerInstance.GetThreadGroups(count);

		SetupGrassCullBuffers(count, out var cameraMat, out var cullCompute, out var source, out var cullData, out var correct);
		cullCompute.SetBuffer(0, "_Grass", source);
		cullCompute.SetBuffer(0, "_CullResult", cullData);
		cullCompute.SetMatrix("cameraFrustum", cameraMat);
		cullCompute.Dispatch(0, threadGroups, 1, 1);

		EndGrassCullTest(source, cullData, correct);
	}
	[Test]
	[TestCase(20)]
	[TestCase(500)]
	[TestCase(1500)]
	[TestCase(15000)]
	public void TestGrassCullComputeBuffer(int count)
	{

		int threadGroups = GrassLayerInstance.GetThreadGroups(count);
		Debug.Log(threadGroups);
		SetupGrassCullBuffers(count, out var cameraMat, out var cullCompute, out var source, out var cullData, out var correct);

		CommandBuffer cmd = new CommandBuffer();
		GrassLayerInstance.RunCullGrass(cmd, threadGroups, cameraMat, cullCompute, source, cullData);

		Graphics.ExecuteCommandBuffer(cmd);

		EndGrassCullTest(source, cullData, correct);
	}

}