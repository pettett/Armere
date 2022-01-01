using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Armere.Console
{

	public static class Console
	{
		readonly struct Command
		{
			public readonly string name;
			public readonly string[] arguments;
			public readonly CommandExecuter executer;

			public Command(string name, string[] arguments, CommandExecuter executer)
			{
				this.name = name;
				this.arguments = arguments;
				this.executer = executer;
			}

			object ParseArg(ReadOnlySpan<char> arg, ArgumentParser p)
			{

				if (p != null)
				{
					try
					{
						return p.Invoke(arg);
					}
					catch
					{
						Debug.Log($"Failed on {arg.ToString()}");
						throw;
					}
				}
				else
					return new string(arg);
			}

			public readonly void RunCommand(ReadOnlySpan<char> args)
			{
				//Debug.Log($"Running command {name} {args.ToString()}");

				object[] parsedArgs;
				if (arguments != null || arguments.Length == 0)
				{
					parsedArgs = new object[arguments.Length];
					bool moreArgs = true;
					for (int i = 0; i < arguments.Length; i++)
					{
						if (!moreArgs)
						{
							//when run out of args, fill rest with nulls (default values)
							parsedArgs[i] = null;
						}
						else
						{

							var p = argumentTypes.GetValueOrDefault(arguments[i]);

							int nextGap = args.IndexOf(' ');
							ReadOnlySpan<char> arg;
							if (nextGap != -1)
							{
								arg = args.Slice(0, nextGap);
								args = args.Slice(nextGap + 1);
							}
							else
							{
								//no more args left
								arg = args;
								moreArgs = false;
							}

							parsedArgs[i] = ParseArg(arg, p);
						}
					}

					//Debug.Log("Finished parsing args");
				}
				else
				{
					parsedArgs = Array.Empty<object>();
				}

				//Debug.Log("Args: " + String.Join(' ', parsedArgs));

				executer.Invoke(parsedArgs);
			}


		}

		public delegate object ArgumentParser(ReadOnlySpan<char> argument);
		public delegate void CommandExecuter(object[] arguments);

		static List<Command> commands = new();


		static Dictionary<string, ArgumentParser> argumentTypes = new();

		public static void RegisterCommand(string name, CommandExecuter executer, params string[] arguments)
		{
			commands.Add(new Command(name, arguments, executer));
		}


		public static void RegisterArgumentType(string type, ArgumentParser parser)
		{
			argumentTypes[type] = parser;
		}


		public static bool ExecuteCommand(this UnityEngine.Object caller, string command)
		{
			Debug.Log(command, caller);
			return ExecuteCommand(command);
		}


		public static bool ExecuteCommand(string command)
		{
			return ExecuteCommand(command.AsSpan());
		}

		public static bool ExecuteCommand(ReadOnlySpan<char> command)
		{
			int nextGap = command.IndexOf(' ');

			var commandName = nextGap != -1 ? command.Slice(0, nextGap) : command;

			Assert.IsNotNull(commands);
			Assert.IsFalse(commands.Count == 0);


			//Debug.Log(commands.Count);

			foreach (var cmd in commands)
			{
				//Debug.Log($"testing {commandName.ToString()}, {cmd.name}");
				if (commandName.Equals(cmd.name, StringComparison.Ordinal))
				{

					cmd.RunCommand(command.Slice(nextGap + 1));

					return true;
				}
			}

			return false;

		}



		static Console()
		{
			//Register default types
			RegisterArgumentType("i32", x => int.Parse(x));
			RegisterArgumentType("u32", x => uint.Parse(x));
			RegisterArgumentType("f32", x => float.Parse(x));

			RegisterArgumentType("i64", x => long.Parse(x));
			RegisterArgumentType("u64", x => ulong.Parse(x));
			RegisterArgumentType("f64", x => double.Parse(x));

			RegisterArgumentType("bool", x => bool.Parse(x));
		}

		public static IEnumerable<string> GetSuggestions(string input)
		{
			if (input.Contains(' '))
			{
				//command has been selected
				string[] args = input.Split(' ');
				var c = commands.Find(x => x.name == args[0]);
				if (c.name != null)
				{

				}
			}
			else
			{
				foreach (var c in commands)
				{
					if (c.name.StartsWith(input))
					{
						yield return c.name;
					}
				}
			}
		}
	}
}
