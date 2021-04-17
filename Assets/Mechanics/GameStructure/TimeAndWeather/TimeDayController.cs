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
	public static readonly int shader_SunDir = Shader.PropertyToID("_SunDir"),
		shader_SunTangent = Shader.PropertyToID("_SunTangent"),
		shader_SunCoTangent = Shader.PropertyToID("_SunCoTangent"),
		shader_CloudsPosition = Shader.PropertyToID("_CloudsPosition"),
		shader_SkyColor = Shader.PropertyToID("_SkyColor");

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
	[System.Serializable]
	public struct CloudProfile
	{
		public Vector2 octavesCoverage;
		public float scale;

		public CloudProfile(Vector2 octavesCoverage, float scale)
		{
			this.octavesCoverage = octavesCoverage;
			this.scale = scale;
		}

		public static CloudProfile Lerp(in CloudProfile a, in CloudProfile b, float t) => new CloudProfile(
			Vector2.Lerp(a.octavesCoverage, b.octavesCoverage, t),
			Mathf.Lerp(a.scale, b.scale, t)
		);
		public static void SetProperties(CloudProfile clear, CloudProfile rain, Texture2D weatherMap)
		{
			for (int x = 0; x < weatherMap.width; x++)
			{
				for (int y = 0; y < weatherMap.height; y++)
				{
					float t = weatherMap.GetPixel(x, y).r;
					Vector2 cov = Vector2.Lerp(clear.octavesCoverage, rain.octavesCoverage, t);
					float scale = Mathf.Lerp(clear.scale, rain.scale, t);
					weatherMap.SetPixel(x, y, new Color(t, cov.x, cov.y, scale));
				}
			}
			weatherMap.Apply();
		}
	}

	const float degreesPerHour = 360 / 24;
	const float oneOver24 = 1f / 24f;

	public static TimeDayController singleton;

	public float maxSunnyScaler = 0.4f;
	public float maxCloudyScaler = 0.6f;
	public float maxWindyScaler = 0.8f;


	Transform sun => RenderSettings.sun.transform;
	[Header("Sun")]
	System.Text.StringBuilder entry;
	public float hour = 12;

	public float azimuth = 20;
	public Vector2 sunDisabledPeriod = new Vector2(22, 4);

	[Header("Time")]
	public FloatEventChannelSO changeTime;
	public float hoursPerSecond = 1;
	[Header("Wind")]

	public Vector2 windStrengthRange = new Vector2(0.2f, 3f);
	public float windChangingSpeed = 2;
	public float cloudWindSpeedMultiplier = 200f;
	Vector2 cloudPosition;
	public Vector3 Wind { get; private set; }

	public GlobalVector3SO windDirection;


	[Header("Weather")]

	public Gradient fogOverDay;
	public Vector2 fogDensityRange;
	[Range(0, 1)] public float rainIntensity;
	public VisualEffect rain;
	public float maxRainPerSecondPerArea = 30;
	public float weatherSimulationTileSize = 50;
	public float weatherSimulationTileOffset = -500;
	public int weatherSimulationResolution = 16;
	Texture2D weatherMap;

	[Header("Skybox Colours")]
	public ColorSet day;
	public ColorSet night;
	public ColorSet dayRain;
	public ColorSet nightRain;
	public Vector2 skyColorTransitionPeriod = new Vector2(0.3f, -0.3f);

	public CloudProfile dryClouds;
	public CloudProfile rainClouds;

	[Header("Thunder")]
	public float meanLightningInterval = 10;
	public float stdDevLightningInterval = 10;
	public float lightningLightTime = 0.5f;
	public Light thunderLight;
	float timeForNextLightning;

	public AudioEventChannelSO thunderChannel;
	public AudioClipSet thunderClips;

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
	public static float Square(float a) => a * a;
	[MyBox.ButtonMethod]
	public void MakeWeatherMap()
	{
		weatherMap = new Texture2D(weatherSimulationResolution, weatherSimulationResolution, TextureFormat.RGBAHalf, false, true);
		weatherMap.wrapMode = TextureWrapMode.Clamp;



		for (int x = 0; x < weatherMap.width; x++)
		{
			for (int y = 0; y < weatherMap.height; y++)
			{
				float val = 1 - Mathf.Sqrt(Square(y - weatherMap.height / 2f) + Square(x - weatherMap.width / 2f)) * 0.1f;
				val *= x == 0 ? 0f : 1f;
				val *= y == 0 ? 0f : 1f;
				val *= x == weatherMap.width - 1 ? 0f : 1f;
				val *= y == weatherMap.width - 1 ? 0f : 1f;

				weatherMap.SetPixel(x, y, new Color(val, 0, 0));
			}
		}

		CloudProfile.SetProperties(dryClouds, rainClouds, weatherMap);


		RenderSettings.skybox.SetTexture("_WeatherMap", weatherMap);
	}
	// Start is called before the first frame update
	void Start()
	{


		if (Application.isPlaying)
		{
			changeTime.OnEventRaised += ChangeTime;
			entry = DebugMenu.CreateEntry("Game");

			MakeWeatherMap();
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


	IEnumerator Lightning()
	{
		float u1 = 1.0f - Random.value; //uniform(0,1] random doubles
		float u2 = 1.0f - Random.value;
		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
					 Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
		float randNormal =
					 meanLightningInterval + stdDevLightningInterval * randStdNormal; //random normal(mean,stdDev^2)

		timeForNextLightning = Time.time + randNormal;

		thunderLight.enabled = true;
		yield return new WaitForSeconds(lightningLightTime);
		thunderLight.enabled = false;

		yield return new WaitForSeconds(1);


		thunderChannel.RaiseEvent(thunderClips, default);
	}

	// Update is called once per frame
	void Update()
	{
		if (Application.isPlaying)
		{
			if (DebugMenu.menuEnabled)
			{
				entry.Clear();
				entry.AppendFormat("Time: {0:00}:{1:00}", Mathf.Floor(hour), 60f * (hour - Mathf.Floor(hour)));
			}

			hour += Time.deltaTime * hoursPerSecond;


			cloudPosition += new Vector2(Wind.x, Wind.z) * Time.smoothDeltaTime * cloudWindSpeedMultiplier;


			RenderSettings.skybox.SetVector(shader_CloudsPosition, cloudPosition);

		}

		hour = Mathf.Repeat(hour, 24);

		float colorTransition = GetColorTransition();

		ColorSet colors;
		if (rainIntensity == 0)
		{
			colors = ColorSet.LerpAmbient(day, night, colorTransition);


		}
		else if (rainIntensity == 1)
		{
			colors = ColorSet.LerpAmbient(dayRain, nightRain, colorTransition);

		}
		else
		{
			colors = ColorSet.LerpAmbient(
			   ColorSet.LerpAmbient(day, dayRain, rainIntensity),
			   ColorSet.LerpAmbient(night, nightRain, rainIntensity),
			   colorTransition
		   );
		}

		if (rainIntensity != 0)
		{
			rain.gameObject.SetActive(true);
			rain.SetFloat("Drops Per Second Per Area", rainIntensity * maxRainPerSecondPerArea);

			if (Application.isPlaying)

				if (Time.time > timeForNextLightning)
				{
					StartCoroutine(Lightning());
				}
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
