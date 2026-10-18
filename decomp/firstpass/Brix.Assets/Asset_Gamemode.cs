using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Brix.External.Factories;
using Brix.Game;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.UI.Icons;
using Brix.Utils;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Assets;

public class Asset_Gamemode : Asset_Base<Asset_Gamemode>
{
	public struct FactionPrototype
	{
		public int uid;

		public string name;

		public Sprite flagIcon;

		public Sprite crystalIcon;

		public uint colour;

		public CrystalVisuals.CrystalColorEnum crystalColor;

		public bool isPlayable;

		public bool isDefault;

		public bool isAI;

		public bool isCorruptrons;

		public bool hasGlobalFogVision;

		public bool isSpectator;

		public bool usesFireflies;

		public FactionPrototype(Table table)
		{
			uid = TryGetValue(table, "uid", -1);
			name = TryGetValue(table, "name", "UNSET");
			flagIcon = TryGetValue<Sprite>(table, "flagIcon", null);
			crystalIcon = TryGetValue<Sprite>(table, "crystalIcon", null);
			colour = TryGetValue(table, "colour", uint.MaxValue);
			crystalColor = TryGetEnumValue(table, "crystalColor", CrystalVisuals.CrystalColorEnum.Neutral);
			isPlayable = TryGetValue(table, "isPlayable", defaultValue: false);
			isDefault = TryGetValue(table, "isDefault", defaultValue: false);
			isAI = TryGetValue(table, "isAI", defaultValue: false);
			isCorruptrons = TryGetValue(table, "isCorruptrons", defaultValue: false);
			hasGlobalFogVision = TryGetValue(table, "hasGlobalFogVision", defaultValue: false);
			isSpectator = TryGetValue(table, "isSpectator", defaultValue: false);
			usesFireflies = TryGetValue(table, "usesFireflies", defaultValue: false);
		}

		public Faction CreateFaction()
		{
			ObjectPoolSingleton.RequestComponent<Faction>(out var t, Brix.External.Factories.Core.Faction);
			t.transform.SetParent(GameObject.Find("/Jeu/Factions").transform);
			t.NetworkID = uid;
			return t;
		}

		public void PopulateFaction(Faction fac)
		{
			fac.Networkname = name;
			fac.NetworkFactionColor = UintToColour(colour);
			fac.CrystalColor = crystalColor;
			fac.FactionIcon = flagIcon;
			fac.CrystalIcon = crystalIcon;
			fac.NetworkisPlayable = isPlayable;
			fac.NetworkisAI = isAI;
			fac.NetworkHasGlobalFogVision = hasGlobalFogVision;
			fac.NetworkIsSpectator = isSpectator;
			fac.usesFireflies = usesFireflies;
			if (isSpectator)
			{
				Faction.Spectator = fac;
			}
			if (isDefault)
			{
				Faction.DefaultNeutral = fac;
			}
			if (isCorruptrons)
			{
				fac.NetworkUseCreepShader = true;
				Faction.DefaultEnemy = fac;
			}
		}

		private static Color UintToColour(uint i)
		{
			uint num = (i >> 24) & 0xFF;
			uint num2 = (i >> 16) & 0xFF;
			uint num3 = (i >> 8) & 0xFF;
			uint num4 = i & 0xFF;
			return new Color((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f, (float)num4 / 255f);
		}

		private static T TryGetValue<T>(Table table, string key, T defaultValue)
		{
			DynValue dynValue = table.Get(key);
			if (dynValue.IsNotNil())
			{
				return dynValue.ToObject<T>();
			}
			return defaultValue;
		}

		private static T TryGetEnumValue<T>(Table table, string key, T defaultValue) where T : struct
		{
			DynValue dv = table.Get(key);
			if (dv.IsNotNil())
			{
				return dv.ToEnum<T>();
			}
			return defaultValue;
		}
	}

	private static readonly string __F_META;

