using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum WeatherConditions
{
    Sunny,
    Cloudy,
    Windy,
    Stormy
}
public class TimeDayController : MonoBehaviour
{
    const float degreesPerHour = 360 / 24;
    const float oneOver24 = 1f / 24f;

    public Material skyboxMaterial;
    public static TimeDayController singleton;
    //current weather will depend on the weather scaler
    public WeatherConditions weather;

    public float maxSunnyScaler = 0.4f;
    public float maxCloudyScaler = 0.6f;
    public float maxWindyScaler = 0.8f;

    [Header("Sun")]
    public Transform sun;
    DebugMenu.DebugEntry<float, float> entry;
    public float hour = 12;
    public float hoursPerSecond = 1;
    public float azimuth = 20;

    [Header("Weather")]

    public AnimationCurve weatherOverTime;
    public Gradient fogOverDay;
    public Vector2 fogDensityRange;
    [ReadOnly] public float weatherScaler;

    public CloudDrawer clouds;

    public static readonly Dictionary<string, float> specialTimes = new Dictionary<string, float>
    {
        {"morning",4},
        {"noon",12},
        {"night",20},
    };

    public static void SetTime(string time)
    {
        if (specialTimes.ContainsKey(time))
            singleton.hour = specialTimes[time];
    }
    public static void SetTime(float hour)
    {
        hour = Mathf.Repeat(hour, 24);
        singleton.hour = hour;
    }
    private void Awake()
    {
        singleton = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        entry = DebugMenu.CreateEntry("Game", "Time: {0:00}:{1:00}", 0f, 0f);

    }

    // Update is called once per frame
    void Update()
    {
        weatherScaler = weatherOverTime.Evaluate(Time.time * hoursPerSecond);

        if (weatherScaler > maxWindyScaler)
            weather = WeatherConditions.Stormy;
        else if (weatherScaler > maxCloudyScaler)
            weather = WeatherConditions.Windy;
        else if (weatherScaler > maxSunnyScaler)
            weather = WeatherConditions.Cloudy;
        else
            weather = WeatherConditions.Sunny;


        if (clouds != null)
            clouds.SetCloudDensity(weatherScaler);

        hour += Time.deltaTime * hoursPerSecond;

        hour = Mathf.Repeat(hour, 24);
        //take 90 so the 0th hour is pointing straight down (-90)
        if (sun != null)
            sun.rotation = Quaternion.Euler(0, 90, azimuth) * Quaternion.Euler(hour * degreesPerHour - 90, 0, 0);

        Color fog = fogOverDay.Evaluate(hour * oneOver24);

        RenderSettings.fogColor = fog;
        RenderSettings.fogDensity = fog.a * (fogDensityRange.y - fogDensityRange.x) + fogDensityRange.x;

        skyboxMaterial.SetColor("_GroundColor", fog);

        entry.value0 = Mathf.Floor(hour);
        entry.value1 = 60f * (hour - Mathf.Floor(hour));

    }
}
