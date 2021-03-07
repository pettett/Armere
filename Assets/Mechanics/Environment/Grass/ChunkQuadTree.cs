using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public abstract class QuadTreeLeaf
{

	public Rect rect;
	public int id;
	public int endID;

	public abstract void DrawGizmos(Terrain terrain, Vector2 uv, float uvRadius, HashSet<int> loaded, bool setColor = true);

	public abstract IEnumerable<QuadTreeEnd> GetLeavesInRect(Rect rect);
	public abstract void GetLeaves(System.Action<QuadTreeEnd> forEach);
	public abstract void GetLeavesInRange(Vector2 uv, float radius, System.Action<QuadTreeEnd> forEach);
	public abstract void GetLeavesInSingleRange(Vector2 uv1, float radius1, Vector2 uv2, float radius2, System.Action<QuadTreeEnd> forEach);

	public bool RectCircleOverlap(Vector2 uv, float radius)
	{
		float r2 = radius * radius;
		//If any point is within circle, true
		return (
			rect.Contains(uv) ||
			(rect.max - uv).sqrMagnitude < r2 || //Test min and max first for less vector2 creation?
			(rect.min - uv).sqrMagnitude < r2 ||
			(new Vector2(rect.xMin, rect.yMax) - uv).sqrMagnitude < r2 ||
			(new Vector2(rect.xMax, rect.yMin) - uv).sqrMagnitude < r2
		);
	}

	public bool SingleRectCircleOverlap(Vector2 uv1, float radius1, Vector2 uv2, float radius2)
	{
		//If any point is within circle, true
		return RectCircleOverlap(uv1, radius1) && !RectCircleOverlap(uv2, radius2);
	}

}

public class QuadTreeEnd : QuadTreeLeaf, IEquatable<QuadTreeEnd>
{
	public bool enabled;
	public int cellsWidth = 1;

	public QuadTreeEnd(bool enabled, Vector2 centre, Vector2 size, int chunksWidth, ref int chunkID, ref int endID)
	{
		this.rect = new Rect(centre - size / 2, size);

		this.enabled = enabled;
		this.cellsWidth = chunksWidth;
		this.id = chunkID;
		chunkID++;
		this.endID = endID;
		endID++;

	}


	public override void DrawGizmos(Terrain terrain, Vector2 uv, float uvRadius, HashSet<int> loaded, bool setColor = true)
	{

		if (setColor)
		{
			if (loaded.Contains(id))
			{
				Gizmos.color = Color.yellow;
			}
			else
			{
				Gizmos.color = new Color(enabled ? 0 : 1, enabled ? 1 : 0, RectCircleOverlap(uv, uvRadius) ? 1 : 0, 0.5f);
			}
		}
		var pos = new Vector3(rect.center.x, 0, rect.center.y);
		var size = new Vector3(rect.size.x, 0, rect.size.y);
		pos.x *= terrain.terrainData.size.x;
		size.x *= terrain.terrainData.size.x;
		pos.z *= terrain.terrainData.size.z;
		size.z *= terrain.terrainData.size.z;

		pos.x += terrain.transform.position.x;
		pos.z += terrain.transform.position.z;

		pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + 1;


		Gizmos.DrawCube(pos, size);

		if (setColor)
			Gizmos.color = Color.white;

		Gizmos.DrawWireCube(pos, size);

	}

	public bool Equals(QuadTreeEnd other)
	{
		return rect.Equals(other.rect);
	}

	public override int GetHashCode()
	{
		int hashCode = -2056627011;
		unchecked
		{
			hashCode = hashCode * -1521134295 + rect.GetHashCode();
			hashCode = hashCode * -1521134295 + enabled.GetHashCode();
		}
		return hashCode;
	}

	public override IEnumerable<QuadTreeEnd> GetLeavesInRect(Rect rect)
	{
		yield return this;
	}
	public override void GetLeavesInRange(Vector2 uv, float radius, System.Action<QuadTreeEnd> forEach)
	{
		forEach(this);
	}

	public override void GetLeavesInSingleRange(Vector2 uv1, float radius1, Vector2 uv2, float radius2, System.Action<QuadTreeEnd> forEach)
	{
		//If inside circle one and not inside circle 2
		if (enabled && SingleRectCircleOverlap(uv1, radius1, uv2, radius2))
		{
			forEach(this);
		}
	}


	public override void GetLeaves(Action<QuadTreeEnd> forEach)
	{
		forEach(this);
	}
}
public class QuadTree : QuadTreeLeaf
{
	public QuadTreeLeaf topLeft;
	public QuadTreeLeaf topRight;
	public QuadTreeLeaf bottomLeft;
	public QuadTreeLeaf bottomRight;

	int leafSize;



