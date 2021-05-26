using System.Collections;
using System.Collections.Generic;
using Armere.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
public class GodCamera : MonoBehaviour
{
	public InputReader input;
	public float speed = 5;
	public float sensitivity = 10;

	public TMPro.TextMeshProUGUI startTooltip;
	public TMPro.TextMeshProUGUI endTooltip;
	public UnityEngine.UI.Image image;

	private void Start()
	{
		endTooltip.gameObject.SetActive(false);
		image.gameObject.SetActive(false);
		cam = GetComponent<Camera>();
	}
	bool dragging = false;
	bool looking = false;
	bool canDrag = false;
	Camera cam;

	Object start;


	// Update is called once per frame
	void Update()
	{
		transform.position += speed * transform.TransformDirection(new Vector3(input.horizontalMovement.x, input.verticalMovement, input.horizontalMovement.y)) * Time.deltaTime;


		if (Mouse.current.rightButton.isPressed)
		{
			if (!looking)
			{
				looking = true;
			}

			Vector3 rot = transform.localEulerAngles;
			Vector2 d = Mouse.current.delta.ReadValue() * sensitivity;
			rot.x -= d.y;
			rot.y += d.x;
			transform.localEulerAngles = rot;
		}
		else if (Mouse.current.leftButton.isPressed && canDrag)
		{
			//Dragging start
			if (!dragging)
			{
				dragging = true;
				UpdateStartTooltip();
				endTooltip.gameObject.SetActive(true);
				image.gameObject.SetActive(true);
			}

			UpdateEndTooltip();
		}
		else if (dragging)
		{
			dragging = false;
			ExecuteDragInstruction();
		}
		else if (!dragging)
		{
			if (looking)
			{
				looking = false;
			}
			UpdateStartTooltip();
		}
	}
	void UpdateStartTooltip()
	{
		Vector2 p = Mouse.current.position.ReadValue();
		startTooltip.rectTransform.anchoredPosition = p;
		string label = null;

		if (Physics.Raycast(cam.ScreenPointToRay(p), out RaycastHit hit, Mathf.Infinity, -1, QueryTriggerInteraction.Collide))
		{
			if (hit.collider.TryGetComponent<AIHumanoid>(out AIHumanoid npc))
			{
				if (npc.currentState is NPCRoutine routine)
				{
					start = npc;
					label = routine.t.name;
					canDrag = true;
				}
			}
		}
		if (label != null)
		{

			startTooltip.gameObject.SetActive(true);
			startTooltip.SetText(label);
		}
		else
		{
			startTooltip.gameObject.SetActive(false);
		}
	}
	void UpdateEndTooltip()
	{
		Vector2 s;

		if (start is MonoBehaviour m)
		{
			s = cam.WorldToScreenPoint(m.transform.position);
			startTooltip.rectTransform.anchoredPosition = s;
		}
		else
		{
			s = startTooltip.rectTransform.anchoredPosition;
		}


		Vector2 e = Mouse.current.position.ReadValue();

		string label = null;

		if (Physics.Raycast(cam.ScreenPointToRay(e), out RaycastHit hit, Mathf.Infinity, -1, QueryTriggerInteraction.Collide))
		{
			//assume goto command
			if (start is AIHumanoid)
			{
				if (hit.collider.TryGetComponent<InteractableItem>(out var item))
				{
					label = $"Pickup {item.item.displayName}";
				}
				else
				{
					label = "GoTo";
				}
			}
		}


		if (label != null)
		{
			endTooltip.gameObject.SetActive(true);
			endTooltip.SetText(label);
		}
		else
		{
			endTooltip.gameObject.SetActive(false);
		}

		endTooltip.rectTransform.anchoredPosition = e;
		//join line to start tooltip to show relation

		image.rectTransform.sizeDelta = new Vector2(10, Vector2.Distance(s, e));
		image.rectTransform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, e - s));
		image.rectTransform.anchoredPosition = s;

	}

	void ExecuteDragInstruction()
	{
		Vector2 p = Mouse.current.position.ReadValue();
		if (Physics.Raycast(cam.ScreenPointToRay(p), out RaycastHit hit, Mathf.Infinity, -1, QueryTriggerInteraction.Collide))
		{
			//assume goto command
			if (start is AIHumanoid h)
			{
				if (hit.collider.TryGetComponent<InteractableItem>(out var item))
				{
					h.StartCoroutine(h.PickupItemRoutine(item));
				}
				else
				{
					h.GoToPosition(hit.point);
				}
			}
		}
		start = null;
		canDrag = false;
		endTooltip.gameObject.SetActive(false);
		image.gameObject.SetActive(false);
	}

}
