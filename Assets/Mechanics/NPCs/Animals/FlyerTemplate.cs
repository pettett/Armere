using System.Collections;
using System.Collections.Generic;
using Armere.Inventory;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "FlyerTemplate", menuName = "Game/Animals/Flyer Template", order = 0)]
public class FlyerTemplate : ScriptableObject
{
	public GameObject prefab;
	public AssetReferenceT<ItemData> item;
	public float speed;
	public float noiseScale;
	public float noiseForce = 1;
	public float centerForce = 0.5f;
	public float velocitySmoothTime = 1;
	//Panic is movement away from noises
	public float panicTime = 1f;
	public float panicSpeed = 1f;
	public bool dissapearAfterPanic = true;
}