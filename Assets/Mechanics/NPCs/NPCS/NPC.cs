using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System.Linq;
using Armere.Inventory;
[RequireComponent(typeof(AIDialogue))]
public class NPC : AIHumanoid
{



	public Transform ambientThought;
	public TextMeshPro ambientThoughtText;
	new Camera camera;

	//Conversation currentConv;
	public static Dictionary<NPCName, NPC> activeNPCs = new Dictionary<NPCName, NPC>();
	public Animator animator;

	public NPCSpawn spawn;







	public void InitNPC(NPCTemplate template, NPCSpawn spawn, Transform[] conversationGroupOverride)
	{

		this.spawn = spawn;


		GetComponent<AIDialogue>().npcName = spawn.spawnedNPCName;
		activeNPCs[spawn.spawnedNPCName] = this;

		gameObject.SetActive(true);
	}

	private void Awake()
	{
		camera = Camera.main;
	}

	protected override void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
	}





	protected override void Update()
	{
		base.Update();
		if (inited)
		{
			ambientThought.rotation = camera.transform.rotation;
		}
	}




	IEnumerator RotatePlayerTowardsNPC(Transform playerTransform)
	{
		var dir = (transform.position - playerTransform.position);
		dir.y = 0;
		Quaternion desiredRot = Quaternion.LookRotation(dir);
		while (Quaternion.Angle(desiredRot, playerTransform.rotation) > 1f)
		{
			playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, desiredRot, Time.deltaTime * 800);
			yield return null;
		}
	}

	public void StartSpeaking(Transform player)
	{
		//Point the player towards the currently speaking npc
		StartCoroutine(RotatePlayerTowardsNPC(player));
	}

	public void StopSpeaking()
	{
		StopAllCoroutines();
	}



	public IEnumerator TurnToPlayer(Vector3 playerPosition)
	{
		var dir = playerPosition - transform.position;
		dir.y = 0;

		Quaternion desiredRot = Quaternion.LookRotation(dir);
		while (Quaternion.Angle(desiredRot, transform.rotation) > 1f)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, Time.deltaTime * 800);
			yield return null;
		}

	}






}
