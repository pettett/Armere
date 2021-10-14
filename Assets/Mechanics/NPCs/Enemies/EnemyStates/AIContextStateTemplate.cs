using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIContextStateTemplate<T> : AIStateTemplate
{
	[System.NonSerialized] public T context;

	public AIContextStateTemplate<T> Target(T context)
	{
		this.context = context;
		return this;
	}
}
