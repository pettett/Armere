using UnityEngine;
using UnityEditor;
using System.Linq;
[CustomPropertyDrawer(typeof(ItemType))]
public class ItemTypePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        System.Type parent = property.serializedObject.targetObject.GetType();
        var attribs = parent.GetCustomAttributes(typeof(AllowItemTypesAttribute), false);
        if (attribs.Length > 0)
        {
            ItemType type = (ItemType)property.enumValueIndex;
            if (!(attribs[0] as AllowItemTypesAttribute).allowedTypes.Contains(type))
            {
                type = (attribs[0] as AllowItemTypesAttribute).allowedTypes[0];
            }
            //Limit the dropdown
            property.enumValueIndex = (int)(ItemType)EditorGUI.EnumPopup(position, new GUIContent(property.displayName), type, (System.Enum i) =>
              {
                  ItemType t = (ItemType)i;
                  return (attribs[0] as AllowItemTypesAttribute).allowedTypes.Contains(t);
              }
                );

        }
        else
        {
            EditorGUI.PropertyField(position, property, new GUIContent(property.displayName));
        }
    }
}