using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public struct TaggedSpawnPointProfile
{
	public AnimalStateTemplate spawnState;
	[MyBox.Tag] public string spawnTag;
}


[CreateAssetMenu(fileName = "Procedural Creature", menuName = "Game/NPCs/Procedural Creature", order = 0)]
public class ProceduralCreature : ScriptableObject
{
	public AssetReferenceGameObject prefab;
	public TaggedSpawnPointProfile[] validTagSpawns;
	public uint desiredSpawns = 5;
}