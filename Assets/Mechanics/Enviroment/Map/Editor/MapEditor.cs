using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    Vector3[] ToVector3(Vector2[] a)
    {
        Vector3[] v = new Vector3[a.Length];
        for (int j = 0; j < v.Length; j++)
        {
            v[j] = new Vector3(a[j].x, 0, a[j].y);

        }
        return v;
    }
    Map m;
    public override void OnInspectorGUI()
    {
        m = target as Map;
        if (GUILayout.Button("Triangulate"))
        {
            for (int i = 0; i < m.regions.Length; i++)
            {
                m.regions[i].triangles = new Triangulator(m.regions[i].shape).Triangulate();
            }
        }
        base.OnInspectorGUI();
    }
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }


    Vector3 test = Vector3.zero;

    void OnSceneGUI(SceneView sceneView)
    {
        // your OnSceneGUI stuffs here
        Vector2 v = Event.current.mousePosition;
        //v.Scale(new Vector2(1, -1));
        int hoveredRegion = -1;


        // Plane p = new Plane(Vector3.up, Vector3.zero);
        // Ray r = sceneView.camera.ScreenPointToRay(v);
        // if (p.Raycast(r, out float e))
        // {
        //     test = r.GetPoint(e);
        // }
        // Handles.DoPositionHandle(test, Quaternion.identity);

        // for (int i = 0; i < m.regions.Length; i++)
        // {
        //     for (int j = 0; j < m.regions[i].triangles.Length / 3; j++)
        //     {
        //         Vector2[] tri = new Vector2[]{
        //             m.regions[i].shape[m.regions[i].triangles[j * 3]],
        //             m.regions[i].shape[m.regions[i].triangles[j * 3 + 1]],
        //             m.regions[i].shape[m.regions[i].triangles[j * 3 + 2]]
        //         };

        //         if (PointInTriangle(
        //             new Vector2(test.x, test.z),
        //             tri[0],
        //             tri[1],
        //             tri[2]
        //         ))
        //         {
        //             hoveredRegion = i;
        //             break;
        //         }
        //     }

        // }


        for (int i = 0; i < m.regions.Length; i++)
        {
            if (hoveredRegion == i) Handles.color = Color.red;
            else Handles.color = Color.white;


            int[] segs = new int[m.regions[i].shape.Length * 2];
            for (int j = 0; j < m.regions[i].shape.Length - 1; j++)
            {
                segs[2 * j] = j;
                segs[2 * j + 1] = j + 1;

            }

            Vector3 c = Vector3.zero;
            for (int j = 0; j < m.regions[i].shape.Length; j++)
            {
                var s = new Vector3(m.regions[i].shape[j].x, m.regions[i].priority, m.regions[i].shape[j].y);

                var n = Handles.FreeMoveHandle(s, Quaternion.identity, 3f, new Vector3(1, 0, 1), Handles.DotHandleCap);
                if (n != s)
                {
                    n.Scale(new Vector3(1, 0, 1));
                    m.regions[i].shape[j] = new Vector2(n.x, n.z);

                    m.regions[i].UpdateBounds();
                    EditorUtility.SetDirty(m);
                }

                c += n;
            }

            Handles.Label(c / m.regions[i].shape.Length, m.regions[i].name, new GUIStyle() { alignment = TextAnchor.MiddleCenter });

            segs[2 * m.regions[i].shape.Length - 1] = 0;
            segs[2 * m.regions[i].shape.Length - 2] = m.regions[i].shape.Length - 1;

            Handles.DrawLines(ToVector3(m.regions[i].shape), segs);
        }

    }
}