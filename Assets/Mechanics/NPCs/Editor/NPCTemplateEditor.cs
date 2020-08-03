using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CustomEditor(typeof(NPCTemplate))]
public class NPCTemplateEditor : Editor
{


    int selectedCascade = 0;
    float[] normalizedCascade;



    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NPCTemplate t = target as NPCTemplate;

        NPCTemplate.RoutineStage s = t.routine[selectedCascade];

        //sort cascades
        Array.Sort(t.routine);

        if (s != t.routine[selectedCascade])
        {
            selectedCascade = Array.IndexOf(t.routine, s);
        }

        float prevValues = 0;
        normalizedCascade = t.routine.Select(r =>
        {
            var amount = r.endTime / 24 - prevValues;
            prevValues += amount;
            return amount;

        }).ToArray();

        RoutineSplitGUI.HandleCascadeSliderGUI(ref normalizedCascade, ref selectedCascade, t.routine);
        //Convert back from normalized cascades into specific time stamps
        prevValues = 0;
        for (int i = 0; i < t.routine.Length; i++)
        {
            t.routine[i].endTime = Mathf.Clamp((normalizedCascade[i] + prevValues) * 24f, 0f, 24f);
            prevValues += normalizedCascade[i];
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("routine").GetArrayElementAtIndex(selectedCascade));




        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Add new stage"))
        {
            var temp = t.routine.ToList();
            temp.Add(new NPCTemplate.RoutineStage());
            t.routine = temp.ToArray();
        }
        if (t.routine.Length > 1)
            if (GUILayout.Button("Remove Selected"))
            {
                var temp = t.routine.ToList();
                temp.RemoveAt(selectedCascade);
                selectedCascade = Mathf.Clamp(selectedCascade, 0, temp.Count - 1);
                t.routine = temp.ToArray();
            }

    }
}