	private static readonly string __F_CLIENTSIDE;

	private static readonly string __F_SERVERSIDE;

	private static readonly string __F_VALIDATOR;

	private static readonly string __F_FACTIONS;

	private static readonly string __F_IMG_VICTORY;

	private static readonly string __F_IMG_DEFEAT;

	private static readonly string __F_IMG_INTRO;

	public bool isSupported = true;

	public int order = -1;

	public Texture2D texVictory;

	public Texture2D texDefeat;

	public Texture2D texIntro;

	static Asset_Gamemode()
	{
		__F_META = "meta.json";
		__F_CLIENTSIDE = "clientside.lua";
		__F_SERVERSIDE = "serverside.lua";
		__F_VALIDATOR = "map_validator.lua";
		__F_FACTIONS = "factions.lua";
		__F_IMG_VICTORY = "victory.png";
		__F_IMG_DEFEAT = "defeat.png";
		__F_IMG_INTRO = "intro.png";
		Asset_Base<Asset_Gamemode>.__CURRENTVERSIONS.Add(typeof(Asset_Gamemode), 1);
	}

	public Asset_Gamemode(string path)
		: base(path)
	{
		Disk_ReadMeta();
	}

	public string PrettyDisplayName(bool supported)
	{
		return base.displayName + ((!supported) ? (" (" + I2Helper.TryGet("##mode_unsupported") + ")") : string.Empty);
	}

	public List<Asset_GamemodePreset> GetPresets(AssetQueryParameters qparams = AssetQueryParameters.AllSPSources, AssetPlayableContext context = AssetPlayableContext.Any)
	{
		return (from x in AssetDispensary.presets.SendQuery(base.codename, qparams, context).Slinq()
			orderby x.order
			select x).ToList();
	}

	public Asset_GamemodePreset GetDefaultPreset(AssetQueryParameters qparams = AssetQueryParameters.AllSPSources, AssetPlayableContext context = AssetPlayableContext.Any)
	{
		Asset_GamemodePreset asset_GamemodePreset = (from x in GetPresets(qparams, context).Slinq()
			where x.isDefault
			select x).FirstOrDefault();
		if (asset_GamemodePreset == null)
		{
			asset_GamemodePreset = GetPresets(qparams, context).Slinq().FirstOrDefault();
		}
		return asset_GamemodePreset;
	}

	private string GetVictoryImagePath()
	{
		return $"{base.path}{__F_IMG_VICTORY}";
	}

	private string GetDefeatImagePath()
	{
		return $"{base.path}{__F_IMG_DEFEAT}";
	}

	private string GetIntroImagePath()
	{
		return $"{base.path}{__F_IMG_DEFEAT}";
	}

	public override void Disk_ReadMeta()
	{
		if (File.Exists(base.path + __F_META))
		{
			Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(base.path + __F_META));
			ReadBaseMeta(hashtable);
			if (hashtable.ContainsKey("__GAMEMODE_ORDER"))
			{
				order = (int)(long)hashtable["__GAMEMODE_ORDER"];
			}
		}
	}

	private void ApplyPresetSettingsToScript(Script script, Asset_GamemodePreset preset)
	{
		Table table = (Table)script.Globals["sv_Settings"];
		if (table == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> setting in preset.settings)
		{
			table[setting.Key] = setting.Value;
		}
	}

	public void Disk_LoadGamemode(out Script outscript, Asset_GamemodePreset preset)
	{
		Script outscript2 = null;
		if (File.Exists(base.path + __F_CLIENTSIDE))
		{
			LuaLoader.Load(out outscript2, base.path + __F_CLIENTSIDE, LuaLibs.BigLib, delegate(Script s)
			{
				ApplyPresetSettingsToScript(s, preset);
			});
		}
		if (NetworkServer.active && File.Exists(base.path + __F_SERVERSIDE))
		{
			if (outscript2 != null)
			{
				outscript2.DoFile(base.path + __F_SERVERSIDE);
			}
			else
			{
				LuaLoader.Load(out outscript2, base.path + __F_SERVERSIDE, LuaLibs.BigLib, delegate(Script s)
				{
					ApplyPresetSettingsToScript(s, preset);
				});
			}
		}
		outscript = outscript2;
	}

