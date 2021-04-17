using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MapTracker : MonoBehaviour
{
	public StringEventChannelSO onRegionChanged;
	static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
	}

	public static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
	{
		float d1, d2, d3;
		bool has_neg, has_pos;

		d1 = sign(pt, v1, v2);
		d2 = sign(pt, v2, v3);
		d3 = sign(pt, v3, v1);

		has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
		has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

		return !(has_neg && has_pos);
	}

	public Map map => SceneMap.instance?.map;
	System.Text.StringBuilder entry;
	// Start is called before the first frame update
	void Start()
	{
		entry = DebugMenu.CreateEntry("Player");
	}
	int inRegion = -1;
	MusicTrack highestMusicTrack;

	struct RegionInfluence
	{
		public int region;
		public float influence;

		public RegionInfluence(int region, float influence)
		{
			this.region = region;
			this.influence = influence;
		}
	}
	public string currentRegion;


	List<RegionInfluence> regionInfluences = new List<RegionInfluence>();
	// Update is called once per frame
	void Update()
	{
		Vector2 pos = new Vector2(transform.position.x, transform.position.z);
		int prevRegion = inRegion;
		float highestVolume = 0;
		inRegion = -1;
		MusicTrack prevTrack = highestMusicTrack;

		highestMusicTrack = null;
		regionInfluences.Clear();


		for (int i = 0; i < map.regions.Length; i++)
		{
			//If highest music track is not null, a track has been found with greater importance then this
			if (inRegion != -1 && highestMusicTrack != null && map.regions[inRegion].priority > map.regions[i].priority) continue; //Only scan top priority layers

			if (map.regions[i].bounds.Contains(pos))
			{
				Vector2[] shape = map.regions[i].shape;

				for (int j = 0; j < map.regions[i].triangles.Length / 3; j++)
				{
					if (PointInTriangle(
						pos,
						shape[map.regions[i].triangles[j * 3]],
						shape[map.regions[i].triangles[j * 3 + 1]],
						shape[map.regions[i].triangles[j * 3 + 2]]))
					{
						inRegion = i;
						//Set this region's track to highest priority if it is not null
						highestMusicTrack = map.regions[i].trackOverride ?? highestMusicTrack;
						highestVolume = 1;
						regionInfluences.Add(new RegionInfluence(i, 1));
					}
				}

				if (!(inRegion == i))
				{
					//Not inside the region - test blending
					Vector2 closestPoint = VectorMath.ClosestPointOnPath(shape, pos, true);
					float sqrDist = (closestPoint - pos).sqrMagnitude;
					if (sqrDist < map.regions[i].blendDistance * map.regions[i].blendDistance)
					{
						//Set this region's track to highest priority if it is not null

						highestMusicTrack = map.regions[i].trackOverride ?? highestMusicTrack;
						if (map.regions[i].trackOverride != null)
						{
							highestVolume = 1 - sqrDist / (map.regions[i].blendDistance * map.regions[i].blendDistance);
							regionInfluences.Add(new RegionInfluence(i, highestVolume));
						}
						else
						{
							regionInfluences.Add(new RegionInfluence(i, 1 - sqrDist / (map.regions[i].blendDistance * map.regions[i].blendDistance)));
						}
					}
				}
			}
		}

		if (highestMusicTrack != null)
		{

			if (!MusicController.TrackPlaying(highestMusicTrack))
			{
				//A new music track is included with this new region
				MusicController.instance.Play(highestMusicTrack);
			}
			MusicController.SetTrackVolume(highestMusicTrack, highestVolume);
		}
		if (prevTrack != highestMusicTrack && prevTrack != null)
		{
			//Stop the previous track from playing
			MusicController.instance.Stop(prevTrack);
		}


		if (prevRegion != inRegion)
		{
			//New region, new name
			if (inRegion == -1)
				OnExitRegions();
			else
				OnChangeRegion(map.regions[inRegion].name);
		}


		if (DebugMenu.menuEnabled)
		{
			entry.Clear();
			entry.Append("Current Regions: ");
			if (regionInfluences.Count == 0)
			{
				entry.Append("Wilderness");
			}
			else
			{
				for (int i = 0; i < regionInfluences.Count; i++)
				{
					entry.AppendFormat("{0} : {1}", map.regions[regionInfluences[i].region].name.ToString(), regionInfluences[i].influence.ToString());
					entry.Append(' ');
				}
			}

		}
	}


	public void OnChangeRegion(string newRegion)
	{
		currentRegion = newRegion;
		LevelInfo.currentLevelInfo.currentRegionName = currentRegion;
		//If autosave has not been activated for a while, autosave
		SaveManager.singleton.AttemptAutoSave();
		onRegionChanged?.RaiseEvent(newRegion);
	}
	public void OnExitRegions()
	{
		currentRegion = "Wilderness";
		LevelInfo.currentLevelInfo.currentRegionName = currentRegion;
		//If autosave has not been activated for a while, autosave
		SaveManager.singleton.AttemptAutoSave();
	}

	private void OnDrawGizmos()
	{
		if (map == null) return;

		Vector2 pos = new Vector2(transform.position.x, transform.position.z);
		for (int i = 0; i < map.regions.Length; i++)
		{
			//If highest music track is not null, a track has been found with greater importance then this
			if (inRegion != -1 && highestMusicTrack != null && map.regions[inRegion].priority > map.regions[i].priority) continue; //Only scan top priority layers

			if (map.regions[i].bounds.Contains(pos))
			{


				//Not inside the region - test blending
				Vector2 closestPoint = VectorMath.ClosestPointOnPath(map.regions[i].shape, pos, true);
				float sqrDist = (closestPoint - pos).sqrMagnitude;
				if (sqrDist < map.regions[i].blendDistance * map.regions[i].blendDistance)
				{
					Gizmos.DrawWireSphere(new Vector3(closestPoint.x, 0, closestPoint.y), 0.05f);
				}

			}
		}

	}
}
