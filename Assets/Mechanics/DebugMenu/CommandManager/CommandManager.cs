using System;
using UnityEngine;
using System.Collections.Generic;
using Armere.Inventory;
using UnityEngine.AddressableAssets;

public class CommandManager : ConsoleReceiver
{
	public string[] startingCommands;

	public string entry;
	public InventoryController inventory;

	const string tp = "tp";
	const string time = "time";
	const string give = "give";
	const string level = "level";
	const string giveQuest = "givequest";

	public static readonly CommandStructure[] commands = {
		new CommandStructure(tp,CommandArgument.Location),
		new CommandStructure(time),
		new CommandStructure(give,CommandArgument.ItemName,CommandArgument.Int),
		new CommandStructure(level,CommandArgument.LevelName),
		new CommandStructure(giveQuest),
	};


	[System.Flags]
	public enum CommandArgument
	{
		ItemName = 1,
		ItemType = 2,
		LevelName = 4,
		Int = 8,
		Location = 16
	}

	public class CommandStructure
	{
		public string name;
		public CommandArgument[] arguments;
		public CommandStructure(string name, params CommandArgument[] args)
		{
			this.name = name;
			this.arguments = args;
		}
	}


	public void ExecuteStartingCommands()
	{
		if (Application.isPlaying)
			foreach (string c in startingCommands)
			{
				OnCommand(new Command(c));
			}
	}

	[MyBox.ButtonMethod]
	public void ExecuteCommand()
	{
		if (Application.isPlaying) OnCommand(new Command(entry));
	}

	void DesiredInputs(Command command, int num)
	{
		if (command.values.Length < num)
			throw new ArgumentException("Not enough inputs for command");
	}
	public override void OnCommand(Command command)
	{
		switch (command.func.ToLower())
		{
			case tp:
				DesiredInputs(command, 1);
				if (TeleportWaypoints.singleton.waypoints.ContainsKey(command.values[0]))
				{
					var t = TeleportWaypoints.singleton.waypoints[command.values[0]];
					Character.playerCharacter.transform.SetPositionAndRotation(t.position, t.rotation);
				}
				break;
			case time:
				DesiredInputs(command, 1);
				if (command.values[0] == "day")
					print("Made day");
				break;
			case give:
				DesiredInputs(command, 2);
				if (ItemDatabase.itemDataNames.ContainsKey(command.values[0]))
				{
					ItemData item = command.values[0];//Implicit parse
					uint count = uint.Parse(command.values[1]);
					inventory.TryAddItem(item, count, false);
				}
				else
				{
					//TODO:
					//Load item
					uint count = uint.Parse(command.values[1]);

					ItemDatabase.LoadItemDataAsync<ItemData>(command.values[0], item => inventory.TryAddItem(item, count, false));
				}
				break;
			case level:
				DesiredInputs(command, 1);
				LevelManager.LoadLevel(command.values[0]);
				break;
			case giveQuest:
				DesiredInputs(command, 1);
				QuestManager.singleton.AddQuest(command.values[0]);
				break;
			default:
				//Invalid command error
				break;
		}
	}

	void AddEnumSuggestions<T>(string start, ref List<string> viableEntries) where T : System.Enum
	{
		foreach (T value in (T[])System.Enum.GetValues(typeof(T)))
		{
			if (value.ToString().ToLower().StartsWith(start.ToLower()))
			{
				viableEntries.Add(value.ToString());
			}
		}
	}

	public override List<string> GetSuggestionsForSlice(int slice, string[] segments)
	{
		List<string> viableEntries = new List<string>();
		if (slice == 0) // When editing first ones just show different types
			for (int i = 0; i < commands.Length; i++)
			{
				if (commands[i].name.StartsWith(segments[0]))
				{
					viableEntries.Add(commands[i].name);
				}
			}
		else
		{
			//editing paramters for command
			for (int i = 0; i < commands.Length; i++)
			{
				CommandStructure c = commands[i];
				if (c.name == segments[0] && c.arguments.Length >= slice)
				{
					//show suggestions for this argument
					CommandArgument a = c.arguments[slice - 1];
					// if (a.HasFlag(CommandArgument.ItemName))
					// {
					// 	//add item name commands
					// 	AddEnumSuggestions<ItemName>(segments[slice], ref viableEntries);
					// }
					if (a.HasFlag(CommandArgument.LevelName))
					{
						//AddEnumSuggestions<LevelName>(segments[slice], ref viableEntries);
					}
					if (a.HasFlag(CommandArgument.Location))
					{
						foreach (var item in TeleportWaypoints.singleton.waypoints)
						{
							if (item.Key.StartsWith(segments[slice]))
							{
								viableEntries.Add(item.Key);
							}
						}
					}
				}
			}
		}
		if (segments.Length > 1)
			for (int i = 0; i < viableEntries.Count; i++)
			{
				viableEntries[i] = string.Join(" ", segments[0..slice]) + " " + viableEntries[i];
			}

		return viableEntries;
	}


}
