using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
public class DebugMenu : MonoBehaviour
{
    public InputAction screenToggle = new InputAction("screenToggle", InputActionType.Button, "<Keyboard>/f3");

    public TextMeshProUGUI topLeft;

    public class DebugEntry
    {
        public string format;
        public object[] values;
        public DebugEntry(string format, params object[] values)
        {
            this.format = format;
            this.values = values;
        }
        public string Line => System.String.Format(format, values);
    }
    [System.Serializable]
    public struct DebugGroup
    {
        public bool showTitle;
        public string name;
    }


    public List<DebugEntry>[] entries;
    [HideInInspector]
    public DebugGroup[] groups;

    static DebugMenu singleton;



    const float fpsMeasurePeriod = 0.5f;
    private uint m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private uint m_CurrentFps;

    private void Awake()
    {
        singleton = this;
        //setup the lists of debug entries
        entries = new List<DebugEntry>[groups.Length];
        for (int i = 0; i < entries.Length; i++)
            entries[i] = new List<DebugEntry>(1);
    }

    private void Start()
    {



        fps = CreateEntry(0, "FPS: {0}", 0);

        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        screenToggle.Enable();

        screenToggle.performed += ToggleScreen;
        ToggleScreen(default(InputAction.CallbackContext));
    }

    void ToggleScreen(InputAction.CallbackContext c)
    {
        enabled = !enabled;
        topLeft.gameObject.SetActive(enabled);
    }


    DebugEntry fps;


    public static int GroupFromName(string groupName)
    {
        for (int i = 0; i < singleton.groups.Length; i++)
        {
            if (singleton.groups[i].name == groupName)
                return i;
        }
        return -1;
    }
    public static DebugEntry CreateEntry(string groupName, string format, params object[] values)
    {
        return singleton._CreateEntry(GroupFromName(groupName), format, values);
    }
    public static DebugEntry CreateEntry(int group, string format, params object[] values)
    {
        return singleton._CreateEntry(group, format, values);
    }

    DebugEntry _CreateEntry(int group, string format, object[] values)
    {
        DebugEntry entry = new DebugEntry(format, values);
        if (!(group >= 0 && group < entries.Length)) throw new System.Exception(string.Format("Group {0} does not exist", group));
        entries[group].Add(entry);
        return entry;
    }

    public static void RemoveEntry(DebugEntry entry)
    {
        for (int i = 0; i < singleton.entries.Length; i++)
        {
            singleton.entries[i].Remove(entry);
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
            fps.values[0] = m_CurrentFps;
        }




        topLeft.text = "";
        for (int i = 0; i < entries.Length; i++)
        {
            //show the title of the group
            if (groups[i].showTitle)
                topLeft.text += groups[i].name + "\n";

            for (int j = 0; j < entries[i].Count; j++)
            {
                topLeft.text += entries[i][j].Line;
                topLeft.text += "\n";
            }
            topLeft.text += "\n";
        }
    }
}
