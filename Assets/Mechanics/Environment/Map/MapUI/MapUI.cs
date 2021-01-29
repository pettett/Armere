
using UnityEngine;

public abstract class MapUI : MonoBehaviour
{
	MapMarker[] markers;
	public float mapScale;
	public RectTransform map;
	public Map mapObject;

	public RectTransform playerPositionIndicator;
	public RectTransform playerViewIndicator;

	protected virtual void Start()
	{
		markers = FindObjectsOfType<MapMarker>();
	}
	protected virtual void Update()
	{
		if (mapObject == null)
		{
			gameObject.SetActive(false);
			return;
		}
		map.sizeDelta = new Vector2(mapObject.contours.terrain.size.x, mapObject.contours.terrain.size.z) * mapScale;
	}
}