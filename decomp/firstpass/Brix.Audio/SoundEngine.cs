using System.Collections.Generic;
using Brix.Lua;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Audio;

public class SoundEngine : MonoBehaviour
{
	private enum EngineState
	{
		Uninitialized,
		Ready,
		Missing
	}

	public struct PlayInfo
	{
		public string soundName;

		public string switchGroup;

		public string switchData;

		public string paramName;

		public float paramData;

		public Vector3 location;

		public GameObject emitter;
	}

	private static BrixLogger logger = new BrixLogger(typeof(SoundEngine).FullName);

	public static HashSet<GameObject> listeSonSFX = new HashSet<GameObject>();

	public static HashSet<GameObject> listeSonUI = new HashSet<GameObject>();

	public static HashSet<GameObject> listeSonMusic = new HashSet<GameObject>();

	public static HashSet<GameObject> listeSonAmbience = new HashSet<GameObject>();

	public static HashSet<GameObject> listeSonVoice = new HashSet<GameObject>();

	private static EngineState _engineState = EngineState.Uninitialized;

	private static bool _quitting = false;

	private static bool _windowsFocused = true;

	private const float MinVolume = 0f;

	private const float MaxVolume = 99f;

	private static GameObject m_emetteurParDefaut;

	private static bool m_mute;

	private static bool _gameIsReady;

	private static float m_volumeMaster = 100f;

	private static float m_volumeUI = 80f;

	private static float m_volumeSFX = 80f;

	private static float m_volumeMusic = 80f;

	private static float m_volumeVoice = 80f;

	private static float m_volumeAmbience = 80f;

	private static Transform _tempParent = null;

	private static List<GameObject> _temps = new List<GameObject>();

	public static bool EngineIsReady
	{
		get
		{
			if (_quitting)
			{
				return false;
			}
			try
			{
				if (_engineState == EngineState.Uninitialized && AkSoundEngine.IsInitialized())
				{
					_engineState = EngineState.Ready;
				}
			}
			catch
			{
				_engineState = EngineState.Missing;
			}
			return _engineState == EngineState.Ready;
		}
	}

	public static bool WindowsFocused => _windowsFocused;

	public static GameObject emetteurParDefaut
	{
		get
		{
			if (m_emetteurParDefaut == null)
			{
				if (Camera.main != null)
				{
					m_emetteurParDefaut = Camera.main.gameObject;
				}
				else
				{
					m_emetteurParDefaut = null;
				}
			}
			return m_emetteurParDefaut;
		}
		set
		{
			if (value != null)
			{
				m_emetteurParDefaut = value;
			}
			else if (Camera.main != null)
			{
				m_emetteurParDefaut = Camera.main.gameObject;
			}
			else
			{
				m_emetteurParDefaut = null;
			}
		}
	}

	[LuaDrivable("mute")]
	public static bool mute
	{
		get
		{
			return m_mute;
		}
		set
		{
			m_mute = value;
			ApplyMusicVolume();
			ApplyAmbienceVolume();
			ApplySFXVolume();
			ApplyUIVolume();
			ApplyVoiceVolume();
		}
	}

	public static bool GameIsReady
	{
		get
		{
			return _gameIsReady;
		}
		set
		{
			_gameIsReady = value;
			ApplyMusicVolume();
			ApplyAmbienceVolume();
			ApplySFXVolume();
			ApplyUIVolume();
			ApplyVoiceVolume();
		}
	}

	public static float volumeMaster
	{
		get
		{
			return m_volumeMaster;
		}
		set
		{
			m_volumeMaster = value;
			ApplyMusicVolume();
			ApplyAmbienceVolume();
			ApplySFXVolume();
			ApplyUIVolume();
			ApplyVoiceVolume();
		}
	}

	public static float volumeUI
	{
		get
		{
			return m_volumeUI;
		}
		set
		{
			m_volumeUI = value;
			ApplyUIVolume();
		}
	}

	public static float volumeSFX
	{
		get
		{
			return m_volumeSFX;
		}
		set
		{
			m_volumeSFX = value;
			ApplySFXVolume();
		}
	}

