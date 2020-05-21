using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(DebugMenu))]
public class DebugMenuEditor : Editor
{
    ReorderableList list;
    private void OnEnable()
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("groups"), false, false, true, true);

        list.drawHeaderCallback = (Rect rect) =>
        {
            float indexWidth = 25;
            float checkWidth = 75;
            float nameWidth = rect.width - checkWidth - indexWidth;


            EditorGUI.LabelField(new Rect(rect.x + indexWidth, rect.y, nameWidth, EditorGUIUtility.singleLineHeight), "Name");
            EditorGUI.LabelField(new Rect(rect.x + indexWidth + nameWidth, rect.y, checkWidth, EditorGUIUtility.singleLineHeight), "Show Title");
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var name = element.FindPropertyRelative("name");
            var showTitle = element.FindPropertyRelative("showTitle");

            float indexWidth = 25;
            float checkWidth = 50;
            float nameWidth = rect.width - checkWidth - indexWidth;

            rect.y += 2;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, indexWidth, EditorGUIUtility.singleLineHeight), index.ToString());

            var newName = EditorGUI.TextField(new Rect(rect.x + indexWidth, rect.y, nameWidth, EditorGUIUtility.singleLineHeight), name.stringValue);


            showTitle.boolValue = EditorGUI.Toggle(new Rect(rect.x + indexWidth + nameWidth + checkWidth * 0.5f - 7, rect.y, checkWidth, EditorGUIUtility.singleLineHeight), showTitle.boolValue);

            name.stringValue = newName;
        };
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        list.DoLayoutList();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
