

using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct CreateCutMeshJob : IJob
{
	int startingVertexOffset;
	bool mergeFaces;
	[ReadOnly]
	NativeArray<float> cutIntensities;
	[ReadOnly]
	NativeArray<Triangle> cutCylinder;
	[ReadOnly]
	NativeArray<int> meshTriangleOffsets;

	NativeArray<float3> vertices;
	NativeArray<float3> normals;
	NativeArray<float2> uv;
	[WriteOnly]
	NativeArray<int> triangles;

	float cutHeight;
	float cutSize;
	TriangleCutMode cutMode;
	float3 centerPoint;
	float subDivisionScalar;
	int minSubdivisions;
	int maxSubdivisions;
	float bevelDistribution;
	float bevelProfile;
	float crossSectionScale;

	public CreateCutMeshJob(int startingVertexOffset, NativeArray<float> cutIntensities, NativeArray<Triangle> cutCylinder, NativeArray<float3> vertices, NativeArray<float3> normals, NativeArray<float2> uv, NativeArray<int> triangles, NativeArray<int> meshTriangleOffsets, TriangleCutMode cutMode, float3 centerPoint, CuttableTreeProfile profile)
	{
		this.startingVertexOffset = startingVertexOffset;
		this.mergeFaces = profile.mergeFaces;
		this.cutIntensities = cutIntensities;
		this.cutCylinder = cutCylinder;
		this.vertices = vertices;
		this.normals = normals;
		this.uv = uv;
		this.triangles = triangles;
		this.meshTriangleOffsets = meshTriangleOffsets;
		this.cutHeight = profile.cutHeight;
		this.cutSize = profile.cutSize;
		this.cutMode = cutMode;
		this.centerPoint = centerPoint;
		this.subDivisionScalar = profile.subdivisionScalar;
		this.minSubdivisions = profile.minSubdivisions;
		this.maxSubdivisions = profile.maxSubdivisions;
		this.bevelDistribution = profile.bevelDistribution;
		this.bevelProfile = profile.bevelProfile;
		this.crossSectionScale = profile.crossSectionScale;
	}

	public int FindFullSubdivisions(float intensity)
	{
		//Base of 2 - intensity of 0 should have no subdivisions
		return intensity == 0 ? 1 : math.clamp((int)math.floor(subDivisionScalar * intensity) + minSubdivisions, minSubdivisions, maxSubdivisions);
	}

	public int FindSubdivisions(TriangleCutMode cutMode, float intensity) => cutMode == TriangleCutMode.Full ?
													FindFullSubdivisions(intensity) * 2 :
													FindFullSubdivisions(intensity);

	public int LinePointCount(TriangleCutMode cutMode, float intensity) => FindSubdivisions(cutMode, intensity) * 2 + 2;


	public void Execute()
	{
		int vertexOffset = startingVertexOffset;
		bool chainToLeft = false;

		//Use this data to finally create the cuts
		for (int i = 0; i < cutCylinder.Length; i++)
		{
			//Blend between triangles on the left (-1) and the right (+1)
			int leftTriangle = i - 1;
			if (leftTriangle == -1) leftTriangle = cutCylinder.Length - 1;

			if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0) || cutMode != TriangleCutMode.Full)
			{
				CutTriangle(cutCylinder[i], chainToLeft, meshTriangleOffsets[i], vertexOffset, cutIntensities[leftTriangle], cutIntensities[i]);


				//Track number of total verts (again)
				vertexOffset += chainToLeft ?
					LinePointCount(cutMode, cutIntensities[i])
					: LinePointCount(cutMode, cutIntensities[leftTriangle]) +
					LinePointCount(cutMode, cutIntensities[i]);

				//This triangle will be the first in startVertex chain sharing vertices
				chainToLeft = mergeFaces;
			}
			else
			{
				chainToLeft = false;
			}
			//rightIntensity = leftIntensity;
		}
	}



	public void CutTriangle(in Triangle t, bool connectLeft, int meshTriangleOffset, int vertexOffset, float leftIntensity, float rightIntensity)
	{

		//Cut the mesh
		int leftPointCount = LinePointCount(cutMode, leftIntensity);
		int leftSubdivisions = FindSubdivisions(cutMode, leftIntensity);
		int rightPointCount = LinePointCount(cutMode, rightIntensity);
		int rightSubdivisions = FindSubdivisions(cutMode, rightIntensity);


		const int left = 0;
		int right = leftPointCount; //Right side is one line over

		int verts = connectLeft ? rightPointCount : rightPointCount + leftPointCount;

		//Jobs are not done in parallel but are faster from burst
		if (!connectLeft)
		{
			//Both jobs are immediately scheduled
			//Create the left connection
			CutLine(t.a, t.pointingUpwards ? t.c : t.b, leftIntensity, leftSubdivisions, vertexOffset + left, t.pointingUpwards);
			//Create the right connection - needs to be done after the left side for safety
			CutLine(t.a, t.pointingUpwards ? t.b : t.c, rightIntensity, rightSubdivisions, vertexOffset + right, t.pointingUpwards);

		}
		else
		{
			//Create right line with no relative offset
			CutLine(t.a, t.pointingUpwards ? t.b : t.c, rightIntensity, rightSubdivisions, vertexOffset, t.pointingUpwards);
		}



		if (connectLeft) //Make right start from 0
			vertexOffset -= right;

		//Setup triangles - add all the triangles for top and bottom parts
		//Add top triangle

		if (t.pointingUpwards && cutMode != TriangleCutMode.Base || !t.pointingUpwards && cutMode != TriangleCutMode.Top)
		{
			//Calculate which parts will connect to the startVertex b and c key vertices
			int connectA1 = t.pointingUpwards ? left + leftPointCount - 1 : right;
			int connectA2 = t.pointingUpwards ? right + rightPointCount - 1 : left;

			triangles[meshTriangleOffset] = (vertexOffset + connectA1);
			triangles[meshTriangleOffset + 1] = (t.a);
			triangles[meshTriangleOffset + 2] = (vertexOffset + connectA2);
			meshTriangleOffset += 3; //This is startVertex copied parameter so can be changed - update for endVertex triangles
		}

		if (!t.pointingUpwards && cutMode != TriangleCutMode.Base || t.pointingUpwards && cutMode != TriangleCutMode.Top)
		{
			//Calculate which parts will connect to the startVertex b and c key vertices
			int connectBC1 = t.pointingUpwards ? left : right + rightPointCount - 1;
			int connectBC2 = t.pointingUpwards ? right : left + leftPointCount - 1;

			triangles[meshTriangleOffset] = (t.b);
			triangles[meshTriangleOffset + 1] = (vertexOffset + connectBC1);
			triangles[meshTriangleOffset + 2] = (vertexOffset + connectBC2);

			triangles[meshTriangleOffset + 3] = (t.b);
			triangles[meshTriangleOffset + 4] = (t.c);
			triangles[meshTriangleOffset + 5] = (vertexOffset + connectBC1);
		}



	}
	public static float3 TriangleNormal(float3 a, float3 b, float3 c) => math.cross(b - a, c - a);

	static float InverseHeightLerp(float3 start, float3 end, float height) => math.unlerp(start.y, end.y, height);

	public void CutLine(int startVertex, int endVertex, float intensity, int subDivisions, int vertexOffset, bool pointingUp)
	{
		float bottom = cutHeight - cutSize * intensity * 0.5f;
		float top = cutHeight + cutSize * intensity * 0.5f;

		float bottomCut = InverseHeightLerp(vertices[startVertex], vertices[endVertex], bottom);
		float topCut = InverseHeightLerp(vertices[startVertex], vertices[endVertex], top);


		float rangeBottom = cutMode == TriangleCutMode.Top ? (bottom + top) * 0.5f : bottom;
		float rangeTop = cutMode == TriangleCutMode.Base ? (bottom + top) * 0.5f : top;


		float lerpRangeBottom = cutMode == TriangleCutMode.Top ? 0.5f : 0;
		float lerpRangeSize = cutMode == TriangleCutMode.Full ? 1 : 0.5f;


		bool flattenLastVert = intensity < 0;


		//Top vertices
		float invTotalDivisions = 1f / subDivisions;

		for (int i = 0; i <= subDivisions; i++)
		{
			//Progress in range 0 to 1
			float progress = i * invTotalDivisions * lerpRangeSize + lerpRangeBottom;
			//X should be in range of -1 to 1
			float x = progress * 2 - 1;
			//Power x to bevel distribution to space out points
			x = math.pow(math.abs(x), 1f / bevelDistribution) * math.sign(x);
			//Apply this power to the progress
			progress = (x + 1) * 0.5f;

			float depth = math.pow(1 - math.pow(math.abs(x), bevelProfile), 1f / bevelProfile) * math.abs(intensity);

			float3 p1 = math.lerp(
				math.lerp(vertices[startVertex], vertices[endVertex], bottomCut),
				math.lerp(vertices[startVertex], vertices[endVertex], topCut), progress);
			float3 pos = math.lerp(p1, centerPoint, depth);

			if (flattenLastVert && i == subDivisions) pos.y = rangeBottom;

			//Every set of 2 points is identical as they do not share normals
			//Vector3 pos = Vector3.Lerp(Vector3.Lerp(vertices[startVertex], vertices[endVertex], t), cutCenter, depth);

			pos.y = math.clamp(pos.y, rangeBottom, rangeTop);

			vertices[vertexOffset + i * 2] = vertices[vertexOffset + i * 2 + 1] = pos;

			float2 dir = math.normalize(new float2(pos.x - centerPoint.x, pos.z - centerPoint.z));
			uv[vertexOffset + i * 2] = uv[vertexOffset + i * 2 + 1] = new float2(0.5f, 0.5f) + dir * (1 - depth) * crossSectionScale * 0.5f;
		}


		//Only change these uvs if they are not supposed to be startVertex part of startVertex flat trunk 
		if (cutMode != TriangleCutMode.Base)
			uv[vertexOffset + subDivisions * 2 + 1] = math.lerp(uv[startVertex], uv[endVertex], topCut);

		if (cutMode != TriangleCutMode.Top)
		{
			normals[vertexOffset] = math.normalize(math.lerp(normals[startVertex], normals[endVertex], bottom));
			uv[vertexOffset] = math.lerp(uv[startVertex], uv[endVertex], bottomCut);
		}
		else
		{
			normals[vertexOffset] = new float3(0, -1, 0);
		}

		if (cutMode != TriangleCutMode.Base)
		{
			normals[vertexOffset + subDivisions * 2 + 1] = math.normalize(math.lerp(normals[startVertex], normals[endVertex], top));
		}
		else
		{
			normals[vertexOffset + subDivisions * 2 + 1] = new float3(0, 1, 0);
		}


		float3 cutSurfaceCenter = math.lerp(vertices[startVertex], vertices[endVertex], (topCut + bottomCut) * 0.5f);

		float3 leftDirection = TriangleNormal(vertices[pointingUp ? endVertex : startVertex], centerPoint, vertices[pointingUp ? startVertex : endVertex]);

		for (int i = 0; i < subDivisions; i++)
		{
			normals[vertexOffset + i * 2 + 1] = normals[vertexOffset + i * 2 + 2] =
				math.cross(vertices[vertexOffset + i * 2 + 1] - vertices[vertexOffset + i * 2 + 2], leftDirection);
		}


	}


}