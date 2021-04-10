using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;

public interface IVariableAddon : IEnumerable<KeyValuePair<string, Yarn.Value>>
{
	string prefix { get; }
	Value this[string name]
	{
		get;
		set;
	}
}