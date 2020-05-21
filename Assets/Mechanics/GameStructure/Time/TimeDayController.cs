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
    //current weather will depend on the weather scaler
    public WeatherConditions weather;

    public float maxSunnyScaler = 0.4f;
    public float maxCloudyScaler = 0.6f;
    public float maxWindyScaler = 0.8f;


    public Transform sun;
    DebugMenu.DebugEntry entry;
    [ReadOnly] public float hour = 12;
    public float hoursPerSecond = 1;
    const float degreesPerHour = 360 / 24;

    public AnimationCurve weatherOverTime;
    [ReadOnly] public float weatherScaler;

    public CloudDrawer clouds;
    // Start is called before the first frame update
    void Start()
    {
        entry = DebugMenu.CreateEntry("Game", "Time: {0}", "00:00");
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



        clouds.SetCloudDensity(weatherScaler);

        hour += Time.deltaTime * hoursPerSecond;

        hour = Mathf.Repeat(hour, 24);
        //take 90 so the 0th hour is pointing straight down (-90)
        sun.rotation = Quaternion.Euler(hour * degreesPerHour - 90, -90, 0);

    }
}
