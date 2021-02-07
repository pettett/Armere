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
[ExecuteAlways]
public class TimeDayController : MonoBehaviour
{
	public static readonly int shader_SunDir = Shader.PropertyToID("_SunDir");
	public static readonly int shader_SunTangent = Shader.PropertyToID("_SunTangent");
	public static readonly int shader_SunCoTangent = Shader.PropertyToID("_SunCoTangent");

	public static readonly int shader_DaySkyColor = Shader.PropertyToID("_DaySkyColor");
	public static readonly int shader_NightSkyColor = Shader.PropertyToID("_NightSkyColor");
	public static readonly int shader_SkyColorTransitionPeriod = Shader.PropertyToID("_SkyColorTransitionPeriod");

	const float degreesPerHour = 360 / 24;
	const float oneOver24 = 1f / 24f;

	public static TimeDayController singleton;
	//current weather will depend on the weather scaler
	public WeatherConditions weather;

	public float maxSunnyScaler = 0.4f;
	public float maxCloudyScaler = 0.6f;
	public float maxWindyScaler = 0.8f;

	[Header("Sun")]
	Transform sun;
	DebugMenu.DebugEntry<float, float> entry;
	public float hour = 12;

	public float azimuth = 20;
	public Vector2 sunDisabledPeriod = new Vector2(22, 4);

	[Header("Time")]
	public FloatEventChannelSO changeTime;
	public float hoursPerSecond = 1;
	[Header("Wind")]

	public Vector2 windStrengthRange = new Vector2(0.2f, 3f);
	public float windChangingSpeed = 2;
	public Vector3 Wind { get; private set; }


	[Header("Weather")]

	public AnimationCurve weatherOverTime;
	public Gradient fogOverDay;
	public Vector2 fogDensityRange;
	[ReadOnly] public float weatherScaler;

	[Header("Skybox Colours")]
	public Color daySkyColour = new Color(0.6038275f, 1, 1);
	public Color nightSkyColour = new Color(0, 0, 0);
	public Vector2 skyColorTransitionPeriod = new Vector2(0.3f, -0.3f);

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

		sun = RenderSettings.sun.transform;
		if (Application.isPlaying)
		{
			changeTime.OnEventRaised += ChangeTime;
			entry = DebugMenu.CreateEntry("Game", "Time: {0:00}:{1:00}", 0f, 0f);
		}

		RenderSettings.skybox.SetColor(shader_DaySkyColor, daySkyColour);
		RenderSettings.skybox.SetColor(shader_NightSkyColor, nightSkyColour);
		RenderSettings.skybox.SetVector(shader_SkyColorTransitionPeriod, skyColorTransitionPeriod);

	}
	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			changeTime.OnEventRaised -= ChangeTime;
			DebugMenu.RemoveEntry(entry);
		}

	}
	public void ChangeTime(float newTime)
	{
		hour = Mathf.Repeat(newTime, 24);
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

		if (Application.isPlaying)
		{
			if (DebugMenu.menuEnabled)
			{
				entry.value0 = Mathf.Floor(hour);
				entry.value1 = 60f * (hour - Mathf.Floor(hour));
			}

			hour += Time.deltaTime * hoursPerSecond;

			hour = Mathf.Repeat(hour, 24);

			sun.gameObject.SetActive(hour > sunDisabledPeriod.x && hour < sunDisabledPeriod.y);

			//take 90 so the 0th hour is pointing straight down (-90)
			if (sun != null)
				sun.rotation = Quaternion.Euler(0, 90, azimuth) * Quaternion.Euler(hour * degreesPerHour - 90, 0, 0);


			float colorTransition = Mathf.SmoothStep(skyColorTransitionPeriod.x, skyColorTransitionPeriod.y, -sun.forward.y);
			RenderSettings.ambientSkyColor = Color.Lerp(daySkyColour, nightSkyColour, colorTransition);
		}



		Color fog = fogOverDay.Evaluate(hour * oneOver24);

		RenderSettings.fogColor = fog;
		RenderSettings.fogDensity = fog.a * (fogDensityRange.y - fogDensityRange.x) + fogDensityRange.x;

		RenderSettings.skybox.SetVector(shader_SunDir, -sun.forward);
		RenderSettings.skybox.SetVector(shader_SunTangent, -sun.right);
		RenderSettings.skybox.SetVector(shader_SunCoTangent, -sun.up);

		//skyboxMaterial.SetColor("_GroundColor", fog);

		//Update wind
		var w = new Vector2(
			Mathf.Cos(Mathf.PerlinNoise(Time.time * windChangingSpeed, 1.5f) * 2 * Mathf.PI),
			Mathf.Sin(Mathf.PerlinNoise(Time.time * windChangingSpeed, 1.5f) * 2 * Mathf.PI)) *
			Mathf.Lerp(windStrengthRange.x, windStrengthRange.y, Mathf.PerlinNoise(Time.time * windChangingSpeed, 0.5f));

		Wind = new Vector3(w.x, 0, w.y);




	}
}
