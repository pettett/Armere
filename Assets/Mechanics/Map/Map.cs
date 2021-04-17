using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Game/Map")]
public class Map : ScriptableObject
{

	[System.Serializable]
	public class Region
	{
		public string name = "New Region";
		[HideInInspector] public Vector2[] shape = new Vector2[3];
		public float priority = 0;
		public float blendDistance = 1;
		[HideInInspector] public int[] triangles;
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

	private void OnEnable()
	{

		trackingQuests.onSelectedQuestStatusUpdated += OnSelectedQuestChanged;
	}
	private void OnDisable()
	{
		trackingQuests.onSelectedQuestStatusUpdated -= OnSelectedQuestChanged;
	}

	void OnSelectedQuestChanged(QuestStatus newSelection)
	{
		onTrackingTargetsChanged?.Invoke();
	}
	public event UnityAction onTrackingTargetsChanged;

	public Region[] regions;

	public ContourGenerator contours;
	public QuestManager trackingQuests;

	public Transform[] NPCTarget(string npc)
	{
		if (NPCManager.singleton.data.ContainsKey(npc))

			return new Transform[1] { NPCManager.singleton.data[npc].npcInstance };
		return null;
	}
	public Transform[] TrackingMarkers
	{
		get
		{
			if (trackingQuests.selectedQuest != null)
			{
				return trackingQuests.selectedQuest.questStage.type switch
				{
					var x when (x == Quest.QuestType.TalkTo || x == Quest.QuestType.Deliver) => NPCTarget(trackingQuests.selectedQuest.questStage.receiver),
					_ => null,
				};
			}
			else return null;
		}
	}
}
