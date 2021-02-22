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

	public abstract class DebugEntryBase
	{
		public string format;
		public abstract string Line { get; }
	}

	public class DebugEntry : DebugEntryBase
	{
		public object[] values;
		public DebugEntry(string format, params object[] values)
		{
			this.format = format;
			this.values = values;
		}
		public override string Line => System.String.Format(format, values);
	}

	public class DebugEntry<T1> : DebugEntryBase
	{
		public T1 value0;
		public DebugEntry(string format, T1 value0)
		{
			this.format = format;
			this.value0 = value0;
		}
		public override string Line => System.String.Format(format, value0);
	}
	public class DebugEntry<T0, T1> : DebugEntry<T0>
	{
		public T1 value1;
		public DebugEntry(string format, T0 value0, T1 value1) : base(format, value0)
		{
			this.value1 = value1;
		}
		public override string Line => System.String.Format(format, value0, value1);
	}
	public class DebugEntry<T0, T1, T2> : DebugEntry<T0, T1>
	{
		public T2 value2;
		public DebugEntry(string format, T0 value0, T1 value1, T2 value2) : base(format, value0, value1)
		{
			this.value2 = value2;
		}
		public override string Line => System.String.Format(format, value0, value1, value2);
	}





	[System.Serializable]
	public struct DebugGroup
	{
		public bool showTitle;
		public string name;
	}


	public List<DebugEntryBase>[] entries;
	[HideInInspector]
	public DebugGroup[] groups;

	static DebugMenu singleton;

	public static bool menuEnabled = false;

	const float fpsMeasurePeriod = 0.5f;
	private uint m_FpsAccumulator = 0;
	private float m_FpsNextPeriod = 0;
	private uint m_CurrentFps;

	DebugEntry<float> fps;

	int currentEntryCharacters = 0;

	private void Awake()
	{
		singleton = this;
		//setup the lists of debug entries
		entries = new List<DebugEntryBase>[groups.Length];
		for (int i = 0; i < entries.Length; i++)
			entries[i] = new List<DebugEntryBase>(1);
	}



	private void Start()
	{

		fps = CreateEntry(0, "FPS: {0}", 0f);

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
		for (int i = 0; i < singleton.groups.Length; i++)
		{
			if (singleton.groups[i].name == groupName)
				return i;
		}
		return -1;
	}

	public static DebugEntry CreateEntry(string groupName, string format, params object[] values) => singleton._CreateEntry(GroupFromName(groupName), format, values);
	public static DebugEntry CreateEntry(int group, string format, params object[] values) => singleton._CreateEntry(group, format, values);

	DebugEntry _CreateEntry(int group, string format, object[] values)
	{
		DebugEntry entry = new DebugEntry(format, values);
		RegisterEntry(group, entry);
		return entry;
	}

	public static DebugEntry<T1> CreateEntry<T1>(string groupName, string format, T1 value1) => singleton._CreateEntry<T1>(GroupFromName(groupName), format, value1);
	public static DebugEntry<T1> CreateEntry<T1>(int group, string format, T1 value1) => singleton._CreateEntry<T1>(group, format, value1);
	DebugEntry<T1> _CreateEntry<T1>(int group, string format, T1 value1)
	{
		DebugEntry<T1> entry = new DebugEntry<T1>(format, value1);
		RegisterEntry(group, entry);
		return entry;
	}

	public static DebugEntry<T0, T1> CreateEntry<T0, T1>(string groupName, string format, T0 value0, T1 value1) => singleton._CreateEntry<T0, T1>(GroupFromName(groupName), format, value0, value1);
	public static DebugEntry<T0, T1> CreateEntry<T0, T1>(int group, string format, T0 value0, T1 value1) => singleton._CreateEntry<T0, T1>(group, format, value0, value1);
	DebugEntry<T0, T1> _CreateEntry<T0, T1>(int group, string format, T0 value0, T1 value1)
	{
		DebugEntry<T0, T1> entry = new DebugEntry<T0, T1>(format, value0, value1);
		RegisterEntry(group, entry);
		return entry;
	}

	public static DebugEntry<T0, T1, T2> CreateEntry<T0, T1, T2>(string groupName, string format, T0 value0, T1 value1, T2 value2) => singleton._CreateEntry<T0, T1, T2>(GroupFromName(groupName), format, value0, value1, value2);
	public static DebugEntry<T0, T1, T2> CreateEntry<T0, T1, T2>(int group, string format, T0 value0, T1 value1, T2 value2) => singleton._CreateEntry<T0, T1, T2>(group, format, value0, value1, value2);
	DebugEntry<T0, T1, T2> _CreateEntry<T0, T1, T2>(int group, string format, T0 value0, T1 value1, T2 value2)
	{
		DebugEntry<T0, T1, T2> entry = new DebugEntry<T0, T1, T2>(format, value0, value1, value2);
		RegisterEntry(group, entry);
		return entry;
	}



	void RegisterEntry(int group, DebugEntryBase entry)
	{
		if (!(group >= 0 && group < entries.Length)) throw new System.Exception(string.Format("Group {0} does not exist", group));
		//Add to the estimate of how long the final string will be
		currentEntryCharacters += entry.format.Length;
		entries[group].Add(entry);
	}

	public static void RemoveEntry(DebugEntryBase entry)
	{
		if (singleton != null && entry != null)
		{
			//remove this entry from the pile
			singleton.currentEntryCharacters -= entry.format.Length;
			for (int i = 0; i < singleton.entries.Length; i++) //Once removed, break loop
				if (singleton.entries[i].Remove(entry)) return;
		}
	}

	private void Update()
	{
		// measure average frames per second
		m_FpsAccumulator++;
		if (Time.realtimeSinceStartup > m_FpsNextPeriod)
		{
			m_CurrentFps = (uint)(m_FpsAccumulator / fpsMeasurePeriod);
			m_FpsAccumulator = 0;
			m_FpsNextPeriod += fpsMeasurePeriod;
			fps.value0 = m_CurrentFps;
		}

		StringBuilder topLeftString = new StringBuilder(currentEntryCharacters);


		for (int i = 0; i < entries.Length; i++)
		{
			//show the title of the group
			if (groups[i].showTitle)
				topLeftString.AppendLine(groups[i].name);

			for (int j = 0; j < entries[i].Count; j++)
			{
				topLeftString.AppendLine(entries[i][j].Line);
			}
			topLeftString.AppendLine();
		}

		topLeft.text = topLeftString.ToString();
	}
}
