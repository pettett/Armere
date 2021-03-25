using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIDialogue))]
public class NPC : AIHumanoid
{


	public Animator animator;

	public NPCSpawn spawn;
	new Collider collider;
	public override Bounds bounds => collider.bounds;

	public void InitNPC(NPCSpawn spawn)
	{

		this.spawn = spawn;

		collider = GetComponent<Collider>();
	}

	public override void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
	}
}
