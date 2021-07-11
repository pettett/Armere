using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class SpawnPoint
{
	public Transform point;
	public bool used;
}
[System.Serializable]
public struct CreatureState
{
	public ProceduralCreature creature;
	public uint spawned;

}
public class ProceduralCreatureSpawner : MonoBehaviour
{
	public Dictionary<string, SpawnPoint[]> taggedSpawns = new Dictionary<string, SpawnPoint[]>();
	public CreatureState[] creatures;
	public float spawnInterval = 1f;
	public bool debugMessages = true;
	void FindSpawns()
	{
		foreach (var creature in creatures)
		{
			foreach (var spawn in creature.creature.validTagSpawns)
			{
				if (!taggedSpawns.ContainsKey(spawn.spawnTag))
				{
					GameObject[] tagged = GameObject.FindGameObjectsWithTag(spawn.spawnTag);
					SpawnPoint[] spawns = new SpawnPoint[tagged.Length];

					for (int i = 0; i < tagged.Length; i++)
						spawns[i] = new SpawnPoint() { point = tagged[i].transform };

					taggedSpawns[spawn.spawnTag] = spawns;
				}
			}
		}
	}


	public bool TryFindSpawnPoint(TaggedSpawnPointProfile[] validSpawns, out SpawnPoint spawn, out TaggedSpawnPointProfile profile)
	{
		foreach (var validSpawn in validSpawns)
		{
			SpawnPoint[] points = taggedSpawns[validSpawn.spawnTag];
			//Give one attempt to spawning with this type
			spawn = points[Random.Range(0, points.Length)];
			if (!spawn.used)
			{
				spawn.used = true;
				profile = validSpawn;
				return true;
			}
		}

		spawn = default;
		profile = default;

		return false;
	}


	private IEnumerator Start()
	{
		FindSpawns();

		while (enabled)
		{
			yield return new WaitForSeconds(spawnInterval);

			StringBuilder message = null;

			if (debugMessages)
				message = new StringBuilder("Attempting Spawn: \n");



			for (int i = 0; i < creatures.Length; i++)
			{
				if (debugMessages)
					message.AppendLine($"	Attemping to spawn {creatures[i].creature.name}:");

				if (creatures[i].spawned >= creatures[i].creature.desiredSpawns)
				{
					if (debugMessages)
						message.AppendLine($"		Too many existing to spawn another");
					continue;
				}
				//No spawn point avaliable
				if (!TryFindSpawnPoint(creatures[i].creature.validTagSpawns, out var spawn, out var profile))
				{
					if (debugMessages)
						message.AppendLine($"		No spawn point to spawn another");
					continue;
				}

				creatures[i].spawned++;

				var op = Addressables.InstantiateAsync(creatures[i].creature.prefab, spawn.point.position, spawn.point.rotation);

				if (debugMessages)
					message.AppendLine($"		Spawing on {spawn.point.gameObject.name}");

				yield return op;

				if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
				{
					var go = op.Result;

					go.GetComponent<AnimalMachine>().Init(profile.spawnState);


					if (debugMessages)
						message.AppendLine($"		Spawn success, finished loop");
					break;
				}
				else if (debugMessages)
					message.AppendLine($"		Spawn failed, continuing");

			}


			Debug.Log(message, this);
		}

	}
}
