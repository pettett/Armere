using UnityEngine;
using UnityEditor;
namespace Armere.Inventory
{
	[CustomEditor(typeof(InventoryController))]
	public class InventoryControllerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			InventoryController t = (InventoryController)target;
			if (Application.isPlaying)
				foreach (var panel in t.panels)
				{
					GUILayout.Label(panel.name, EditorStyles.boldLabel);
					for (int i = 0; i < panel.stackCount; i++)
					{
						GUILayout.Label(panel.ItemAt(i).ToString());
					}
				}
		}
	}
}