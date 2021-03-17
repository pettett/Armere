using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMap : MonoBehaviour
{
	public Map map;
	public static SceneMap instance;

	private void Awake()
	{
		instance = this;
	}
}