	QuadTreeLeaf GenerateLeaf(bool[,] cells, Vector2 center, int startX, int startY, int endX, int endY, int minCellGroupSize, int maxCellGroupSize, ref int chunkID, ref int endID)
	{
		// Debug.Log($"{startX},{startY},{endX},{endY}");
		//Debug.Log(endX - startX);
		if (endX - startX == minCellGroupSize)
		{
			bool enabled = false;
			//Search through all the cells in region. If any are enabled, all are enabled
			for (int xCell = startX; xCell < endX; xCell++)
				for (int yCell = startY; yCell < endY; yCell++)
					if (cells[xCell, yCell])
					{
						enabled = true;
						continue;
					}

			return new QuadTreeEnd(enabled, center, rect.size / 2, minCellGroupSize, ref chunkID, ref endID);
		}
		//This square is too big, 
		if (endX - startX > maxCellGroupSize)
		{
			return new QuadTree(cells, center, rect.size / 2, startX, startY, endX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID, ref endID);
		}

		bool hasChunk = false;
		bool allChunk = true;

		for (int xChunk = startX; xChunk < endX; xChunk++)
		{
			for (int yChunk = startY; yChunk < endY; yChunk++)
			{
				//BOTTOM LEFT
				if (cells[xChunk, yChunk])
				{
					//cut segment of chunks
					hasChunk = true;
				}
				else
				{
					allChunk = false;
				}
			}
		}


		if (hasChunk)
		{
			if (allChunk)
			{
				//Region is full
				return new QuadTreeEnd(true, center, rect.size / 2, (endX - startX), ref chunkID, ref endID);
			}
			else
			{
				return new QuadTree(cells, center, rect.size / 2, startX, startY, endX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID, ref endID);
			}
		}
		else
		{
			//Nothing in this region

			return new QuadTreeEnd(false, center, rect.size / 2, (endX - startX), ref chunkID, ref endID);
		}

	}


	public static QuadTree CreateQuadTree(bool[,] chunks, Vector2 centre, Vector2 size, int minCellGroupSize, int maxCellGroupSize)
	{
		int id = 1;
		int id2 = 1;
		return new QuadTree(chunks, centre, size, 0, 0, chunks.GetLength(0), chunks.GetLength(1), minCellGroupSize, maxCellGroupSize, ref id, ref id2);
	}

	public QuadTree(bool[,] chunks, Vector2 centre, Vector2 size, int startX, int startY, int endX, int endY, int minCellGroupSize, int maxCellGroupSize,
	 ref int chunkID, ref int endID)
	{
		this.rect = new Rect(centre - size / 2, size);

		id = -1;
		this.endID = endID;
		endID++;

		leafSize = endX - startX;

		if (leafSize == 0)
		{
			throw new System.Exception("Width or height is 0");
		}

		Vector2 offset = size / 4;

		int midY = startY + leafSize / 2;
		int midX = startX + leafSize / 2;


		bottomLeft = GenerateLeaf(chunks, centre + new Vector2(-offset.x, -offset.y), startX, startY, midX, midY, minCellGroupSize, maxCellGroupSize, ref chunkID, ref endID);
		bottomRight = GenerateLeaf(chunks, centre + new Vector2(offset.x, -offset.y), midX, startY, endX, midY, minCellGroupSize, maxCellGroupSize, ref chunkID, ref endID);
		topLeft = GenerateLeaf(chunks, centre + new Vector2(-offset.x, offset.y), startX, midY, midX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID, ref endID);
		topRight = GenerateLeaf(chunks, centre + new Vector2(offset.x, offset.y), midX, midY, endX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID, ref endID);
	}


	public override void DrawGizmos(Terrain terrain, Vector2 uv, float uvRadius, HashSet<int> loaded, bool setColor = true)
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(new Vector3(rect.center.x, 0, rect.center.y), new Vector3(rect.size.x, 0, rect.size.y));

		GetLeaves(x => x.DrawGizmos(terrain, uv, uvRadius, loaded));
	}

	public override IEnumerable<QuadTreeEnd> GetLeavesInRect(Rect rect)
	{
		if (topRight.rect.Overlaps(rect)) foreach (var l in topRight.GetLeavesInRect(rect)) yield return l;
		if (topLeft.rect.Overlaps(rect)) foreach (var l in topLeft.GetLeavesInRect(rect)) yield return l;
		if (bottomRight.rect.Overlaps(rect)) foreach (var l in bottomRight.GetLeavesInRect(rect)) yield return l;
		if (bottomLeft.rect.Overlaps(rect)) foreach (var l in bottomLeft.GetLeavesInRect(rect)) yield return l;
	}


	public override void GetLeavesInRange(Vector2 uv, float radius, System.Action<QuadTreeEnd> forEach)
	{
		if (topRight.RectCircleOverlap(uv, radius)) topRight.GetLeavesInRange(uv, radius, forEach);
		if (topLeft.RectCircleOverlap(uv, radius)) topLeft.GetLeavesInRange(uv, radius, forEach);
		if (bottomRight.RectCircleOverlap(uv, radius)) bottomRight.GetLeavesInRange(uv, radius, forEach);
		if (bottomLeft.RectCircleOverlap(uv, radius)) bottomLeft.GetLeavesInRange(uv, radius, forEach);
	}

	public override void GetLeavesInSingleRange(Vector2 uv1, float radius1, Vector2 uv2, float radius2, System.Action<QuadTreeEnd> forEach)
	{
		//If inside circle one at all - need to add small amount to stop it just breaking
		if (topRight.RectCircleOverlap(uv1, radius1)) topRight.GetLeavesInSingleRange(uv1, radius1, uv2, radius2, forEach);
		if (topLeft.RectCircleOverlap(uv1, radius1)) topLeft.GetLeavesInSingleRange(uv1, radius1, uv2, radius2, forEach);
		if (bottomRight.RectCircleOverlap(uv1, radius1)) bottomRight.GetLeavesInSingleRange(uv1, radius1, uv2, radius2, forEach);
		if (bottomLeft.RectCircleOverlap(uv1, radius1)) bottomLeft.GetLeavesInSingleRange(uv1, radius1, uv2, radius2, forEach);
	}


	public override void GetLeaves(Action<QuadTreeEnd> forEach)
	{
		topRight.GetLeaves(forEach);
		topLeft.GetLeaves(forEach);
		bottomRight.GetLeaves(forEach);
		bottomLeft.GetLeaves(forEach);
	}
}

