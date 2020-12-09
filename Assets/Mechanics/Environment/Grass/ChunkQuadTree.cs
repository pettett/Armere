using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class QuadTreeLeaf
{

    public Rect rect;
    public int id;

    public abstract void DrawGizmos();

    public abstract IEnumerable<QuadTreeEnd> GetLeavesInRect(Rect rect);

}

public class QuadTreeEnd : QuadTreeLeaf, IEquatable<QuadTreeEnd>
{
    public bool enabled;
    public int cellsWidth = 1;

    public QuadTreeEnd(bool enabled, Vector2 centre, Vector2 size, int chunksWidth, ref int chunkID)
    {
        this.rect = new Rect(centre - size / 2, size);

        this.enabled = enabled;
        this.cellsWidth = chunksWidth;
        this.id = chunkID;
        chunkID++;
    }


    public override void DrawGizmos()
    {
        DrawGizmos(true);
    }
    public void DrawGizmos(bool setColor = true)
    {

        if (setColor)
            Gizmos.color = new Color(enabled ? 0 : 1, enabled ? 1 : 0, 0, 0.5f);

        Gizmos.DrawCube(new Vector3(rect.center.x, 0, rect.center.y), new Vector3(rect.size.x, 0, rect.size.y));

        if (setColor)
            Gizmos.color = Color.white;

        Gizmos.DrawWireCube(new Vector3(rect.center.x, 0, rect.center.y), new Vector3(rect.size.x, 0, rect.size.y));

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
}

public class QuadTree : QuadTreeLeaf, IEnumerable<QuadTreeLeaf>
{
    public QuadTreeLeaf topLeft;
    public QuadTreeLeaf topRight;
    public QuadTreeLeaf bottomLeft;
    public QuadTreeLeaf bottomRight;

    int leafSize;

    QuadTreeLeaf GenerateLeaf(bool[,] chunks, Vector2 center, int startX, int startY, int endX, int endY, int minCellGroupSize, int maxCellGroupSize, ref int chunkID)
    {
        // Debug.Log($"{startX},{startY},{endX},{endY}");
        //Debug.Log(endX - startX);
        if (endX - startX == minCellGroupSize)
        {
            return new QuadTreeEnd(chunks[startX, startY], center, rect.size / 2, 1, ref chunkID);
        }
        //This square is too big, 
        if (endX - startX > maxCellGroupSize)
        {
            return new QuadTree(chunks, center, rect.size / 2, startX, startY, endX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID);
        }

        bool hasChunk = false;
        bool allChunk = true;

        for (int xChunk = startX; xChunk < endX; xChunk++)
        {
            for (int yChunk = startY; yChunk < endY; yChunk++)
            {
                //BOTTOM LEFT
                if (chunks[xChunk, yChunk])
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
                return new QuadTreeEnd(true, center, rect.size / 2, (endX - startX) / minCellGroupSize, ref chunkID);
            }
            else
            {
                return new QuadTree(chunks, center, rect.size / 2, startX, startY, endX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID);
            }
        }
        else
        {
            //Nothing in this region

            return new QuadTreeEnd(false, center, rect.size / 2, (endX - startX) / minCellGroupSize, ref chunkID);
        }

    }


    public static QuadTree CreateQuadTree(bool[,] chunks, Vector2 centre, Vector2 size, int minCellGroupSize, int maxCellGroupSize, ref int chunkID)
    {
        return new QuadTree(chunks, centre, size, 0, 0, chunks.GetLength(0), chunks.GetLength(1), minCellGroupSize, maxCellGroupSize, ref chunkID);
    }

    public QuadTree(bool[,] chunks, Vector2 centre, Vector2 size, int startX, int startY, int endX, int endY, int minCellGroupSize, int maxCellGroupSize, ref int chunkID)
    {
        this.rect = new Rect(centre - size / 2, size);

        this.id = chunkID;
        chunkID++;

        leafSize = endX - startX;

        if (leafSize == 0)
        {
            throw new System.Exception("Width or height is 0");
        }

        Vector2 offset = size / 4;

        int midY = startY + leafSize / 2;
        int midX = startX + leafSize / 2;

        bottomLeft = GenerateLeaf(chunks, centre + new Vector2(-offset.x, -offset.y), startX, startY, midX, midY, minCellGroupSize, maxCellGroupSize, ref chunkID);
        bottomRight = GenerateLeaf(chunks, centre + new Vector2(offset.x, -offset.y), midX, startY, endX, midY, minCellGroupSize, maxCellGroupSize, ref chunkID);
        topLeft = GenerateLeaf(chunks, centre + new Vector2(-offset.x, offset.y), startX, midY, midX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID);
        topRight = GenerateLeaf(chunks, centre + new Vector2(offset.x, offset.y), midX, midY, endX, endY, minCellGroupSize, maxCellGroupSize, ref chunkID);
    }


    public override void DrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3(rect.center.x, 0, rect.center.y), new Vector3(rect.size.x, 0, rect.size.y));

        foreach (var l in this)
        {
            l.DrawGizmos();
        }
    }

    public override IEnumerable<QuadTreeEnd> GetLeavesInRect(Rect rect)
    {
        if (topRight.rect.Overlaps(rect)) foreach (var l in topRight.GetLeavesInRect(rect)) yield return l;
        if (topLeft.rect.Overlaps(rect)) foreach (var l in topLeft.GetLeavesInRect(rect)) yield return l;
        if (bottomRight.rect.Overlaps(rect)) foreach (var l in bottomRight.GetLeavesInRect(rect)) yield return l;
        if (bottomLeft.rect.Overlaps(rect)) foreach (var l in bottomLeft.GetLeavesInRect(rect)) yield return l;
    }


    public IEnumerator<QuadTreeLeaf> GetEnumerator()
    {
        //yield squares in clockwise order
        yield return topRight;
        yield return bottomRight;
        yield return bottomLeft;
        yield return topLeft;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


}