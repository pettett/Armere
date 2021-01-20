using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focusable : MonoBehaviour, IVisable
{
	public static List<Focusable> focusables = new List<Focusable>();

	public Transform indicatorAnchor;

	FocusableIndicatorUI indicatorUI;

	public Vector3 VisionPoint => indicatorAnchor.position;


	public bool inVision { get; set; } = false;

	// Start is called before the first frame update
	void Start()
	{
		focusables.Add(this);
		GameCameras.s.cameraVision.visionGroup.Add(this);

		indicatorUI = IndicatorsUIController.singleton.CreateFocusableIndicator(indicatorAnchor);

		indicatorUI.focusStateGraphic.canvasRenderer.SetAlpha(0);
	}
	private void OnDestroy()
	{
		focusables.Remove(this);
		GameCameras.s.cameraVision.visionGroup.Remove(this);
	}

	// Update is called once per frame
	void Update()
	{

	}
	public void OnFocus()
	{
		indicatorUI.SetFocused();
	}
	public void OnUnFocus()
	{
		indicatorUI.SetUnFocused();
	}

	public void OnEnterVision()
	{
		inVision = true;
		indicatorUI.focusStateGraphic.CrossFadeAlpha(1, 0.2f, true);
	}

	public void OnStayVision(float center, float distance)
	{


	}

	public void OnExitVision()
	{
		inVision = false;
		indicatorUI.focusStateGraphic.CrossFadeAlpha(0, 0.2f, true);
	}
}
