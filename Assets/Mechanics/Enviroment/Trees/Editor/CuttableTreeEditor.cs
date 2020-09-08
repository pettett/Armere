using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CuttableTree))]
public class CuttableTreeEditor : Editor
{
    public CuttableTree.TriangleCutMode testCutMode;

    private void OnEnable()
    {
        var t = (target as CuttableTree);
        t.FindCylinderTriangles();
    }

    void Property(string name)
    {
        //Debug.Log(name);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
    int subs = 6;
    float profile = 0.5f;
    public override void OnInspectorGUI()
    {
        var t = (target as CuttableTree);
        EditorGUI.BeginChangeCheck();

        //base.OnInspectorGUI();

        // ObjectField<MeshFilter>("Mesh Filter", ref t.meshFilter);
        // ObjectField<MeshCollider>("Mesh Filter", ref t.meshCollider);
        // ObjectField<AudioSource>("Audio Source", ref t.audioSource);
        Property("meshFilter");
        Property("meshCollider");
        Property("meshRenderer");
        Property("audioSource");
        Property("originalMesh");

        // GUILayout.Label("Cutting", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUI.BeginChangeCheck();

        Property("cutHeight");

        if (EditorGUI.EndChangeCheck())
            t.FindCenterPoint();

        Property("cutSize");

        if (EditorGUI.EndChangeCheck())
        {
            t.FindCylinderTriangles();
            t.TestForVerticesAboveCut();
            EditorUtility.SetDirty(t);
        }



        Property("cutCenter");
        Property("mergeFaces");
        Property("subdivisionScalar");
        Property("minSubdivisions");
        Property("maxSubdivisions");
        Property("intensityCutoff");

        Property("minSeveredIntensity");
        Property("bevelProfile");
        Property("bevelDistribution");
        Property("activeCutVectors");
        Property("gizmos");
        Property("damageToCut");

        Property("logDensity");
        Property("logEstimateHeight");
        Property("logEstimateRadius");
        Property("logKnockingForce");

        Property("logMaterial");
        Property("crosssectionMaterial");

        Property("crossSectionScale");

        Property("impactClips");

        testCutMode = (CuttableTree.TriangleCutMode)EditorGUILayout.EnumPopup("Test Cut Mode", testCutMode);

        if (EditorGUI.EndChangeCheck())
        {
            t.UpdateMeshFilter(testCutMode);
        }
        subs = Mathf.Max(1, EditorGUILayout.IntField("Subs", subs));
        profile = EditorGUILayout.FloatField("Profile", profile);

        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        Rect profileRect = GUILayoutUtility.GetAspectRect(1);
        float leftMovement = profileRect.width % (lineGap * boldLineStep) - 0.5f;
        profileRect.width -= leftMovement;
        profileRect.xMax += leftMovement;
        profileRect.xMin += leftMovement;

        profileRect.height -= profileRect.height % (lineGap * boldLineStep) - 0.5f;
        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
        {
            GUI.BeginClip(profileRect);
            GL.PushMatrix();
            DrawGrid(profileRect);


            DrawProfile(profileRect, subs);
            DrawProfile(profileRect, 40);

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
            GUI.EndClip();
        }





        GUI.enabled = false;

        EditorGUILayout.IntField("Additional Vertices:", t.meshFilter.sharedMesh.vertexCount - t.originalMesh.vertexCount);
        EditorGUILayout.FloatField("Log Mass:", t.LogMass);
    }
    const int lineGap = 10;
    const int boldLineStep = 5;
    public void DrawGrid(Rect rect)
    {
        //https://gamedev.stackexchange.com/questions/141302/how-do-i-draw-lines-to-a-custom-inspector/142326

        var material = new Material(Shader.Find("Hidden/Internal-Colored"));
        // If we are currently in the Repaint event, begin to draw a clip of the size of 
        // previously reserved rectangle, and push the current matrix for drawing.


        // Clear the current render buffer, setting a new background colour, and set our
        // material for rendering.
        GL.Clear(true, false, Color.black);
        material.SetPass(0);

        // Start drawing in OpenGL Quads, to draw the background canvas. Set the
        // colour black as the current OpenGL drawing colour, and draw a quad covering
        // the dimensions of the layoutRectangle.
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(rect.width, 0, 0);
        GL.Vertex3(rect.width, rect.height, 0);
        GL.Vertex3(0, rect.height, 0);
        GL.End();

        // Start drawing in OpenGL Lines, to draw the lines of the grid.
        GL.Begin(GL.LINES);

        // Store measurement values to determine the offset, for scrolling animation,
        // and the line count, for drawing the grid.
        int count = (int)(rect.width / lineGap) + 20;

        for (int i = 0; i < count; i++)
        {
            // For every line being drawn in the grid, create a colour placeholder; if the
            // current index is divisible by 5, we are at a major segment line; set this
            // colour to a dark grey. If the current index is not divisible by 5, we are
            // at a minor segment line; set this colour to a lighter grey. Set the derived
            // colour as the current OpenGL drawing colour.
            Color lineColour = i % boldLineStep == 0 ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.2f, 0.2f, 0.2f);
            GL.Color(lineColour);

            // Derive a new x co-ordinate from the initial index, converting it straight
            // into line positions, and move it back to adjust for the animation offset.
            float x = i * lineGap;

            if (x >= 0 && x < rect.width)
            {
                // If the current derived x position is within the bounds of the
                // rectangle, draw another vertical line.
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, rect.height, 0);
            }

            if (i < rect.height / lineGap)
            {
                // Convert the current index value into a y position, and if it is within
                // the bounds of the rectangle, draw another horizontal line.
                GL.Vertex3(0, i * lineGap, 0);
                GL.Vertex3(rect.width, i * lineGap, 0);
            }
        }
        // End lines drawing.
        GL.End();
    }

    public void DrawProfile(Rect rect, int subDivisions)
    {
        GL.Begin(GL.LINE_STRIP);

        GL.Vertex3(0, 0, 0);

        Vector2 bottom = new Vector2(0, 0);
        Vector2 top = new Vector2(0, 1);
        Vector2 mid = new Vector2(1, 0.5f);


        for (int i = 0; i <= subDivisions * 2; i++)
        {
            //Reprosent the bevel shape with a superellipse
            //(x/a)^n + (y/b)^n = 1
            float x = i / (float)subDivisions - 1;


            float y = Mathf.Pow(1 - Mathf.Pow(Mathf.Abs(x), profile), 1 / profile);

            x = (x + 1) * 0.5f;

            Vector2 p1 = Vector2.Lerp(bottom, top, x);
            Vector2 point = Vector2.Lerp(p1, mid, y);

            GL.Vertex3(y * rect.width, x * rect.height, 0);

        }

        // End lines drawing.
        GL.End();
    }


    public void OnSceneGUI()
    {
        var t = this.target as CuttableTree;
        Handles.color = Color.red;
        Handles.DrawWireDisc(t.transform.position + (t.transform.up * t.cutHeight), t.transform.up, t.logEstimateRadius);
        Handles.DrawWireDisc(t.transform.position + (t.transform.up * (t.cutHeight + t.logEstimateHeight)), t.transform.up, t.logEstimateRadius);
        Handles.DrawLine(
            t.transform.position + (t.transform.up * t.cutHeight) + t.transform.forward * t.logEstimateRadius,
            t.transform.position + (t.transform.up * (t.cutHeight + t.logEstimateHeight) + t.transform.forward * t.logEstimateRadius)
        );
        Handles.DrawLine(
            t.transform.position + (t.transform.up * t.cutHeight) - t.transform.forward * t.logEstimateRadius,
            t.transform.position + (t.transform.up * (t.cutHeight + t.logEstimateHeight) - t.transform.forward * t.logEstimateRadius)
        );
        Handles.DrawLine(
            t.transform.position + (t.transform.up * t.cutHeight) + t.transform.right * t.logEstimateRadius,
            t.transform.position + (t.transform.up * (t.cutHeight + t.logEstimateHeight) + t.transform.right * t.logEstimateRadius)
        );
        Handles.DrawLine(
            t.transform.position + (t.transform.up * t.cutHeight) - t.transform.right * t.logEstimateRadius,
            t.transform.position + (t.transform.up * (t.cutHeight + t.logEstimateHeight) - t.transform.right * t.logEstimateRadius)
        );

        Handles.color = Color.green;
        Vector3[] verts = t.originalMesh.vertices;

        //Draw triangle cylinder
        for (int i = 0; i < t.cylinderTriangles.Length; i++)
        {
            Handles.DrawLine(
                t.transform.position + verts[t.cylinderTriangles[i].b],
                t.transform.position + verts[t.cylinderTriangles[i].c]
            );

        }
    }

}

