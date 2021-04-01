using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour, IInteractable
{
	public bool canInteract { get => !moving && canOpen; set { canOpen = value; } }

	public float requiredLookDot => 0;

	public string interactionDescription => "Open";

	public string interactionName => null;

	public Vector3 worldOffset => default;

	public bool open;
	public bool moving = false;
	public bool canOpen = true;
	public bool canClose = true;

	public Vector3 closedPos = new Vector3(0, 0, 0);
	public Vector3 openPos = new Vector3(0, 0, 2.4f);


	public void Interact(IInteractor interactor)
	{

		if (open && canClose)
		{
			Close();
		}
		else if (!open && canOpen)
		{
			Open();
		}
	}

	public void Open()
	{
		StartCoroutine(MoveToPos(openPos));
		open = true;
	}
	public void Close()
	{
		StartCoroutine(MoveToPos(closedPos));
		open = false;
	}

	IEnumerator MoveToPos(Vector3 pos)
	{
		moving = true;
		Vector3 h = transform.localPosition;
		float t = 0;
		while (t < 1)
		{
			t += Time.deltaTime;
			transform.localPosition = Vector3.Lerp(h, pos, t);
			yield return null;
		}
		transform.localPosition = pos;
		moving = false;
	}

}
