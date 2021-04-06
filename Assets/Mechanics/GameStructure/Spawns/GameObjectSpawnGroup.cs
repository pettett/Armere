using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSpawnGroup : MonoBehaviour
{
	public static GameObjectSpawnGroup singleton;
	private void Awake()
	{
		singleton = this;
	}
}
