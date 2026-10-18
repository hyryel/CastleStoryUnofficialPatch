using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine.Abstract;
using Brix.Engine.FlatWorld;
using Brix.Lifecycle.Pooling;
using Brix.NewUI.VersionDisplay;
using Brix.Utils;
using Brix.Utils.Logging;
using Network;
using UnityEngine;

namespace Brix.Engine;

public class Neo : Brix.Engine.Abstract.Neo
{
	public enum PathNodeDebug
	{
		None,
		PathScanned,
		PathChosen,
		KnowledgeRegion,
		PathMarker,
		BlueprintScanned
	}

	private struct RelaisData
	{
		public Action action;

		public float delay;

		public string name;
	}

	private struct RelaisEndOfFrameData
	{
		public Action action;

		public string name;
	}

	private static BrixLogger logger = new BrixLogger(typeof(Neo).FullName);

	private static Neo instance;

	private static GameObject jeu;

	public static bool menusOff;

	public static Dictionary<string, Transform> enfants = new Dictionary<string, Transform>();

	public static bool modeEditeur;

	public static bool modeDebug = false;

	public static bool disableSpecialAssets = false;

	public static bool showUnsupportedGameModes = false;

	public static bool hideVersionWithMenus = false;

	public static bool versionOff = false;

	public static bool skipWelcomeDisclaimers = false;

	public static bool showHelpAtGameStart = false;

	public static int overrideTerrainAlgos = 0;

	public static int overrideTerrainLodMax = -1;

	public static int overrideTerrainLateAlgos = 0;

	public static int terrainAcceleration = 1;

	public static int terrainLodQuality = 1;

	public static float canvasScale = 1f;

	public static int alternateLodQuality = 3;

	public static bool lodSetToExtreme = false;

	public string modeDeJeu;

	public static bool multiplayer;

	public static string multiplayerIP;

	public static string multiplayerPort;

	public static string multiplayerName;

	public static MatchList.MatchData matchData;

	private static int _terrainThreading = 1;

	public static PathNodeDebug PathNodeDebugMode;

	private static VersionTracker _version;

	private static bool LoadTimeOnceInSession = false;

	private RelaisData[] _relais = new RelaisData[64];

	private int _relaisCount;

	private RelaisEndOfFrameData[] _relaisEndOfFrame = new RelaisEndOfFrameData[64];

	private int _relaisEndOfFrameCount;

	private static readonly WaitForEndOfFrame WaifForEndOfFrame = new WaitForEndOfFrame();

	public static GameObject Jeu => jeu;

	public static bool Exists => instance != null;

	public static bool IsMultiplayer => multiplayer;

	public static int TerrainThreading
	{
		get
		{
			return _terrainThreading;
		}
		set
		{
			if (_terrainThreading != value)
			{
				_terrainThreading = value;
				FlatWorldInitializer.Refresh();
			}
		}
	}

	public static string ModeDeJeu => instance.modeDeJeu;

	private static VersionTracker version
	{
		get
		{
			if (_version == null)
			{
				_version = UnityEngine.Object.FindObjectOfType<VersionTracker>();
			}
			return _version;
		}
	}

	public static string ProtocolVersion => version.ProtocolVersion;

	public static string DisplayVersion => version.DisplayVersion;

	public static string SemanticVersion => version.SemanticVersion;

	private void Awake()
	{
		logger.Info("Neo awoken");
		instance = this;
		jeu = GameObject.Find("/Jeu");
		enfants.Clear();
		InitEnfants(GameObject.Find("/Jeu").transform, string.Empty);
		logger.Info("Neo.Awake() done");
	}

	public static void TogglePathDebug()
	{
		if (PathNodeDebugMode == PathNodeDebug.BlueprintScanned)
		{
			PathNodeDebugMode = PathNodeDebug.None;
		}
		else
		{
			PathNodeDebugMode++;
		}
		logger.InfoFormat("Path debug : {0}", PathNodeDebugMode);
	}

	public static int GetActualDesiredFPS(int targetFPS)
	{
		if (Jeu == null && targetFPS <= 0)
		{
			return 60;
		}
		return targetFPS;
	}

	public static void LoadTimeForGameAnalytics(long lastFileTimeUTC)
	{
		if (!LoadTimeOnceInSession)
		{
			DateTime dateTime = DateTime.FromFileTimeUtc(lastFileTimeUTC);
			string arg = "Release";
			GA.API.Design.NewEvent($"Build:Type:{arg}");
			if (lastFileTimeUTC == 0)
			{
				GA.API.Design.NewEvent($"BuildUnique:Type:{arg}");
			}
			if (dateTime.Day != DateTime.UtcNow.Day)
			{
				GA.API.Design.NewEvent($"BuildDaily:Type:{arg}");
			}
			if (dateTime.Month != DateTime.UtcNow.Month)
			{
				GA.API.Design.NewEvent($"BuildMonthly:Type:{arg}");
			}
			LoadTimeOnceInSession = true;
		}
	}

	public static void PrendCetteMerde(Component c)
	{
		PrendCetteMerde(c.gameObject);
	}

	public static void PrendCetteMerde(GameObject g)
	{
		ClasseMoi("Debris", g);
	}

	public static void Vidanges()
	{
		if (!multiplayer)
		{
			ClearMoi("Debris");
		}
	}

	public static void InitEnfants(Transform debut, string prefixe)
	{
		foreach (Transform item in debut)
		{
			string text = $"{prefixe}{item.name}";
			enfants[text] = item;
			text = string.Format("{0}{1}", text, "/");
			InitEnfants(item, text);
		}
	}

	public static void ClasseMoi(string nom, Component c)
	{
		ClasseMoi(nom, c.transform);
	}

