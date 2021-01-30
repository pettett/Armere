
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class MapUI : MonoBehaviour
{
	MapMarker[] markers;
	public float mapScale;
	public RectTransform map;
	public Map mapObject;

	public RectTransform playerPositionIndicator;
	public RectTransform playerViewIndicator;
	public Material mapBackground;
	public bool focusOnPlayer = false;
	public float[] zoomStages = new float[]{
		1,3,5
	};
	int zoomStage = 2;
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
		markers = FindObjectsOfType<MapMarker>();
		mapBackground.SetTexture("_HeightTex", mapObject.contours.terrain.heightmapTexture);
		UpdateZoom();
	}
	/// <summary>
	/// Turns world position into map UI
	/// </summary>
	/// <param name="position">World Space Coordinate</param>
	/// <returns></returns>
	public Vector2 WorldToMapSpace(Vector3 position)
	{
		return new Vector2(position.x / mapObject.contours.terrain.size.x, position.z / mapObject.contours.terrain.size.z);
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
		return new Vector2(worldSpace.x, worldSpace.z) * -mapScale + map.anchoredPosition;
	}

	protected void Update()
	{
		if (mapObject == null)
		{
			gameObject.SetActive(false);
			return;
		}
		map.sizeDelta = new Vector2(mapObject.contours.terrain.size.x, mapObject.contours.terrain.size.z) * mapScale;

		if (focusOnPlayer)
		{
			//Move the map object to focus on player's position
			Vector3 trackPos = LevelInfo.currentLevelInfo.playerTransform.position;
			map.anchoredPosition = new Vector2(trackPos.x, trackPos.z) * mapScale;
		}
		else
		{
			//Move the player to focus on the map
			Vector3 trackPos = LevelInfo.currentLevelInfo.playerTransform.position;
			Vector2 mapPos = WorldSpaceToUIPosition(trackPos);
			playerViewIndicator.anchoredPosition = mapPos;
			playerPositionIndicator.anchoredPosition = mapPos;
		}

		float playerLookDir = 180 - LevelInfo.currentLevelInfo.playerTransform.eulerAngles.y;
		float cameraLookDir = 180 - Camera.main.transform.eulerAngles.y;

		playerPositionIndicator.eulerAngles = Vector3.forward * playerLookDir;
		playerViewIndicator.eulerAngles = Vector3.forward * cameraLookDir;

	}
}