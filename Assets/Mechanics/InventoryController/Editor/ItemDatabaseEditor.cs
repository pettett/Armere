using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

namespace Armere.Inventory
{
	static class KvpExtensions
	{
		public static void Deconstruct<TKey, TValue>(
			this KeyValuePair<TKey, TValue> kvp,
			out TKey key,
			out TValue value)
		{
			key = kvp.Key;
			value = kvp.Value;
		}
	}

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
		bool canAssign = true;
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
			GUI.enabled = Application.isPlaying && canAssign;
			if (GUILayout.Button("Assign item ids"))
			{
				AssignItemIDs();
			}

			GUI.enabled = true;
			if (GUILayout.Button("Test"))
			{
				((ItemDatabase)target).Test();
			}

		}


		public void AssignItemIDs()
		{

			canAssign = false;
			var x = Addressables.LoadResourceLocationsAsync("item", type: typeof(ItemData));


			x.Completed += _ =>
			{
				var y = Addressables.LoadAssetsAsync<ItemData>(x.Result, null);

				y.Completed += _ =>
				{
					for (int i = 0; i < y.Result.Count; i++)
					{
						if (y.Result[i] is PhysicsItemData physics)
						{
							Debug.Log(physics.gameObject.AssetGUID);
						}
					}
				};
			};


		}


	}
}