	public static float volumeMusic
	{
		get
		{
			return m_volumeMusic;
		}
		set
		{
			m_volumeMusic = value;
			ApplyMusicVolume();
		}
	}

	public static float volumeVoice
	{
		get
		{
			return m_volumeVoice;
		}
		set
		{
			m_volumeVoice = value;
			ApplyVoiceVolume();
		}
	}

	public static float volumeAmbience
	{
		get
		{
			return m_volumeAmbience;
		}
		set
		{
			m_volumeAmbience = value;
			ApplyAmbienceVolume();
		}
	}

	private void Start()
	{
		InitEngine();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnApplicationQuit()
	{
		_quitting = true;
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		_windowsFocused = focusStatus;
		if (AkSoundEngine.IsInitialized() && !Application.runInBackground)
		{
			if (focusStatus)
			{
				AkSoundEngine.WakeupFromSuspend();
			}
			else
			{
				AkSoundEngine.Suspend();
			}
			AkSoundEngine.RenderAudio();
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		listeSonSFX.Clear();
		listeSonUI.Clear();
		listeSonMusic.Clear();
		listeSonAmbience.Clear();
		listeSonVoice.Clear();
		listeSonSFX.Add(emetteurParDefaut);
		listeSonUI.Add(emetteurParDefaut);
		listeSonMusic.Add(emetteurParDefaut);
		listeSonAmbience.Add(emetteurParDefaut);
		listeSonVoice.Add(emetteurParDefaut);
		GameIsReady = false;
	}

	private void InitEngine()
	{
		if (base.transform.parent == null)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
		AkSoundEngine.LoadBank("CS_Prototype_1.bnk", -1, out var _);
	}

	private void Awake()
	{
		listeSonSFX.Clear();
		listeSonUI.Clear();
		listeSonMusic.Clear();
		listeSonAmbience.Clear();
		listeSonVoice.Clear();
		listeSonSFX.Add(emetteurParDefaut);
		listeSonUI.Add(emetteurParDefaut);
		listeSonMusic.Add(emetteurParDefaut);
		listeSonAmbience.Add(emetteurParDefaut);
		listeSonVoice.Add(emetteurParDefaut);
		GameIsReady = false;
	}

	private static float ApplySoundMultiplier(float volume, bool allowOutsideGame)
	{
		return (!mute && (allowOutsideGame || GameIsReady)) ? Mathf.Clamp(volume * volumeMaster / 100f, 0f, 99f) : 0f;
	}

	public static void ApplyUIVolume()
	{
		float value = ApplySoundMultiplier(m_volumeUI, allowOutsideGame: true);
		SetParam("UI_Volume", value);
	}

	public static void ApplySFXVolume()
	{
		float value = ApplySoundMultiplier(m_volumeSFX, allowOutsideGame: false);
		SetParam("SFX_Volume", value);
	}

	public static void ApplyMusicVolume()
	{
		float value = ApplySoundMultiplier(m_volumeMusic, allowOutsideGame: true);
		SetParam("Music_Volume", value);
	}

	public static void ApplyVoiceVolume()
	{
		float value = ApplySoundMultiplier(m_volumeVoice, allowOutsideGame: false);
		SetParam("Voices_Volume", value);
	}

	public static void ApplyAmbienceVolume()
	{
		float value = ApplySoundMultiplier(m_volumeAmbience, allowOutsideGame: false);
		SetParam("Environment_Volume", value);
	}

	private static GameObject GetTemporaryTracker()
	{
		GameObject gameObject = null;
		while (gameObject == null && _temps.Count > 0)
		{
			gameObject = _temps.Pop();
		}
		if (gameObject == null)
		{
			gameObject = new GameObject("SonTemp");
			if (_tempParent == null)
			{
				_tempParent = new GameObject("SoundEmitters").transform;
				Object.DontDestroyOnLoad(_tempParent);
			}
			gameObject.transform.SetParent(_tempParent);
		}
		return gameObject;
	}

	private static GameObject GetTemporaryTracker(Vector3 location)
	{
		GameObject temporaryTracker = GetTemporaryTracker();
		temporaryTracker.transform.position = location;
		return temporaryTracker;
	}

	private static void ReleaseTemporaryTracker(GameObject g)
	{
		_temps.Add(g);
	}

	private static GameObject GetTracker(GameObject tracked)
	{
		return tracked;
	}

	public static void Play(PlayInfo info)
	{
		if (!EngineIsReady)
		{
			logger.Warn("SoundEngine.Play(info) is ignored because engine is not ready.");
			return;
		}
		if (string.IsNullOrEmpty(info.soundName))
		{
			logger.Warn("SoundEngine.Play(info) is ignored because sound is null.");
			return;
		}
		bool flag = false;
		GameObject gameObject;
		if (info.emitter != null)
		{
			gameObject = GetTracker(info.emitter);
		}
		else if (info.location != Vector3.zero)
		{
			gameObject = GetTemporaryTracker(info.location);
			flag = true;
		}
		else
		{
			gameObject = GetTracker(emetteurParDefaut);
		}
		if (!string.IsNullOrEmpty(info.switchGroup))
		{
			_SetSwitch(info.switchGroup, info.switchData, gameObject);
		}
		if (!string.IsNullOrEmpty(info.paramName))
		{
			_SetParam(info.paramName, info.paramData, gameObject);
		}
		if (flag)
		{
			_PlayTemporary(info.soundName, gameObject);
		}
		else
		{
			_Play(info.soundName, gameObject);
		}
	}

	private static void _PlayTemporary(string soundName, GameObject tempEmit)
	{
		AkSoundEngine.PostEvent(soundName, tempEmit, 1u, delegate
		{
			ReleaseTemporaryTracker(tempEmit);
		}, null);
	}

	private static void _Play(string name, GameObject emitter)
	{
		AudioGameObjectTracker tracker = emitter.GetComponent<AudioGameObjectTracker>();
		if (tracker != null)
		{
			tracker.BeginSound();
			AkSoundEngine.PostEvent(name, emitter, 1u, delegate
			{
				if (tracker != null)
				{
					tracker.EndSound();
				}
			}, null);
		}
		else
		{
			AkSoundEngine.PostEvent(name, emitter);
		}
	}

	private static void _SetSwitch(string group, string data, GameObject emitter)
	{
		AkSoundEngine.SetSwitch(group, data, emitter);
	}

	private static void _SetParam(string name, float value)
	{
		AkSoundEngine.SetRTPCValue(name, value);
	}

	private static void _SetParam(string name, float value, GameObject emitter)
	{
		AkSoundEngine.SetRTPCValue(name, value, emitter);
	}

	public static void Play(string name)
	{
		Play(name, emetteurParDefaut);
	}

	public static void Play(string name, GameObject emitter)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		if (emitter == null)
		{
			emitter = emetteurParDefaut;
		}
		if (!EngineIsReady)
		{
			if (!_quitting)
			{
				logger.WarnFormat("SoundEngine.Play({0}, {1}) isn't ready. Please check SoundEngine.EngineIsReady before playing a sound outside the main game loop.", name, emitter);
			}
		}
		else if (emitter != null)
		{
			_Play(name, emitter);
		}
	}

	public static void SetSwitch(string group, string data)
	{
		SetSwitch(group, data, emetteurParDefaut);
	}

	public static void SetSwitch(string group, string data, GameObject emitter)
	{
		if (string.IsNullOrEmpty(group))
		{
			return;
		}
		if (emitter == null)
		{
			emitter = emetteurParDefaut;
		}
		if (!EngineIsReady)
		{
			if (!_quitting)
			{
				logger.WarnFormat("SoundEngine.SetSwitch({0}, {1}, {2}) isn't ready. Please check SoundEngine.EngineIsReady before playing a sound outside the main game loop.", group, data, emitter);
			}
		}
		else if (emitter != null)
		{
			_SetSwitch(group, data, GetTracker(emitter));
		}
	}

	public static void SetParam(string name, float value)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		if (!EngineIsReady)
		{
			if (!_quitting)
			{
				logger.WarnFormat("SoundEngine.SetParam({0}, {1}) isn't ready. Please check SoundEngine.EngineIsReady before playing a sound outside the main game loop.", name, value);
			}
		}
		else
		{
			_SetParam(name, value);
		}
	}