	public DynValue Disk_RunValidator(List<Dictionary<string, object>> table, Script ownerScript = null)
	{
		DynValue result;
		if (ownerScript == null)
		{
			Action<Script> preloadAction = delegate(Script script)
			{
				LuaVector3.RegisterClass(script);
				LuaQuaternion.RegisterClass(script);
				LuaMarker.RegisterClassValidation(script);
				script.Globals["__LUAMARKER_VALIDATION_LIST__"] = table;
			};
			result = LuaLoader.Load(out ownerScript, base.path + __F_VALIDATOR, preloadAction, null);
			result.Table.MakePrime();
			Script.Kill(ref ownerScript);
		}
		else
		{
			ownerScript.Globals["__LUAMARKER_VALIDATION_LIST__"] = table;
			result = ownerScript.LoadFile(base.path + __F_VALIDATOR).Function.Call();
		}
		return result;
	}

	public DynValue Disk_RunValidator(List<Dictionary<string, object>> table, out Script ownerScript)
	{
		Action<Script> preloadAction = delegate(Script script)
		{
			LuaVector3.RegisterClass(script);
			LuaQuaternion.RegisterClass(script);
			LuaMarker.RegisterClassValidation(script);
			script.Globals["__LUAMARKER_VALIDATION_LIST__"] = table;
		};
		DynValue result = LuaLoader.Load(out ownerScript, base.path + __F_VALIDATOR, preloadAction, null);
		result.Table.MakePrime();
		return result;
	}

	public FactionPrototype[] Disk_ReadFactionPrototypes()
	{
		Action<Script> preloadAction = delegate(Script script)
		{
			IconDB.RegisterClass(script);
			CrystalVisuals.RegisterClass(script);
		};
		Script outscript = null;
		DynValue dynValue = LuaLoader.Load(out outscript, base.path + __F_FACTIONS, preloadAction, null);
		FactionPrototype[] array = new FactionPrototype[dynValue.Table.Length];
		int num = 0;
		foreach (DynValue value in dynValue.Table.Values)
		{
			ref FactionPrototype reference = ref array[num];
			reference = new FactionPrototype(value.Table);
			num++;
		}
		Script.Kill(ref outscript);
		return array;
	}

	public Texture2D Disk_ReadVictoryImage()
	{
		texVictory = SubAsset_Texture2D.GetAssetSync(GetVictoryImagePath(), clearOnSceneChanged: true);
		return texVictory;
	}

	public Texture2D Disk_ReadDefeatImage()
	{
		texDefeat = SubAsset_Texture2D.GetAssetSync(GetDefeatImagePath(), clearOnSceneChanged: true);
		return texDefeat;
	}

	public Texture2D Disk_ReadIntroImage()
	{
		texIntro = SubAsset_Texture2D.GetAssetSync(GetIntroImagePath(), clearOnSceneChanged: true);
		return texIntro;
	}

	public void Disk_ReadVictoryImageAsync(Action<Texture2D> callback)
	{
		callback(Disk_ReadVictoryImage());
	}

	public void Disk_ReadDefeatImageAsync(Action<Texture2D> callback)
	{
		callback(Disk_ReadDefeatImage());
	}

	public void Disk_ReadIntroImageAsync(Action<Texture2D> callback)
	{
		callback(Disk_ReadIntroImage());
	}

	protected override void Disk_WriteMeta()
	{
		Hashtable hashtable = new Hashtable();
		FillBaseMeta(hashtable);
		Asset_Base<Asset_Gamemode>.Disk_WriteJsonData(hashtable, base.path + __F_META, humanReadable: true);
	}
}
