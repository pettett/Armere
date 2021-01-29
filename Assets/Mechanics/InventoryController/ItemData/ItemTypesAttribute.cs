using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AllowItemTypesAttribute : System.Attribute
{
	// See the attribute guidelines at
	//  http://go.microsoft.com/fwlink/?LinkId=85236
	public readonly ItemType[] allowedTypes;

	// This is a positional argument
	public AllowItemTypesAttribute(params ItemType[] allowedTypes)
	{
		this.allowedTypes = allowedTypes;
	}
}