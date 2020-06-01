using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ContourLineGraphic : MaskableGraphic
{
    public ContourGenerator cg;
    public float thickness = 1;

    void AddContourLine(VertexHelper vh, ContourGenerator.ContourLine l)
    {
        int triOffset = vh.currentVertCount;
        for (int i = 0; i < l.points.Length; i++)
        {
            vh.AddVert(l.points[i] * rectTransform.rect.size - rectTransform.rect.size * 0.5f, color, l.points[i]);
            Vector2 e = l.GetExtrudedPoint(i, thickness);
            vh.AddVert(e * rectTransform.rect.size - rectTransform.rect.size * 0.5f, color, e);
        }
        for (int i = 0; i < l.points.Length - 1; i++)
        {
            vh.AddTriangle(triOffset + i * 2 + 1, triOffset + i * 2, triOffset + i * 2 + 2);
            vh.AddTriangle(triOffset + i * 2 + 1, triOffset + i * 2 + 2, triOffset + i * 2 + 3);
        }
        if (l.loop)
        {
            //attach first and last verts

            vh.AddTriangle(triOffset + 1, triOffset, triOffset + l.points.Length * 2 - 1);
            vh.AddTriangle(triOffset, triOffset + l.points.Length * 2 - 2, triOffset + l.points.Length * 2 - 1);
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (cg != null)
            for (int i = 0; i < cg.levels.Length; i++)
            {
                for (int j = 0; j < cg.levels[i].lines.Length; j++)
                {
                    AddContourLine(vh, cg.levels[i].lines[j]);
                }

            }
    }
}