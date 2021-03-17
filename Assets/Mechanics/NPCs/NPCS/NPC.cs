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

	public void InitNPC(NPCTemplate template, NPCSpawn spawn, Transform[] conversationGroupOverride)
	{

		this.spawn = spawn;

		collider = GetComponent<Collider>();


		gameObject.SetActive(true);
	}

	public override void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
	}
}
