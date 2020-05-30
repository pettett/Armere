using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContourGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ContourLine
    {
        public Vector2[] points;

        public ContourLine(params Vector2[] p)
        {
            points = p;
        }

        public void GizmosDraw(float scale)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.DrawLine(points[i] * scale, points[i + 1] * scale);
            }
        }
    }
    [System.Serializable]
    public class ContourLevel
    {
        public ContourLine[] lines;
        public void GizmosDraw(float scale)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].GizmosDraw(scale);
            }
        }
    }


    public Terrain terrain;
    public float contourDistance = 1;


    public ContourLevel[] levels;


    float[,] heights;

    float scale = 10;


    public void GenerateContours(){
        int cascadeCount = Mathf.CeilToInt(terrain.terrainData.size.y / contourDistance);
        levels = new ContourLevel[cascadeCount - 1];
        for (int i = 0; i < cascadeCount - 1; i++)
        {
            levels[i] = GenerateIsoLines(i * contourDistance / terrain.terrainData.size.y);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawGUITexture(new Rect(0, 0, scale, scale), terrain.terrainData.heightmapTexture);

        for (int k = 0; k < levels.Length; k++)
        {
            levels[k].GizmosDraw(scale);
        }

    }

    bool SampleHeightmap(int x, int y, float level)
    {

        return heights[y, x] >= level;
    }
    float Height(int x, int y) => heights[y, x];

    ContourLevel GenerateIsoLines(float level)
    {
        heights = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
        List<Vector2> lines = new List<Vector2>();
        float gap = 1f / terrain.terrainData.heightmapResolution;
        float hgap = gap * 0.5f;

        for (int x = 0; x < terrain.terrainData.heightmapResolution - 1; x++)
        {
            for (int y = 0; y < terrain.terrainData.heightmapResolution - 1; y++)
            {
                //Each iso line is generated in the center of a 2x2 pixel grid
                int c =
                (SampleHeightmap(x, y, level) ? 1 : 0) +
                (SampleHeightmap(x + 1, y, level) ? 2 : 0) +
                (SampleHeightmap(x, y + 1, level) ? 4 : 0) +
                (SampleHeightmap(x + 1, y + 1, level) ? 8 : 0);
                float topLeft = Height(x, y);
                float bottomLeft = Height(x, y + 1);
                float topRight = Height(x + 1, y);
                float bottomRight = Height(x + 1, y + 1);

                Vector2[] points = null;
                switch (c)
                {
                    case 1:
                        //Only above the level in one corner - DONE

                        points = new Vector2[]{
                            new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft,topRight,level), y * gap),
                            new Vector2(x * gap, y * gap + gap* Mathf.InverseLerp(topLeft,bottomLeft,level))
                        };
                        break;

                    case 2:
                        //one corner line
                        points = new Vector2[]{
                            new Vector2(x * gap +gap* Mathf.InverseLerp(topLeft,topRight,level), y * gap ),
                            new Vector2(x * gap+ gap , y * gap +gap* Mathf.InverseLerp(topRight,bottomRight,level))
                        };
                        break;
                    case 3:
                        //left to right line
                        points = new Vector2[]{
                            new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft,bottomLeft,level)),
                            new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight,bottomRight,level) )
                        };
                        break;
                    case 4:
                        //one corner line
                        points = new Vector2[]{
                            new Vector2(x * gap, y * gap+ gap * Mathf.InverseLerp(topLeft,bottomLeft,level) ),
                            new Vector2(x * gap + gap* Mathf.InverseLerp(bottomLeft,bottomRight,level), y * gap +gap)
                        };
                        break;

                    case 5:
                        //Up Down line
                        points = new Vector2[]{
                            new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft,topRight,level), y * gap),
                            new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft,bottomRight,level), y * gap + gap )
                        };
                        break;
                    case 6:
                        //one corner line
                        points = new Vector2[]{
                            new Vector2(x * gap, y * gap),
                            new Vector2(x * gap + gap, y * gap + gap )
                        };
                        break;
                    case 7:
                        //Inverse 1 - DONE
                        points = new Vector2[]{
                            new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft,bottomRight,level), y * gap +gap ),
                            new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight,bottomRight,level) )
                        };
                        break;
                    case 8:
                        //Bottom Right single corner line
                        //from bottom line to right line
                        points = new Vector2[]{
                            new Vector2(x * gap + gap, y * gap + gap* Mathf.InverseLerp(topRight,bottomRight,level)),
                            new Vector2(x * gap +gap* Mathf.InverseLerp(bottomLeft,bottomRight,level), y * gap + gap  )
                        };
                        break;
                    case 9:
                        //one corner line
                        points = new Vector2[]{
                            new Vector2(x * gap +gap , y * gap),
                            new Vector2(x * gap + gap, y * gap )
                        };
                        break;
                    case 10:
                        //Up Down line
                        points = new Vector2[]{
                            new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft,topRight,level), y * gap ),
                            new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft,bottomRight,level), y * gap + gap )
                        };
                        break;
                    case 11:
                        //negative gradient line on bottom left
                        points = new Vector2[]{
                            new Vector2(x * gap + gap* Mathf.InverseLerp(bottomLeft,bottomRight,level), y * gap + gap),
                            new Vector2(x * gap, y * gap+gap * Mathf.InverseLerp(topLeft,bottomLeft,level) )
                        };
                        break;
                    case 12:
                        //two corner left/right line - inverse case 3
                        points = new Vector2[]{
                            new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft,bottomLeft,level) ),
                            new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight,bottomRight,level))
                        };
                        break;
                    case 13:
                        //one corner line
                        points = new Vector2[]{
                            new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft,topRight,level), y * gap),
                            new Vector2(x * gap + gap, y * gap + gap* Mathf.InverseLerp(topRight,bottomRight,level) )
                        };
                        break;
                    case 14:
                        //one corner line
                        points = new Vector2[]{
                            new Vector2(x * gap, y * gap + gap* Mathf.InverseLerp(topLeft,bottomLeft,level)),
                            new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft,topRight,level), y * gap)
                        };
                        break;
                    default:
                        break;
                }
                if (points != null)
                {
                    lines.AddRange(points);
                }
            }
        }
        for (int i = 0; i < lines.Count; i++)
        {
            lines[i] = new Vector2(lines[i].x + hgap, 1 - lines[i].y - hgap);
        }

        List<List<Vector2>> levelLines = new List<List<Vector2>>();
        while (lines.Count != 0)
        {  

            var ps = new List<Vector2>();
            //Add the first segments
            ps.Add(lines[0]);
            ps.Add(lines[1]);
            lines.RemoveRange(0,1);

            //go through the rest of the lines and test if they can attach to this
            bool foundConnection = true;
            while (foundConnection)
            {
                //Reset found connection so the loop will exit when no connecting line is found
                foundConnection = false;
                for (int i = 0; i < lines.Count / 2; i++)
                {
                    if (ps[ps.Count - 1] == lines[i * 2])
                    {
                        ps.Add(lines[i * 2 + 1]);
                        lines.RemoveAt(i * 2 + 1);
                        foundConnection = true;
                    }
                    else if (ps[ps.Count - 1] == lines[i * 2 + 1])
                    {
                        ps.Add(lines[i * 2]);
                        lines.RemoveAt(i * 2);
                        foundConnection = true;
                    }
                }
            }

            levelLines.Add(ps);

        }
        


        //Convert all lines to contourLine classes

        var l = new ContourLine[levelLines.Count];
        for (int i = 0; i < levelLines.Count; i++)
        {
            l[i] = new ContourLine() { points = levelLines[i].ToArray() };
        }

        return new ContourLevel() { lines = l };
        //Done - generate points on a line

        //TODO - identify line segments to make line strings
    }
}
