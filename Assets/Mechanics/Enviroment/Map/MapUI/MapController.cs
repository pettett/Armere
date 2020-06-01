using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapController : MapUI
{
    public float[] zoomStages = new float[]{
        1,3,5
    };
    int zoomStage = 0;
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
        Debug.Log("Dragging");
    }
    void UpdateZoom()
    {
        mapScale = zoomStages[zoomStage];
    }
    protected override void Update()
    {
        //Vector2 mousePos =EventSystem.current.
        base.Update();
    }
    protected override void Start()
    {
        UpdateZoom();
        base.Start();
    }
}
