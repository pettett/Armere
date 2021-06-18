using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CommandWindow : EditorWindow
{
	string command;

	public List<string> recentCommands = new List<string>();

	[MenuItem("Armere/Commands")]
	private static void ShowWindow()
	{
		var window = GetWindow<CommandWindow>();
		window.titleContent = new GUIContent("Commands");
		window.Show();
	}
	void SetCommand(object command)
	{
		this.command = (string)command;
	}
	private void OnGUI()
	{

		command = EditorGUILayout.TextField(command);
		if (recentCommands.Count != 0 && GUILayout.Button("Recent"))
		{
			GenericMenu menu = new GenericMenu();
			for (int i = 0; i < recentCommands.Count; i++)
			{
				menu.AddItem(new GUIContent(recentCommands[i]), false, SetCommand, recentCommands[i]);
			}
			menu.ShowAsContext();
		}


		GUI.enabled = Application.isPlaying;
		if (GUILayout.Button("Execute"))
		{
			Console.singleton.receiver.OnCommand(new Command(command));
			recentCommands.Add(command);

			if (recentCommands.Count > 10)
			{
				recentCommands.RemoveAt(0);
			}
		}

	}
}