public struct QuadTreeNode
{
	public int firstChild;
	public int secondChild;
	public int thirdChild;
	public int fourthChild;
	public Rect rect;
	public bool isBranch;

	public QuadTreeNode(QuadTree tree)
	{
		this.firstChild = tree.topLeft.endID; // Point to nodes array
		this.secondChild = tree.topRight.endID;
		this.thirdChild = tree.bottomLeft.endID;
		this.fourthChild = tree.bottomRight.endID;
		this.rect = tree.rect;
		this.isBranch = true;
	}
	public QuadTreeNode(QuadTreeEnd tree)
	{
		this.firstChild = tree.id; //Point to node data
		this.secondChild = -1;
		this.thirdChild = -1;
		this.fourthChild = -1;
		this.rect = tree.rect;
		this.isBranch = false;
	}
}
public struct QuadTreeNodeData
{
	public bool enabled;
	public int chunkCellsWidth;
	public Rect rect;
	public QuadTreeNodeData(QuadTreeEnd tree)
	{
		this.enabled = tree.enabled;
		this.chunkCellsWidth = tree.cellsWidth;
		this.rect = tree.rect;
	}
}

// public class FullQuadTree
// {
// 	public QuadTreeNode[] nodes;
// 	public QuadTreeNodeData[] nodeData;
// 	public FullQuadTree(bool[,] chunks, int minCellGroupSize, int maxCellGroupSize)
// 	{

// 		QuadTree tree = QuadTree.CreateQuadTree(chunks, Vector2.one * 0.5f, Vector2.one, minCellGroupSize, maxCellGroupSize);
// 		int treeEnds = 0;
// 		int treeBranches = 0;

// 		//Turn tree into arrays
// 		foreach (var leaf in tree)
// 		{
// 			if (leaf.id == -1)
// 			{
// 				treeBranches++;
// 			}
// 			else
// 			{
// 				treeEnds++;
// 			}
// 		}

// 		nodes = new QuadTreeNode[treeEnds + treeBranches];
// 		nodeData = new QuadTreeNodeData[treeEnds];

// 		foreach (var leaf in tree)
// 		{
// 			if (leaf.id == -1)
// 				nodes[leaf.endID] = new QuadTreeNode((QuadTree)leaf);
// 			else if (leaf is QuadTreeEnd end)
// 			{
// 				nodes[leaf.endID] = new QuadTreeNode(end);
// 				nodeData[leaf.id] = new QuadTreeNodeData(end);
// 			}
// 		}
// 	}
// 	public bool RectCircleOverlap(Rect rect, Vector2 uv, float radius)
// 	{
// 		float r2 = radius * radius;
// 		//If any point is within circle, true
// 		return (
// 			(rect.max - uv).sqrMagnitude < r2 || //Test min and max first for less vector2 creation?
// 			(rect.min - uv).sqrMagnitude < r2 ||
// 			(new Vector2(rect.xMin, rect.yMax) - uv).sqrMagnitude < r2 ||
// 			(new Vector2(rect.xMax, rect.yMin) - uv).sqrMagnitude < r2 ||
// 			rect.Contains(uv)
// 		);
// 	}
// 	public IEnumerable<int> GetNodesInRange(Vector2 uv, float radius)
// 	{
// 		int currentNode = 0;
// 		Queue<int> queue = new Queue<int>();
// 		do
// 		{
// 			if (queue.Count > 0) currentNode = queue.Dequeue();

// 			//Return indexes of node data contained inside circle
// 			if (RectCircleOverlap(nodes[currentNode].rect, uv, radius))
// 			{

// 				if (nodes[currentNode].isBranch)
// 				{
// 					queue.Enqueue(nodes[currentNode].firstChild);
// 					queue.Enqueue(nodes[currentNode].secondChild);
// 					queue.Enqueue(nodes[currentNode].thirdChild);
// 					queue.Enqueue(nodes[currentNode].fourthChild);
// 				}

// 				yield return currentNode;


// 			}


// 		} while (queue.Count != 0);
// 	}
// }
