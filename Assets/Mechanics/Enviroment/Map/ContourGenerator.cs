using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;
public class ContourGenerator : MonoBehaviour
{
    public bool gizmos;
    [System.Serializable]
    public class ContourLine
    {
        public Vector2[] points;
        public Color color;
        public bool loop = true;
        public Mesh mesh;

        public ContourLine(Vector2[] p, bool loop)
        {
            points = p;
            this.loop = loop;
            color = new Color(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
            mesh = GenerateMesh();

        }
        public Mesh GenerateMesh()
        {
            var m = new Mesh();
            var v = new Vector3[points.Length * 2];
            int tris = loop ? points.Length * 2 : points.Length * 2 - 2;
            var t = new int[tris * 3];

            for (int i = 0; i < points.Length; i++)
            {
                v[i * 2] = points[i];
                v[i * 2 + 1] = GetExtrudedPoint(i);
            }
            m.SetVertices(v);

            for (int i = 0; i < points.Length - 1; i++)
            {
                t[i * 6 + 0] = i * 2 + 1;
                t[i * 6 + 1] = i * 2 + 0;
                t[i * 6 + 2] = i * 2 + 2;

                t[i * 6 + 3] = i * 2 + 1;
                t[i * 6 + 4] = i * 2 + 2;
                t[i * 6 + 5] = i * 2 + 3;
            }

            if (loop)
            {
                //attach first and last verts
                t[t.Length - 1] = 1;
                t[t.Length - 2] = 0;
                t[t.Length - 3] = v.Length - 1;

                t[t.Length - 4] = 0;
                t[t.Length - 5] = v.Length - 2;
                t[t.Length - 6] = v.Length - 1;
            }

            m.SetTriangles(t, 0);
            m.RecalculateNormals();

            m.UploadMeshData(true);
            return m;
        }
        Vector2 Normal(Vector2 p1, Vector2 p2)
        {
            return Vector2.Perpendicular(p1 - p2).normalized;
        }
        Vector2 PointNormal(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (Normal(p1, p2) + Normal(p2, p3)).normalized;
        }

        public Vector2 GetExtrudedPoint(int i, float extrude = 0.002f)
        {
            Vector2 pNormal;
            if (points.Length < 2)
            {
                throw new System.Exception("Not enough points for line");
            }
            if (i == 0)
            {
                if (loop && points.Length > 2) //If loop and i is 0 or max, loop around
                {
                    pNormal = PointNormal(points[points.Length - 1], points[0], points[1]);
                }
                else //else use the normal of the line 
                {
                    pNormal = Normal(points[0], points[1]);
                }
            }
            else if (i == points.Length - 1)
            {
                //Last point
                if (loop && points.Length > 2) //If loop and i is 0 or max, loop around
                {
                    pNormal = PointNormal(points[points.Length - 2], points[points.Length - 1], points[0]);
                }
                else //else use the normal of the line 
                {
                    pNormal = Normal(points[points.Length - 2], points[points.Length - 1]);
                }
            }
            else
            {
                pNormal = PointNormal(points[i - 1], points[i], points[i + 1]);
            }

            return points[i] + pNormal * extrude;
        }

        public void GizmosDraw(float scale, Terrain terrain, float height)
        {
            Gizmos.color = color;
            Vector3 offset = terrain.transform.position;
            offset.z = -offset.z;
            for (int i = 0; i < points.Length - 1; i++)
            {
                //Draw line on texture
                // Gizmos.DrawLine(points[i] * scale, points[i + 1] * scale);

                // Gizmos.DrawLine(GetExtrudedPoint(i) * scale, GetExtrudedPoint(i + 1) * scale);



                // Draw world position
                // Gizmos.DrawLine(
                //     new Vector3(
                //         points[i].x * terrain.terrainData.size.x,
                //         height * terrain.terrainData.size.y,
                //         -points[i].y * terrain.terrainData.size.z
                //         ) + offset,
                //     new Vector3(
                //         points[i + 1].x * terrain.terrainData.size.x,
                //         height * terrain.terrainData.size.y,
                //         -points[i + 1].y * terrain.terrainData.size.z
                //         ) + offset);

            }
            //Join the start and end point
            if (loop)
            {
                //  Gizmos.DrawLine(points[0] * scale, points[points.Length - 1] * scale);
                //  Gizmos.DrawLine(GetExtrudedPoint(0) * scale, GetExtrudedPoint(points.Length - 1) * scale);
            }
            Gizmos.DrawMesh(mesh, new Vector3(0, 0, height * -2f), Quaternion.identity, Vector3.one * scale);
        }
    }
    [System.Serializable]
    public class ContourLevel
    {
        public ContourLine[] lines;
        public float level;
        public ContourLevel(ContourLine[] lines, float level)
        {
            this.lines = lines;
            this.level = level;
        }
        public void GizmosDraw(float scale, Terrain terrain)
        {
            if (lines != null)
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i].GizmosDraw(scale, terrain, level);
                }
        }
    }


    public Terrain terrain;
    public float contourDistance = 1;


    public ContourLevel[] levels;

    public Texture2D terrainHeightmap;
    public Gradient heightGradient = new Gradient();
    float[,] heights;

    float scale = 10;
    public bool roundToNearest;
    public float roundTo;
    public void GenerateContours()
    {
        GetHeights();
        int cascadeCount = Mathf.CeilToInt(terrain.terrainData.size.y / contourDistance);
        //levels = new ContourLevel[cascadeCount - 1];
        List<ContourLevel> l = new List<ContourLevel>();
        for (int i = 0; i < cascadeCount - 1; i++)
        {
            var le = GenerateIsoLines(i * contourDistance / terrain.terrainData.size.y);
            if (le.lines.Length != 0)
            {
                l.Add(le);
            }
        }
        levels = l.ToArray();
    }
    void GetHeights()
    {
        heights = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
    }
    public void GenerateTerrainHeightmap()
    {
        int res = terrain.terrainData.heightmapResolution;
        GetHeights();

        terrainHeightmap = new Texture2D(res, res);

        Color[] colors = new Color[res * res];
        float pixelGap = 1f / res;
        float h;

        float[] s = new float[9];
        Vector3 normal = new Vector3();
        float normalScale = 1;
        Vector3 sunDir = new Vector3(-1, 1, 1).normalized;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {


                //calculate normal of the point
                if (x != 0 && y != 0 && x != res - 1 && y != res - 1)
                {
                    s[0] = Height(x - 1, y - 1);
                    s[1] = Height(x, y - 1);
                    s[2] = Height(x + 1, y - 1);
                    s[3] = Height(x - 1, y);
                    s[4] = Height(x, y);
                    s[5] = Height(x + 1, y);
                    s[6] = Height(x - 1, y + 1);
                    s[7] = Height(x, y + 1);
                    s[8] = Height(x + 1, y + 1);

                    normal.x = normalScale * -(s[2] - s[0] + 2 * (s[5] - s[3]) + s[8] - s[6]);
                    normal.y = 1f;
                    normal.z = normalScale * -(s[6] - s[0] + 2 * (s[7] - s[1]) + s[8] - s[2]);

                    normal.Normalize();
                    float ndotl = Mathf.Clamp01(Vector3.Dot(normal, sunDir));
                    float brightness = ndotl * 0.5f;
                    colors[x + y * res] = new Color(brightness, brightness, brightness) + heightGradient.Evaluate(Height(x, y)) * 0.5f;
                }
            }
        }

        terrainHeightmap.SetPixels(colors);
        terrainHeightmap.Apply();
    }

    private void OnDrawGizmosSelected()
    {
        if (!gizmos) return;
        Gizmos.DrawGUITexture(new Rect(0, 0, scale, scale), terrainHeightmap);
        if (levels != null)
            for (int k = 0; k < levels.Length; k++)
            {
                levels[k].GizmosDraw(scale, terrain);
            }

    }

    bool SampleHeightmap(int x, int y, float level)
    {

        return heights[y, x] >= level;
    }
    float Height(int x, int y) => heights[y, x];



    ContourLevel GenerateIsoLines(float level)
    {


        float gap = 1f / terrain.terrainData.heightmapResolution;
        float hgap = gap * 0.5f;
        float topLeft;
        float bottomLeft;
        float topRight;
        float bottomRight;

        int isoLineResolution = terrain.terrainData.heightmapResolution - 1;

        int[,] cases = new int[isoLineResolution, isoLineResolution];

        int lineSegments = 0;

        int[] segmentsPerCase = new int[]{//Number of segments in every case from 0 to 15
            0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0
        };

        for (int x = 0; x < isoLineResolution; x++)
        {
            for (int y = 0; y < isoLineResolution; y++)
            {
                cases[x, y] = (SampleHeightmap(x, y, level) ? 1 : 0) +
                (SampleHeightmap(x + 1, y, level) ? 2 : 0) +
                (SampleHeightmap(x, y + 1, level) ? 4 : 0) +
                (SampleHeightmap(x + 1, y + 1, level) ? 8 : 0);
                //Add to the running total of cases required
                lineSegments += segmentsPerCase[cases[x, y]];
            }
        }
        //Pre-allocate space for every segment
        List<Vector2> lines = new List<Vector2>(lineSegments * 2);

        int index = 0;
        for (int x = 0; x < isoLineResolution; x++)
        {
            for (int y = 0; y < isoLineResolution; y++)
            {
                //Each iso line is generated in the center of a 2x2 pixel grid

                topLeft = Height(x, y);
                bottomLeft = Height(x, y + 1);
                topRight = Height(x + 1, y);
                bottomRight = Height(x + 1, y + 1);

                switch (cases[x, y])
                {
                    case 1:
                        //Only above the level in one corner - DONE
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft, topRight, level), y * gap));
                        lines.Add(new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft, bottomLeft, level)));
                        break;
                    case 2:
                        //one corner line
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft, topRight, level), y * gap));
                        lines.Add(new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight, bottomRight, level)));
                        break;
                    case 3:
                        //left to right line
                        lines.Add(new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft, bottomLeft, level)));
                        lines.Add(new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight, bottomRight, level)));
                        break;
                    case 4:
                        //one corner line
                        lines.Add(new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft, bottomLeft, level)));
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft, bottomRight, level), y * gap + gap));
                        break;

                    case 5:
                        //Up Down line
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft, topRight, level), y * gap));
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft, bottomRight, level), y * gap + gap));
                        break;
                    case 6:
                        //one corner line
                        lines.Add(new Vector2(x * gap, y * gap));
                        lines.Add(new Vector2(x * gap + gap, y * gap + gap));
                        break;
                    case 7:
                        //Inverse 1 - DONE
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft, bottomRight, level), y * gap + gap));
                        lines.Add(new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight, bottomRight, level)));
                        break;
                    case 8:
                        //Bottom Right single corner line
                        //from bottom line to right line
                        lines.Add(new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight, bottomRight, level)));
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft, bottomRight, level), y * gap + gap));
                        break;
                    case 9:
                        //one corner line
                        lines.Add(new Vector2(x * gap + gap, y * gap));
                        lines.Add(new Vector2(x * gap + gap, y * gap));
                        break;
                    case 10:
                        //Up Down line
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft, topRight, level), y * gap));
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft, bottomRight, level), y * gap + gap));
                        break;
                    case 11:
                        //negative gradient line on bottom left
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(bottomLeft, bottomRight, level), y * gap + gap));
                        lines.Add(new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft, bottomLeft, level)));
                        break;
                    case 12:
                        //two corner left/right line - inverse case 3
                        lines.Add(new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft, bottomLeft, level)));
                        lines.Add(new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight, bottomRight, level)));
                        break;
                    case 13:
                        //one corner line
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft, topRight, level), y * gap));
                        lines.Add(new Vector2(x * gap + gap, y * gap + gap * Mathf.InverseLerp(topRight, bottomRight, level)));
                        break;
                    case 14:
                        //one corner line
                        lines.Add(new Vector2(x * gap, y * gap + gap * Mathf.InverseLerp(topLeft, bottomLeft, level)));
                        lines.Add(new Vector2(x * gap + gap * Mathf.InverseLerp(topLeft, topRight, level), y * gap));
                        break;
                    default:
                        break;
                }
                index += segmentsPerCase[cases[x, y]] * 2;
            }
        }

        Debug.Assert(index == lines.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            lines[i] = new Vector2(lines[i].x + hgap, 1 - lines[i].y - hgap);
        }



        Profiler.BeginSample("Sorting Lines");
        ContourLine[] l;

        List<System.Tuple<LinkedList<Vector2>, bool>> levelLines = new List<System.Tuple<LinkedList<Vector2>, bool>>();
        while (lines.Count != 0)
        {
            var ps = new LinkedList<Vector2>();
            //Add the first segments
            ps.AddLast(lines[0]);
            ps.AddLast(lines[1]);
            lines.RemoveRange(0, 2);

            //go through the rest of the lines and test if they can attach to this
            bool foundConnection = true;
            bool loop = false;
            while (foundConnection)
            {
                //Reset found connection so the loop will exit when no connecting line is found
                foundConnection = false;
                for (int i = 0; i < lines.Count / 2; i++)
                {
                    if (ps.Last.Value == lines[i * 2])
                    {
                        ps.AddLast(lines[i * 2 + 1]);
                        lines.RemoveRange(i * 2, 2);
                        foundConnection = true;
                    }
                    else if (ps.Last.Value == lines[i * 2 + 1])
                    {
                        ps.AddLast(lines[i * 2]);
                        lines.RemoveRange(i * 2, 2);
                        foundConnection = true;
                    }
                    else if (ps.First.Value == lines[i * 2])
                    {
                        ps.AddFirst(lines[i * 2 + 1]);
                        lines.RemoveRange(i * 2, 2);
                        foundConnection = true;
                    }
                    else if (ps.First.Value == lines[i * 2 + 1])
                    {
                        ps.AddFirst(lines[i * 2]);
                        lines.RemoveRange(i * 2, 2);
                        foundConnection = true;
                    }
                    if (ps.First.Value == ps.Last.Value)
                    {
                        loop = true;
                        ps.RemoveLast();
                        break;
                    }
                }
            }
            if (ps.Count >= 2) // Lists of length 0 dont count
                levelLines.Add(new System.Tuple<LinkedList<Vector2>, bool>(ps, loop));
        }
        l = new ContourLine[levelLines.Count];
        for (int i = 0; i < levelLines.Count; i++)
        {
            l[i] = new ContourLine(levelLines[i].Item1.ToArray(), levelLines[i].Item2);
        }


        //Convert all lines to contourLine classes

        Profiler.EndSample();


        return new ContourLevel(l, level);
        //Done - generate points on a line

        //TODO - identify line segments to make line strings
    }
}