	public static void ClasseMoi(string nom, GameObject g)
	{
		ClasseMoi(nom, g.transform);
	}

	public static void ClasseMoi(string nom, Transform t)
	{
		string key = SanitizeClasseMoiPath(nom);
		if (!enfants.TryGetValue(key, out var value) || value == null)
		{
			value = GameObjectResolver.FindOrCreateGameObject($"Jeu/{nom}");
			enfants[key] = value;
		}
		t.SetParent(value);
	}

	private static string SanitizeClasseMoiPath(string path)
	{
		string text = ((path.StartsWith("/Jeu/") && path.Length > 5) ? path.Substring(5) : ((path.StartsWith("Jeu/") && path.Length > 4) ? path.Substring(4) : ((path.StartsWith("/") && path.Length > 1) ? path.Substring(1) : (string.IsNullOrEmpty(path) ? "UnclassedObjects" : path))));
		if (text.EndsWith("/"))
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}

	public static void EffaceMoi(string nom)
	{
		if (enfants.TryGetValue(nom, out var value))
		{
			enfants.Remove(nom);
		}
		else
		{
			GameObject gameObject = GameObjectResolver.FindGameObjectInScene($"/Jeu/{nom}");
			if (gameObject != null)
			{
				value = gameObject.transform;
			}
		}
		if (value != null)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
	}

	public static void ClearMoi(string nom)
	{
		if (!enfants.TryGetValue(nom, out var value))
		{
			GameObject gameObject = GameObjectResolver.FindGameObjectInScene($"/Jeu/{nom}");
			if (gameObject != null)
			{
				value = gameObject.transform;
			}
		}
		if (value != null)
		{
			for (int num = value.childCount - 1; num >= 0; num--)
			{
				ObjectPoolSingleton.Release(value.GetChild(num).gameObject);
			}
		}
	}

	public virtual void OnDestroy()
	{
		StopAllCoroutines();
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		int num = _relaisCount;
		for (int i = 0; i < num; i++)
		{
			if (_relais[i].delay <= deltaTime)
			{
				try
				{
					_relais[i].action();
				}
				catch (Exception e)
				{
					logger.ErrorFormat(e, "Exception in Relais : {0}", _relais[i].name);
				}
				num--;
				ref RelaisData reference = ref _relais[i];
				reference = _relais[num];
				_relais[num] = default(RelaisData);
				_relaisCount--;
				if (num < _relaisCount)
				{
					ref RelaisData reference2 = ref _relais[num];
					reference2 = _relais[_relaisCount];
					_relais[_relaisCount] = default(RelaisData);
				}
				i--;
			}
			else
			{
				_relais[i].delay -= deltaTime;
			}
		}
	}

	private void Start()
	{
		StartCoroutine(UpdateEndOfFrame());
	}

	private IEnumerator UpdateEndOfFrame()
	{
		while (true)
		{
			yield return WaifForEndOfFrame;
			int n = _relaisEndOfFrameCount;
			for (int i = 0; i < n; i++)
			{
				_relaisEndOfFrame[i].action();
				_relaisEndOfFrame[i] = default(RelaisEndOfFrameData);
			}
			if (n < _relaisEndOfFrameCount)
			{
				Array.Copy(_relaisEndOfFrame, n, _relaisEndOfFrame, 0, _relaisEndOfFrameCount - n);
				Array.Clear(_relaisEndOfFrame, _relaisEndOfFrameCount - n, n);
			}
			_relaisEndOfFrameCount -= n;
		}
	}

	public new static Coroutine StartCoroutine(IEnumerator f)
	{
		return (!(instance != null)) ? null : ((MonoBehaviour)instance).StartCoroutine(f);
	}

	public static void Relais(Action action, float delay, string name)
	{
		if (instance._relaisCount >= instance._relais.Length)
		{
			Array.Resize(ref instance._relais, instance._relaisCount * 2);
		}
		ref RelaisData reference = ref instance._relais[instance._relaisCount++];
		reference = new RelaisData
		{
			action = action,
			delay = delay,
			name = name
		};
	}

	public static void Relais(Action action, string name)
	{
		if (instance == null)
		{
			logger.ErrorFormat("Neo.Relais was called outside of ScenePrincipale (name={0}). Ignoring.", name);
			return;
		}
		if (instance._relaisEndOfFrameCount >= instance._relaisEndOfFrame.Length)
		{
			Array.Resize(ref instance._relaisEndOfFrame, instance._relaisEndOfFrameCount * 2);
		}
		ref RelaisEndOfFrameData reference = ref instance._relaisEndOfFrame[instance._relaisEndOfFrameCount++];
		reference = new RelaisEndOfFrameData
		{
			action = action,
			name = name
		};
	}

	private static void RelaisWrapper(IDisposable toDispose, string actionName, Action action)
	{
		action();
		toDispose.Dispose();
	}

	public static void StepOneSecond()
	{
		StartCoroutine(StepSec());
	}

	public static void StepOneTenth()
	{
		StartCoroutine(StepTenth());
	}

	public static IEnumerator StepSec()
	{
		ClockConfig.Paused = false;
		float waitUntil = Time.realtimeSinceStartup + 1f;
		while (Time.realtimeSinceStartup < waitUntil)
		{
			yield return null;
		}
		ClockConfig.Paused = true;
	}

	public static IEnumerator StepTenth()
	{
		ClockConfig.Paused = false;
		float waitUntil = Time.realtimeSinceStartup + 0.1f;
		while (Time.realtimeSinceStartup < waitUntil)
		{
			yield return null;
		}
		ClockConfig.Paused = true;
	}
}
