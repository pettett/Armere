using UnityEngine;
using UnityEditor;

namespace Armere.Inventory
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : Editor
    {
        ItemName editingItem;
        SerializedProperty itemArray;
        SerializedProperty editingProperty;
        private void OnEnable()
        {
            itemArray = serializedObject.FindProperty("itemData");
            var t = target as ItemDatabase;
            var names = System.Enum.GetValues(typeof(ItemName)) as ItemName[];

            if (t.itemData.Length != names.Length)
            {
                var newArray = new ItemData[names.Length];

                t.itemData.CopyTo(newArray, 0);
                t.itemData = newArray;
            }

            editingItem = (ItemName)0;
            UpdateEditingProperty();
        }
        void UpdateEditingProperty()
        {
            editingProperty = itemArray.GetArrayElementAtIndex((int)editingItem);
            editingProperty.isExpanded = true;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            editingItem = (ItemName)EditorGUILayout.EnumPopup(editingItem);
            if (EditorGUI.EndChangeCheck())
            {
                //Changed edited property
                UpdateEditingProperty();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(editingProperty, GUIContent.none, true);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

    }
}