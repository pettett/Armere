using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map")]
public class Map : ScriptableObject
{

    [System.Serializable]
    public class Region
    {
        public string name = "New Region";
        public Vector2[] shape;
        public float priority = 0;
        public float blendDistance = 1;
        public int[] triangles;
        public Rect bounds;
        [Tooltip("Track to override from a lower priority region below or from world audio")]
        public MusicTrack trackOverride;
        public void UpdateBounds()
        {
            Vector2 min = shape[0];
            Vector2 max = shape[0];
            if (shape.Length > 1)
                for (int i = 1; i < shape.Length; i++)
                {
                    min.x = Mathf.Min(min.x, shape[i].x);
                    min.y = Mathf.Min(min.y, shape[i].y);
                    max.x = Mathf.Max(max.x, shape[i].x);
                    max.y = Mathf.Max(max.y, shape[i].y);
                }
            min -= Vector2.one * blendDistance;
            max += Vector2.one * blendDistance;
            bounds = new Rect(min, max - min);
        }
    }
    public Texture2D texture;
    public Region[] regions;
}
