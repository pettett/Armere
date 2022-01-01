using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace Armere.Console
{

	public class EditorConsole : EditorWindow
	{
		string input = "";

		[MenuItem("Armere/Console")]
		private static void ShowWindow()
		{
			var window = GetWindow<EditorConsole>();
			window.titleContent = new GUIContent("Console");
			window.Show();
		}

		private void OnGUI()
		{
			input = GUILayout.TextField(input);

			foreach (var s in Console.GetSuggestions(input))
			{
				if (GUILayout.Button(s))
					input = s;
			}

			if (GUILayout.Button("Execute"))
			{
				Console.ExecuteCommand(input);
				input = "";
			}

		}
	}
}
