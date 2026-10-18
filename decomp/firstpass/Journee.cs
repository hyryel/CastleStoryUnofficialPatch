using System.Runtime.Serialization;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Network;
using Brix.Input;
using Brix.Lua;
using Brix.Network.Util;
using Brix.Skybox;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

[SerializerTarget(new string[] { "save" })]
[JsonObject(MemberSerialization.OptIn)]
public class Journee : MonoBehaviour
{
	public Orbit SunOrbit;

	public Signal NightFall = new Signal();

	public Signal Sunrise = new Signal();

	[LuaDrivable("hk_OnNightfall")]
	private LuaHook lua_nightfall;

	[LuaDrivable("hk_OnSunrise")]
	private LuaHook lua_sunrise;

	[Range(0f, 24f)]
	[JsonProperty]
	public float Heure;

	private bool _isDayTime;

	public float Longueur;

	[JsonProperty]
	public bool Pause;

	private static bool _pausedByWorldEditor = true;

	public bool MainJournee;

	public float LoadedTimeOfTheDay;

	public AnimationCurve Curve;

	public static Journee instance;

	public static GameSignals.GameSignal<float, float> AfterTimeChangeSignal = new GameSignals.GameSignal<float, float>();

	public static GameSignals.GameSignal OnSetIsPaused = new GameSignals.GameSignal();

	[JsonProperty]
	[LuaDrivable("dayLengthFactor")]
	private float dayLengthFactor = 1f;

	[JsonProperty]
	[LuaDrivable("nightLengthFactor")]
	private float nightLengthFactor = 1f;

	[LuaDrivable("sunSpeed")]
	public float sunSpeed = 1f;

	private const float totalSpanTime = 24f;

	private const float duskTime = 19f;

	private const float dawnTime = 5f;

	private float lastCheck;

	[JsonProperty]
	public Vector2 SunElevationLimits
	{
		get
		{
			return (!(SunOrbit != null)) ? Vector2.zero : SunOrbit.ElevationLimits;
		}
		set
		{
			if (SunOrbit != null)
			{
				SunOrbit.ElevationLimits = value;
			}
		}
	}

	public bool IsDayTime
	{
		get
		{
			return _isDayTime;
		}
		set
		{
			if (_isDayTime && !value)
			{
				NightFall.Invoke();
				if (MainJournee && lua_nightfall != null)
				{
					LuaHookBase.InvokeFromCLR(lua_nightfall);
				}
			}
			if (!_isDayTime && value)
			{
				Sunrise.Invoke();
				if (MainJournee && lua_sunrise != null)
				{
					LuaHookBase.InvokeFromCLR(lua_sunrise);
				}
			}
			_isDayTime = value;
		}
	}

	public static bool PausedByArchitecte => ClockConfig.Paused;

	public static bool PausedByWorldEditor
	{
		get
		{
			return InputModeController.DefaultMode == InputMode.worldEditor && _pausedByWorldEditor;
		}
		set
		{
			bool pausedByWorldEditor = _pausedByWorldEditor;
			bool flag = value;
			if (flag != pausedByWorldEditor)
			{
				_pausedByWorldEditor = flag;
				GameSignals.Invoke(OnSetIsPaused);
			}
		}
	}

	public bool IsPaused
	{
		get
		{
			return Pause || PausedByWorldEditor || PausedByArchitecte;
		}
		set
		{
			bool pause = Pause;
			bool flag = value;
			if (flag != pause)
			{
				Pause = flag;
				GameSignals.Invoke(OnSetIsPaused);
			}
		}
	}

	public static float CurrentTime
	{
		get
		{
			return (!(instance == null)) ? instance.Heure : 0f;
		}
		set
		{
			if (instance != null)
			{
				instance.SetTime(value);
			}
		}
	}

	public static bool CurrentPaused
	{
		get
		{
			return instance != null && instance.IsPaused;
		}
		set
		{
			if (instance != null)
			{
				instance.IsPaused = value;
			}
		}
	}

	private float nightSpanTime => 10f;

	private float daySpanTime => 24f - nightSpanTime;

	[LuaDrivable("dayTime")]
	private static float lua_time
	{
		get
		{
			return instance.Heure;
		}
		set
		{
			if (value < 0f)
			{
				instance.SetTime(0f);
			}
			else if (value > 24f)
			{
				instance.SetTime(24f);
			}
			else
			{
				instance.SetTime(value);
			}
		}
	}

	[LuaDrivable("dayPause")]
	private static bool lua_pause
	{
		get
		{
			return instance.IsPaused;
		}
		set
		{
			instance.IsPaused = value;
		}
	}

	[LuaDrivable("ambientColor")]
	private Color lua_ambientColor
	{
		get
		{
			return GameObject.Find("Jeu/Environnement/Skybox3d/AmbientLighting").GetComponent<MeshRenderer>().sharedMaterial.color;
		}
		set
		{
			UNetJourneeSync.instance.SetNetworkAtmosphereColor(value);
		}
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext context)
	{
		GameSignals.Invoke(OnSetIsPaused);
	}

	public void SetTime(float dstTime)
	{
		float heure = Heure;
		Heure = dstTime;
		GameSignals.Invoke(AfterTimeChangeSignal, Heure, heure);
		Update();
	}

	private void Awake()
	{
		IsDayTime = !(Heure >= 19f) && !(Heure <= 5f);
	}

	private void Start()
	{
		if (MainJournee || instance == null)
		{
			instance = this;
			lua_nightfall = new LuaHook(NightFall);
			lua_sunrise = new LuaHook(Sunrise);
		}
		GameSignals.Once(GameSignals.LevelLoaded, delegate(Journee self)
		{
			self.Initialize();
		}, this);
	}

	private void Initialize()
	{
		if (NetworkServer.active)
		{
			LoadedTimeOfTheDay = Heure;
			UNetJourneeSync.instance.ForceUpdateHeure();
		}
	}

	private void Update()
	{
		float heure = Heure;
		if (!IsPaused && !ClockConfig.Paused)
		{
			float num = Curve.Evaluate(Heure / 24f) * Longueur;
			float num2 = 24f * Time.deltaTime / num * (sunSpeed * sunSpeed);
			nightLengthFactor = ((nightLengthFactor != 0f) ? nightLengthFactor : 0.0001f);
			dayLengthFactor = ((dayLengthFactor != 0f) ? dayLengthFactor : 0.0001f);
			num2 = ((!(Heure >= 19f) && !(Heure <= 5f)) ? (num2 / dayLengthFactor) : (num2 / nightLengthFactor));
			num2 = ((num2 != 0f) ? num2 : 0.0001f);
			Heure += num2;
		}
		IsDayTime = !(Heure >= 19f) && !(Heure <= 5f);
		Heure = ((Heure != 24f) ? Mathf.Repeat(Heure, 24f) : Heure);
		Longueur = Mathf.Clamp(Longueur, 1f, 10000f);
		GameSignals.Invoke(AfterTimeChangeSignal, Heure, heure);
	}

	public void EnvoyerAuSoundEngine()
	{
		if (Time.time >= lastCheck + 1f)
		{
			lastCheck = Time.time;
		}
	}

	[LuaDrivable("ForceUpdateTime")]
	public static DynValue lua_ForceUpdateTime(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		UNetJourneeSync.instance.ForceUpdateHeure();
		UNetJourneeSync.instance.AtmosphereColorSet(value: true);
		return DynValue.Void;
	}
}
