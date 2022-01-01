using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Console
{
	public class SceneCommands : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{
			Console.RegisterCommand("timescale", a => { Time.timeScale = (float)a[0]; }, "float");
			Console.RegisterCommand("play", _ => { Time.timeScale = 1; });
			Console.RegisterCommand("pause", _ => { Time.timeScale = 0; });
		}


	}
}
