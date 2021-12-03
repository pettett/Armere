using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputVisualizer : MonoBehaviour
{
	public InputAction button = new InputAction("button", InputActionType.Button);
	public Image image;


	// Start is called before the first frame update
	void Start()
	{
		button.Enable();

		button.started += context => Debug.Log($"{context.action} started");
		button.performed += context => Debug.Log($"{context.action} performed");
		button.canceled += context => Debug.Log($"{context.action} canceled");

	}


}
