using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Lua;

[JsonObject(MemberSerialization.OptIn)]
public class LuaMarker : GameComponent
{
	[JsonProperty]
	[LuaDrivable("markerTag")]
	public string markerTag = "nobody";

	[JsonProperty]
	[LuaDrivable("markerGroups")]
	public List<string> markerGroups = new List<string>();

	[JsonProperty]
	public string DisplayName;

	[JsonProperty]
	[LuaDrivable("placed")]
	public bool Placed;

	private static Signal _markerChangedStatic = new Signal();

	private static List<LuaMarker> _allMarkers = new List<LuaMarker>();

	private Signal _markerChanged = new Signal();

	private BeaconDriver _beaconDriver;

	public static IReadOnlySignal MarkerChangedStatic => _markerChangedStatic;

	private static List<LuaMarker> AllMarkers2 => (from i in UIGameObserver.worldEditorObjects.All.Slinq()
		select i.GetComponent<LuaMarker>()).ToList();

	public IReadOnlySignal MarkerChanged => _markerChanged;

	public Vector3 position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Vector3 localPosition
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return base.transform.rotation;
		}
		set
		{
			base.transform.rotation = value;
		}
	}

	public Quaternion localRotation
	{
		get
		{
			return base.transform.localRotation;
		}
		set
		{
			base.transform.localRotation = value;
		}
	}

	[MoonSharpVisible(false)]
	public static LuaMarker GetTagged(string tagged)
	{
		return _allMarkers.Slinq().FirstOrDefault((LuaMarker x) => x.markerTag == tagged);
	}

	[MoonSharpVisible(false)]
	public static Dictionary<string, object> GetTagged(string tagged, List<Dictionary<string, object>> serialised)
	{
		return serialised.Slinq().FirstOrDefault((Dictionary<string, object> x) => (string)x["markerTag"] == tagged);
	}

	[MoonSharpVisible(false)]
	public static List<LuaMarker> GetGroup(string group)
	{
		return (from x in _allMarkers.Slinq()
			where x.markerGroups.Contains(@group)
			select x).ToList();
	}

	public static List<LuaMarker> GetGroup2(string group)
	{
		return (from x in AllMarkers2.Slinq()
			where x.markerGroups.Contains(@group)
			select x).ToList();
	}

	[MoonSharpVisible(false)]
	public static int GetGroupCount(string group)
	{
		return (from x in _allMarkers.Slinq()
			where x.markerGroups.Contains(@group)
			select x).Count();
	}

	[MoonSharpVisible(false)]
	public static List<Dictionary<string, object>> GetGroup(string group, List<Dictionary<string, object>> serialised)
	{
		if (serialised != null)
		{
			return (from x in serialised.Slinq()
				where ((List<string>)x["markerGroups"]).Contains(@group)
				select x).ToList();
		}
		return new List<Dictionary<string, object>>();
	}

	[MoonSharpVisible(false)]
	public static List<Dictionary<string, object>> GetGroup2(string group, List<Dictionary<string, object>> serialised)
	{
		if (serialised != null)
		{
			return (from x in serialised.Slinq()
				where ((List<string>)x["markerGroups"]).Contains(@group)
				select x).ToList();
		}
		return new List<Dictionary<string, object>>();
	}

	[MoonSharpVisible(false)]
	public static void Mark(LuaDriver ld, string tag, Table groups)
	{
		if (ld != null)
		{
			LuaMarker luaMarker = ld.gameObject.AddComponent<LuaMarker>();
			luaMarker.markerTag = tag;
			luaMarker.markerGroups = new List<string>(groups.Length);
			foreach (DynValue value in groups.Values)
			{
				luaMarker.markerGroups.Add(value.String);
			}
			luaMarker._markerChanged.Invoke();
			_markerChangedStatic.Invoke();
			return;
		}
		throw new ScriptRuntimeException("Markers.Mark can only mark drivers, arg #1 is not.");
	}

	[MoonSharpVisible(false)]
	public static List<Dictionary<string, object>> GetSerialisedMarkers()
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>(_allMarkers.Count);
		foreach (LuaMarker allMarker in _allMarkers)
		{
			if (allMarker.gameObject.activeSelf && !allMarker.IsInPicker)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("markerTag", allMarker.markerTag);
				dictionary.Add("markerGroups", allMarker.markerGroups);
				dictionary.Add("position", allMarker.position);
				dictionary.Add("localPosition", allMarker.localPosition);
				dictionary.Add("rotation", allMarker.rotation);
				dictionary.Add("localRotation", allMarker.localRotation);
				list.Add(dictionary);
			}
		}
		return list;
	}

	private static List<Dictionary<string, object>> GetLuaMarkerValidationList(Script script)
	{
		Table table = (Table)script.Globals["__LUAMARKER_VALIDATION_LIST__"];
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (DynValue value in table.Values)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("markerTag", value.Table.Get("markerTag").CastToString());
			dictionary.Add("markerGroups", value.Table.Get("markerGroups").ToObject<List<string>>());
			dictionary.Add("position", value.Table.Get("position").ToObject<Vector3>());
			dictionary.Add("localPosition", value.Table.Get("localPosition").ToObject<Vector3>());
			dictionary.Add("rotation", value.Table.Get("rotation").ToObject<Quaternion>());
			dictionary.Add("localRotation", value.Table.Get("localRotation").ToObject<Quaternion>());
			list.Add(dictionary);
		}
		return list;
	}

	[MoonSharpVisible(false)]
	public static void RegisterClass(Script script)
	{
		LuaLibs.RegisterUserData<LuaMarker>();
		LuaLibs.RegisterClrToScript(delegate(Script sc, LuaMarker lm)
		{
			LuaDriver component = lm.GetComponent<LuaDriver>();
			return (component == null) ? UserData.Create(lm) : UserData.Create(component);
		});
		Table table = new Table(script);
		table["GetTagged"] = new Func<string, LuaMarker>(GetTagged);
		table["GetGroup"] = new Func<string, List<LuaMarker>>(GetGroup);
		table["GetGroup2"] = new Func<string, List<LuaMarker>>(GetGroup2);
		table["Mark"] = new Action<LuaDriver, string, Table>(Mark);
		script.Globals["Markers"] = table;
	}

	public static void RegisterClassValidation(Script script)
	{
		Table table = new Table(script);
		table["GetTagged"] = (Func<string, Dictionary<string, object>>)((string s) => GetTagged(s, GetLuaMarkerValidationList(script)));
		table["GetGroup"] = (Func<string, List<Dictionary<string, object>>>)((string s) => GetGroup(s, GetLuaMarkerValidationList(script)));
		table["GetGroup2"] = (Func<string, List<Dictionary<string, object>>>)((string s) => GetGroup2(s, GetLuaMarkerValidationList(script)));
		script.Globals["Markers"] = table;
	}

	public void Translate(Vector3 translation)
	{
		base.transform.Translate(translation);
	}

	private void RecenterOnBeacon()
	{
		if (!(_beaconDriver == null) && !(_beaconDriver.Beacon == null))
		{
			Transform transform = _beaconDriver.Beacon.transform;
			transform.SetParent(null, worldPositionStays: true);
			base.transform.position = transform.position;
			transform.SetParent(base.transform, worldPositionStays: true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_allMarkers.Add(this);
	}

	protected override void Start()
	{
		base.Start();
		_beaconDriver = GetComponent<BeaconDriver>();
		if (_beaconDriver != null && _beaconDriver.Beacon != null)
		{
			_beaconDriver.Beacon.PositionChange.End.Connect(delegate
			{
				RecenterOnBeacon();
			}).DisconnectWhen(base.OnDestroySignal);
		}
	}

	protected override void OnDestroy()
	{
		_allMarkers.Remove(this);
		_markerChangedStatic.Invoke();
		base.OnDestroy();
	}
}
