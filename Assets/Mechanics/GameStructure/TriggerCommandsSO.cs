
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Console;
[CreateAssetMenu(menuName = "Channels/Commands Channel")]
public class TriggerCommandsSO : VoidEventChannelSO
{
	public string[] commands = System.Array.Empty<string>();
	public override void RaiseEvent()
	{
		foreach (var c in commands)
		{
			this.ExecuteCommand(c);
		}
	}
}
