
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MapUI : MonoBehaviour
{
	MapMarker[] markers;
	public float mapScale;
	public RectTransform map;
	public RectTransform mapFrame;
	public GameObject trackingMarkerPrefab;

	public RectTransform playerPositionIndicator;
	public RectTransform playerViewIndicator;
	public Material mapBackground;
	public bool focusOnPlayer = false;
	public bool forceTargetsIntoFrame = false;
	public float[] zoomStages = new float[]{
		1,3,5
	};
	int zoomStage = 2;

	Transform[] trackingMarkerTargets;
	RectTransform[] trackingMarkers;

	public void ZoomMapIn()
	{
		zoomStage = Mathf.Min(zoomStages.Length - 1, zoomStage + 1);

		UpdateZoom();
	}
	public void ZoomMapOut()
	{
		zoomStage = Mathf.Max(0, zoomStage - 1);
		UpdateZoom();
	}
	public void OnMapDrag(BaseEventData d)
	{
		PointerEventData pointerData = (PointerEventData)d;

		map.anchoredPosition += pointerData.delta * 2.5f;
	}
	public void OnMapScroll(BaseEventData d)
	{
		PointerEventData pointerData = (PointerEventData)d;

		if (pointerData.scrollDelta.y > 0)
		{
			ZoomMapIn();
		}
		else
		{
			ZoomMapOut();
		}
	}

	void UpdateZoom()
	{
		if (zoomStages.Length > 0)
			mapScale = zoomStages[zoomStage];
	}

	protected void Start()
	{
		if (SceneMap.instance == null)
		{
			enabled = false;
			return;

		}
		markers = FindObjectsOfType<MapMarker>();

		if (SceneMap.instance.map.contours.terrain != null)
			mapBackground.SetTexture("_HeightTex", SceneMap.instance.map.contours.terrain.heightmapTexture);
		UpdateZoom();

		SceneMap.instance.map.onTrackingTargetsChanged += UpdateTrackingMarkers;
	}
	private void OnDestroy()
	{
		if (SceneMap.instance != null)
			SceneMap.instance.map.onTrackingTargetsChanged -= UpdateTrackingMarkers;
	}

	public void UpdateTrackingMarkers()
	{


		if (trackingMarkers != null)
			for (int i = 0; i < trackingMarkers.Length; i++)
			{
				Destroy(trackingMarkers[i].gameObject);
			}

		trackingMarkerTargets = SceneMap.instance.map.TrackingMarkers;

		if (trackingMarkerTargets != null)
		{
			trackingMarkers = new RectTransform[trackingMarkerTargets.Length];

			for (int i = 0; i < trackingMarkers.Length; i++)
			{
				trackingMarkers[i] = (RectTransform)Instantiate(trackingMarkerPrefab, map).transform;
			}
		}
		else
		{
			trackingMarkers = null;
		}
	}

	/// <summary>
	/// Turns world position into map UI
	/// </summary>
	/// <param name="position">World Space Coordinate</param>
	/// <returns></returns>
	public Vector2 WorldToMapSpace(Vector3 position)
	{
		return new Vector2(position.x / SceneMap.instance.map.contours.terrain.size.x, position.z / SceneMap.instance.map.contours.terrain.size.z);
	}
	/// <summary>
	/// Turns Map UV into position on UI
	/// </summary>
	/// <param name="mapSpace">Map UV</param>
	/// <returns></returns>
	public Vector2 MapSpaceToUIPosition(Vector2 mapSpace)
	{
		return mapSpace * mapScale;
	}

	public Vector2 WorldSpaceToUIPosition(Vector3 worldSpace)
	{
		Vector2 pos = new Vector2(worldSpace.x, worldSpace.z) * -mapScale;

		if (forceTargetsIntoFrame)
		{
			Rect clip = GetViewRect();
			pos.Set(Mathf.Clamp(pos.x, clip.xMin, clip.xMax), Mathf.Clamp(pos.y, clip.yMin, clip.yMax));
		}
		return pos;
	}

	public Rect GetViewRect()
	{
		Rect rect = new Rect();

		rect.size = mapFrame.rect.size;
		rect.center = -map.anchoredPosition;


		return rect;
	}

	protected void Update()
	{
		if (SceneMap.instance.map == null)
		{
			gameObject.SetActive(false);
			return;
		}

		if (LevelInfo.currentLevelInfo == null || LevelInfo.currentLevelInfo.playerTransform == null) return;


		map.sizeDelta = SceneMap.instance.map.contours.mapExtents * 2 * mapScale;



		//Move the player to focus on the map
		Vector3 trackPos = LevelInfo.currentLevelInfo.playerTransform.position;
		Vector2 mapPos = WorldSpaceToUIPosition(trackPos);
		playerViewIndicator.anchoredPosition = mapPos;
		playerPositionIndicator.anchoredPosition = mapPos;

		if (focusOnPlayer)
		{
			//Move the map object to focus on player's position
			map.anchoredPosition = new Vector2(trackPos.x, trackPos.z) * mapScale;
		}


		//Update tracking markers by moving to real world positions
		if (trackingMarkers != null)
		{
			for (int i = 0; i < trackingMarkers.Length; i++)
			{
				trackingMarkers[i].anchoredPosition = WorldSpaceToUIPosition(trackingMarkerTargets[i].position);
			}
		}

		float playerLookDir = 180 - LevelInfo.currentLevelInfo.playerTransform.eulerAngles.y;
		float cameraLookDir = 180 - Camera.main.transform.eulerAngles.y;

		playerPositionIndicator.eulerAngles = Vector3.forward * playerLookDir;
		playerViewIndicator.eulerAngles = Vector3.forward * cameraLookDir;

	}
}