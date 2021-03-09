using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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

	public static readonly int shader_SkyColor = Shader.PropertyToID("_SkyColor");

	[System.Serializable]
	public struct ColorSet
	{
		[ColorUsage(false)] public Color skyColor, equatorColor, groundColor, skyboxColor;
		public static ColorSet LerpAmbient(in ColorSet a, in ColorSet b, float t) => new ColorSet(
			Color.Lerp(a.skyColor, b.skyColor, t),
			Color.Lerp(a.equatorColor, b.equatorColor, t),
			Color.Lerp(a.groundColor, b.groundColor, t),
			default
		);
		public static ColorSet LerpSkybox(in ColorSet a, in ColorSet b, float t) => new ColorSet(
			default,
			default,
			default,
			Color.Lerp(a.skyboxColor, b.skyboxColor, t)
		);
		public ColorSet(Color skyColour, Color equatorColour, Color groundColour, Color skyboxColor)
		{
			this.skyColor = skyColour;
			this.equatorColor = equatorColour;
			this.groundColor = groundColour;
			this.skyboxColor = skyboxColor;
		}
		public void SetAmbient()
		{
			RenderSettings.ambientSkyColor = skyColor;
			RenderSettings.ambientEquatorColor = equatorColor;
			RenderSettings.ambientGroundColor = groundColor;
		}
	}

	const float degreesPerHour = 360 / 24;
	const float oneOver24 = 1f / 24f;

	public static TimeDayController singleton;
	//current weather will depend on the weather scaler
	public WeatherConditions weather;

	public float maxSunnyScaler = 0.4f;
	public float maxCloudyScaler = 0.6f;
	public float maxWindyScaler = 0.8f;


	Transform sun => RenderSettings.sun.transform;
	[Header("Sun")]
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

	public GlobalVector3SO windDirection;


	[Header("Weather")]

	public AnimationCurve weatherOverTime;
	public Gradient fogOverDay;
	public Vector2 fogDensityRange;
	public float rainIntensity;
	public VisualEffect rain;
	public float maxRainPerSecondPerArea = 30;
	[ReadOnly] public float weatherScaler;

	[Header("Skybox Colours")]
	public ColorSet day;
	public ColorSet night;
	public ColorSet dayRain;
	public ColorSet nightRain;
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


		if (Application.isPlaying)
		{
			changeTime.OnEventRaised += ChangeTime;
			entry = DebugMenu.CreateEntry("Game", "Time: {0:00}:{1:00}", 0f, 0f);
		}



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
	public float GetColorTransition() => Mathf.InverseLerp(skyColorTransitionPeriod.x, skyColorTransitionPeriod.y, -sun.forward.y);

	public Color GetSkyboxColor()
	{
		float colorTransition = GetColorTransition();
		ColorSet colors;
		if (rainIntensity == 0)
			colors = ColorSet.LerpSkybox(day, night, colorTransition);
		else if (rainIntensity == 1)
			colors = ColorSet.LerpSkybox(dayRain, nightRain, colorTransition);
		else
			colors = ColorSet.LerpSkybox(
			   ColorSet.LerpSkybox(day, dayRain, rainIntensity),
			   ColorSet.LerpSkybox(night, nightRain, rainIntensity),
			   colorTransition
		   );

		return colors.skyboxColor;
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

		}

		hour = Mathf.Repeat(hour, 24);

		float colorTransition = GetColorTransition();

		ColorSet colors;
		if (rainIntensity == 0)
			colors = ColorSet.LerpAmbient(day, night, colorTransition);
		else if (rainIntensity == 1)
			colors = ColorSet.LerpAmbient(dayRain, nightRain, colorTransition);
		else
			colors = ColorSet.LerpAmbient(
			   ColorSet.LerpAmbient(day, dayRain, rainIntensity),
			   ColorSet.LerpAmbient(night, nightRain, rainIntensity),
			   colorTransition
		   );

		if (rainIntensity != 0)
		{
			rain.gameObject.SetActive(true);
			rain.SetFloat("Drops Per Second Per Area", rainIntensity * maxRainPerSecondPerArea);
		}
		else
		{
			rain.gameObject.SetActive(false);
		}


		colors.SetAmbient();


		RenderSettings.skybox.SetColor(shader_SkyColor, colors.skyColor);

		UpdateSunPosition();


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
		windDirection.value = Wind;

	}
	[MyBox.ButtonMethod]
	public void UpdateSunPosition()
	{

		RenderSettings.sun.gameObject.SetActive(hour > sunDisabledPeriod.x && hour < sunDisabledPeriod.y);

		//take 90 so the 0th hour is pointing straight down (-90)
		RenderSettings.sun.transform.rotation = Quaternion.Euler(0, 90, azimuth) * Quaternion.Euler(hour * degreesPerHour - 90, 0, 0);
	}
}
