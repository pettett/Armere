using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Text;
public class DebugMenu : MonoBehaviour
{
	public InputReader inputReader;
	public TextMeshProUGUI topLeft;

	[System.Serializable]
	public struct DebugGroup
	{
		public bool showTitle;
		public string name;
	}


	public List<StringBuilder>[] entries;
	[HideInInspector]
	public DebugGroup[] groups;

	static DebugMenu singleton;

	public static bool menuEnabled = false;

	const float fpsMeasurePeriod = 0.5f;
	private uint m_FpsAccumulator = 0;
	private float m_FpsNextPeriod = 0;
	private uint m_CurrentFps;

	StringBuilder fps;


	private void Awake()
	{
		singleton = this;
		//setup the lists of debug entries
		entries = new List<StringBuilder>[groups.Length];
		for (int i = 0; i < entries.Length; i++)
			entries[i] = new List<StringBuilder>(1);
	}



	private void Start()
	{

		fps = CreateEntry(0);

		m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;

		inputReader.showReadoutScreenEvent += ToggleScreen;

		ToggleScreen(InputActionPhase.Performed);
	}
	private void OnDestroy()
	{
		inputReader.showReadoutScreenEvent -= ToggleScreen;
	}


	void ToggleScreen(InputActionPhase phase)
	{
		if (phase == InputActionPhase.Performed)
		{
			enabled = !enabled;
			menuEnabled = enabled;
			topLeft.gameObject.SetActive(enabled);
		}
	}




	public static int GroupFromName(string groupName)
	{
		if (singleton == null)
		{
			return -1;
		}

		for (int i = 0; i < singleton.groups.Length; i++)
		{
			if (singleton.groups[i].name == groupName)
				return i;
		}
		return -1;
	}

	public static StringBuilder CreateEntry(string groupName) => singleton?._CreateEntry(GroupFromName(groupName));
	public static StringBuilder CreateEntry(int group) => singleton?._CreateEntry(group);

	StringBuilder _CreateEntry(int group)
	{
		StringBuilder entry = new StringBuilder();
		RegisterEntry(group, entry);
		return entry;
	}



	void RegisterEntry(int group, StringBuilder entry)
	{
		if (!(group >= 0 && group < entries.Length)) throw new System.Exception(string.Format("Group {0} does not exist", group));
		//Add to the estimate of how long the final string will be
		entries[group].Add(entry);
	}

	public static void RemoveEntry(StringBuilder entry)
	{
		if (singleton != null && entry != null)
		{
			//remove this entry from the pile
			for (int i = 0; i < singleton.entries.Length; i++) //Once removed, break loop
				if (singleton.entries[i].Remove(entry)) return;
		}
	}

	StringBuilder topLeftString = new StringBuilder();
	private void Update()
	{
		// measure average frames per second
		m_FpsAccumulator++;
		if (Time.realtimeSinceStartup > m_FpsNextPeriod)
		{
			m_CurrentFps = (uint)(m_FpsAccumulator / fpsMeasurePeriod);
			m_FpsAccumulator = 0;
			m_FpsNextPeriod += fpsMeasurePeriod;

			fps.Clear();
			fps.Append("FPS: ");
			fps.Append(m_CurrentFps);
		}

		topLeftString.Clear();

		for (int i = 0; i < entries.Length; i++)
		{
			//show the title of the group
			if (groups[i].showTitle)
				topLeftString.AppendLine(groups[i].name);



			for (int j = 0; j < entries[i].Count; j++)
			{
				topLeftString.EnsureCapacity(topLeftString.Length + entries[i][j].Length);
				for (int k = 0; k < entries[i][j].Length; k++)
				{
					topLeftString.Append(entries[i][j][k]);
				}
				topLeftString.AppendLine();
			}
			topLeftString.AppendLine();
		}

		topLeft.text = topLeftString.ToString();
	}
}
