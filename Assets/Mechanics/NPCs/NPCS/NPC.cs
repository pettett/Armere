using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIDialogue))]
public class NPC : AIHumanoid
{
	public NPCSpawn spawn;
	new Collider collider;
	public override Bounds bounds => collider.bounds;

	public void InitNPC(NPCSpawn spawn)
	{

		this.spawn = spawn;

	}

	public override void Start()
	{

		collider = GetComponent<Collider>();
		base.Start();
	}
}
