
using UnityEngine;

public abstract class MapUI : MonoBehaviour
{
    MapMarker[] markers;
    public float mapScale;
    public RectTransform map;
    public Terrain terrain;
    protected virtual void Start()
    {
        markers = FindObjectsOfType<MapMarker>();
    }
    protected virtual void Update()
    {
        map.sizeDelta = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z) * mapScale;
    }
}