	public static void SetParam(string name, float value, GameObject emitter)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		if (emitter == null)
		{
			emitter = emetteurParDefaut;
		}
		if (!EngineIsReady)
		{
			if (!_quitting)
			{
				logger.WarnFormat("SoundEngine.SetParam({0}, {1}, {2}) isn't ready. Please check SoundEngine.EngineIsReady before playing a sound outside the main game loop.", name, value, emitter);
			}
		}
		else if (emitter != null)
		{
			_SetParam(name, value, GetTracker(emitter));
		}
	}

	public static void AddEmitter(string type, GameObject emitter)
	{
		if (emitter == null)
		{
			logger.Warn("SoundEngine.AddEmitter: NULL emitter sent");
			return;
		}
		switch (type)
		{
		case "SFX":
			if (!listeSonSFX.Contains(emitter))
			{
				listeSonSFX.Add(emitter);
				SetParam("SFX_Volume", m_volumeSFX, emitter);
			}
			else
			{
				logger.Warn("SoundEngine.AddEmitter: SFX Emitter " + emitter.name + " already in the list.");
			}
			break;
		case "UI":
			if (!listeSonUI.Contains(emitter))
			{
				listeSonUI.Add(emitter);
				SetParam("UI_Volume", m_volumeUI, emitter);
			}
			else
			{
				logger.Warn("SoundEngine.AddEmitter: UI Emitter " + emitter.name + " already in the list.");
			}
			break;
		case "Music":
			if (!listeSonUI.Contains(emitter))
			{
				listeSonUI.Add(emitter);
				SetParam("Music_Volume", m_volumeMusic, emitter);
			}
			else
			{
				logger.Warn("SoundEngine.AddEmitter: Music Emitter " + emitter.name + " already in the list.");
			}
			break;
		case "Voice":
			if (!listeSonUI.Contains(emitter))
			{
				listeSonUI.Add(emitter);
				SetParam("Voices_Volume", m_volumeMusic, emitter);
			}
			else
			{
				logger.Warn("SoundEngine.AddEmitter: Voice Emitter " + emitter.name + " already in the list.");
			}
			break;
		default:
			logger.Warn("SoundEngine.AddEmitter: unknown type");
			break;
		}
	}

	public static bool IsPlaying(string eventName)
	{
		if (emetteurParDefaut != null)
		{
			return IsPlaying(eventName, emetteurParDefaut);
		}
		return false;
	}

	public static bool IsPlaying(string eventName, GameObject emitter)
	{
		bool result = false;
		if (!EngineIsReady || emitter != null)
		{
		}
		return result;
	}

	public static void Terminer()
	{
		listeSonSFX.Clear();
		listeSonUI.Clear();
		listeSonMusic.Clear();
		listeSonAmbience.Clear();
		listeSonVoice.Clear();
		AkSoundEngine.StopAll();
		if (Camera.main != null)
		{
			listeSonSFX.Add(Camera.main.gameObject);
			listeSonUI.Add(Camera.main.gameObject);
			listeSonMusic.Add(Camera.main.gameObject);
			listeSonAmbience.Add(Camera.main.gameObject);
			listeSonVoice.Add(Camera.main.gameObject);
		}
	}

	[LuaDrivable("Play")]
	private static DynValue lua_Play(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "Play");
		if (!args.AsBool(1, "Play") || GameIsReady)
		{
			Play(text);
		}
		return DynValue.Void;
	}

	[LuaDrivable("IsPlaying")]
	private static DynValue IsPlaying(ScriptExecutionContext context, CallbackArguments args)
	{
		string eventName = args.AsString(0, "IsPlaying");
		return DynValue.NewBoolean(IsPlaying(eventName));
	}

	[LuaDrivable("SetParam")]
	private static DynValue lua_SetParam(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "SetParam");
		float value = args.AsFloat(1, "SetParam");
		SetParam(text, value);
		return DynValue.Void;
	}
}
