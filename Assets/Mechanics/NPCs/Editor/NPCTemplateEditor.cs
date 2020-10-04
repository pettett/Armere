using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CustomEditor(typeof(NPCTemplate))]
public class NPCTemplateEditor : Editor
{


    int[] selectedCascades = new int[0];
    float[] normalizedCascade;



    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NPCTemplate t = target as NPCTemplate;
        if (selectedCascades.Length != t.routines.Length)
        {
            int[] newSelected = new int[t.routines.Length];
            //Copy as much as possible
            for (int i = 0; i < newSelected.Length && i < selectedCascades.Length; i++)
                newSelected[i] = selectedCascades[i];

            selectedCascades = newSelected;
        }

        for (int r = 0; r < t.routines.Length; r++)
        {

            NPCTemplate.Routine routine = t.routines[r];
            NPCTemplate.RoutineStage stage = routine.stages[selectedCascades[r]];

            if (r != t.routines.Length - 1)
            {

                EditorGUI.BeginChangeCheck();
                var routineProperty = serializedObject.FindProperty("routines").GetArrayElementAtIndex(r);
                EditorGUILayout.PropertyField(routineProperty.FindPropertyRelative("activateOnQuestComplete"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }


            //sort cascades
            Array.Sort(routine.stages);

            if (stage != routine.stages[selectedCascades[r]])
            {
                selectedCascades[r] = Array.IndexOf(routine.stages, stage);
            }

            float prevValues = 0;
            normalizedCascade = routine.stages.Select(s =>
            {
                var amount = s.endTime / 24 - prevValues;
                prevValues += amount;
                return amount;

            }).ToArray();

            RoutineSplitGUI.HandleCascadeSliderGUI(ref normalizedCascade, ref selectedCascades[r], routine.stages);
            //Convert back from normalized cascades into specific time stamps
            prevValues = 0;
            for (int i = 0; i < routine.stages.Length; i++)
            {
                routine.stages[i].endTime = Mathf.Clamp((normalizedCascade[i] + prevValues) * 24f, 0f, 24f);
                prevValues += normalizedCascade[i];
            }

            EditorGUI.BeginChangeCheck();
            var stageProperty = serializedObject.FindProperty("routines").GetArrayElementAtIndex(r).FindPropertyRelative("stages").GetArrayElementAtIndex(selectedCascades[r]);
            EditorGUILayout.PropertyField(stageProperty);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Add new stage"))
            {
                //insert a new blank element
                var array = serializedObject.FindProperty("routines").GetArrayElementAtIndex(r).FindPropertyRelative("stages");
                array.InsertArrayElementAtIndex(array.arraySize);
                serializedObject.ApplyModifiedProperties();
            }



            if (routine.stages.Length > 1)
                if (GUILayout.Button("Remove Selected"))
                {
                    var array = serializedObject.FindProperty("routines").GetArrayElementAtIndex(r).FindPropertyRelative("stages");
                    //Delete the selected element
                    array.DeleteArrayElementAtIndex(selectedCascades[r]);
                    selectedCascades[r] = Mathf.Clamp(selectedCascades[r], 0, array.arraySize - 1);

                    serializedObject.ApplyModifiedProperties();
                }

            if (t.routines.Length > 1)
                if (GUILayout.Button("Delete Routine"))
                {
                    //Delete this array
                    serializedObject.FindProperty("routines").DeleteArrayElementAtIndex(r);

                    serializedObject.ApplyModifiedProperties();
                }

        }


        if (GUILayout.Button("Add new Routine"))
        {
            // var temp = t.routines.ToList();
            // temp.Add(new NPCTemplate.Routine());
            // t.routines = temp.ToArray();
            serializedObject.FindProperty("routines").InsertArrayElementAtIndex(serializedObject.FindProperty("routines").arraySize);
            serializedObject.ApplyModifiedProperties();
        }


    }
}