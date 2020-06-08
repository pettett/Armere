using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTracker : MonoBehaviour
{

    static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    public Map map;
    DebugMenu.DebugEntry entry;
    // Start is called before the first frame update
    void Start()
    {
        entry = DebugMenu.CreateEntry("Player", "Current Region: {0}", "");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.z);
        int inRegion = -1;
        for (int i = 0; i < map.regions.Length; i++)
        {
            if (inRegion != -1 && map.regions[inRegion].priority > map.regions[i].priority) continue; //Only scan top priority layers

            if (map.regions[i].bounds.Contains(pos))
            {

                for (int j = 0; j < map.regions[i].triangles.Length / 3; j++)
                {
                    Vector2[] tri = new Vector2[]{
                    map.regions[i].shape[map.regions[i].triangles[j * 3]],
                    map.regions[i].shape[map.regions[i].triangles[j * 3 + 1]],
                    map.regions[i].shape[map.regions[i].triangles[j * 3 + 2]]
                };

                    if (PointInTriangle(
                        pos,
                        tri[0],
                        tri[1],
                        tri[2]
                    ))
                    {
                        inRegion = i;
                    }
                }
            }
        }


        if (inRegion != -1)
        {
            entry.values[0] = map.regions[inRegion].name;
        }
        else
        {
            entry.values[0] = "Wilderness";
        }
    }
}
