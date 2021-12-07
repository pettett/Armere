using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyListener : MonoBehaviour
{
	public InputAction key;
	public VoidEventChannelSO action;

	// Start is called before the first frame update
	void Start()
	{
		key.performed += _ => action.RaiseEvent();
	}

	private void OnEnable()
	{
		key.Enable();
	}
	private void OnDisable()
	{
		key.Disable();
	}

}
