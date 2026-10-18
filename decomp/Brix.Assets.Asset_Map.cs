using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.Engine.FlatWorld;
using Brix.Engine.Nature;
using Brix.Game;
using Brix.IO.Serialization;
using Brix.Input;
using Brix.Legacy;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Assets;

public class Asset_Map : Asset_Base<Asset_Map>
{
	public class TreeInfo
	{
		public int x;

		public int y;

		public int z;

		public int s = 15;
	}

	public enum MapType
	{
		Unknown,
		Map,
		Save
	}

	public enum MapSize
	{
		Unknown,
		Large,
		Medium,
		Small
	}

	public enum SaveType
	{
		Unkown,
		Map,
		Manualsave,
		Autosave,
		Quicksave
	}

	public class SaveHandler : MapHandler
	{
		public List<Asset_Map> SendSaveQuery(SaveType savetype, AssetQueryParameters qparams = AssetQueryParameters.AllSources, AssetPlayableContext pcontexts = AssetPlayableContext.Any)
		{
			return SendQuery(qparams, pcontexts).Slinq().Where((Asset_Map x, SaveType aSaveType) => x.saveType == aSaveType, savetype).ToList();
		}
	}

	public class MapHandler : AssetHandler<Asset_Map>
	{
		protected override void LoadSubassets(List<Asset_Map> assets)
		{
			Script tokill = null;
			List<Asset_Gamemode> list = AssetDispensary.gamemodes.SendQuery();
			foreach (Asset_Map asset in assets)
			{
				List<Dictionary<string, object>> list2 = null;
				try
				{
					list2 = asset.Disk_ReadLuaMarkersCount();
				}
				catch (Exception message)
				{
					logger.Error(message);
				}
				if (list2 == null)
				{
					continue;
				}
				List<Asset_Gamemode> list3 = new List<Asset_Gamemode>();
				foreach (Asset_Gamemode item in list)
				{
					if ((tokill != null) ? item.Disk_RunValidator(list2, tokill).Table.Get(1).Boolean : item.Disk_RunValidator(list2, out tokill).Table.Get(1).Boolean)
					{
						list3.Add(item);
					}
				}
				asset.SetSupportedGamemodes(list3);
			}
			Script.Kill(ref tokill);
			base.LoadSubassets(assets);
		}
	}

	private static readonly string __F_FACTIONS;

	private static readonly string __F_GAMEOBJECTS;

	private static readonly string __F_HEIGHTMAP;

	private static readonly string __F_LUAMARKERSCOUNT;

	private static readonly string __F_META;

	private static readonly string __F_PLACEDBLOCKS;

	private static readonly string __F_PLANTS;

	private static readonly string __F_BOULDERS;

	private static readonly string __F_IRON;

	private static readonly string __F_PYRITE;

	private static readonly string __F_TERRAINDAMAGE;

	private static readonly string __F_THUMBNAIL;

	private static readonly string __F_TREES;

	private static readonly string __F_WEGUIDE;

	private static readonly string __F_NOISE;

	private static BrixLogger logger;

	public string _lastPlayedGamemode = string.Empty;

	public string _lastPlayedGamemodePreset = string.Empty;

	public bool isDebug;

	public MapType mapType;

	public MapSize mapSize;

	public SaveType saveType;

	public string biomeName;

	private string[] _supportedGMCodenames;

	private List<Asset_Gamemode> _supportedGamemodes;

	private Texture2D textGuide;

	private static readonly char[] TmpSplit;

	public bool restrictGamemode => _supportedGMCodenames != null && _supportedGMCodenames.Length > 0;

	public BlockDataStruct[] Disk_ReadPlacedBlocks
	{
		get
		{
			BlockDataStruct[] data;
			return (!Disk_ReadJsonData<BlockDataStruct[]>(__F_PLACEDBLOCKS, out data)) ? new BlockDataStruct[0] : data;
		}
	}

	static Asset_Map()
	{
		__F_FACTIONS = "Factions.json";
		__F_GAMEOBJECTS = "gameobjects.json";
		__F_HEIGHTMAP = "Heightmap.png";
		__F_LUAMARKERSCOUNT = "LuaMarkersCount.json";
		__F_META = "meta.json";
		__F_PLACEDBLOCKS = "PlacedBlocks.json";
		__F_PLANTS = "plants.json";
		__F_BOULDERS = "boulders.json";
		__F_IRON = "iron.json";
		__F_PYRITE = "pyrite.json";
		__F_TERRAINDAMAGE = "TerrainDamage.json";
		__F_THUMBNAIL = "thumbnail.png";
		__F_TREES = "trees.json";
		__F_WEGUIDE = "Guide.jpg";
		__F_NOISE = "noise.json";
		logger = new BrixLogger(typeof(Asset_Map).FullName);
		TmpSplit = new char[1] { '/' };
		Asset_Base<Asset_Map>.__CURRENTVERSIONS.Add(typeof(Asset_Map), 25);
	}

	public Asset_Map(string uri)
		: base(uri)
	{
		try
		{
			Disk_ReadMeta();
		}
		catch (Exception e)
		{
			logger.ErrorFormat(e, "Error when loading map at '{0}'", uri);
			SetInvalidMapInfo();
		}
	}

	public List<Asset_Gamemode> GetGamemodes(AssetPlayableContext context = AssetPlayableContext.Any)
	{
		if (_supportedGamemodes == null)
		{
			SetSupportedGamemodes();
		}
		if (context == AssetPlayableContext.Any)
		{
			return _supportedGamemodes;
		}
		return (from x in _supportedGamemodes.Slinq().Where((Asset_Gamemode x, AssetPlayableContext ctx) => x?.playableContext.HasFlag(ctx) ?? false, context)
			orderby x.order descending
			select x).ToList();
	}

	public int GetMarkerGroupCount(string markerGroup)
	{
		List<Dictionary<string, object>> list = null;
		try
		{
			list = Disk_ReadLuaMarkersCount();
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		return (from l in list?.Slinq()
			where l["markerGroups"] is List<string> list2 && list2.Contains("Player Starting Point")
			select l).Count() ?? 0;
	}

	public Asset_Gamemode GetLastPlayedGamemode()
	{
		return (!string.IsNullOrEmpty(_lastPlayedGamemode)) ? AssetDispensary.gamemodes.SendCodenameQuery(_lastPlayedGamemode) : null;
	}

	public Asset_GamemodePreset GetLastPlayedGamemodePreset()
	{
		if (_lastPlayedGamemodePreset == "Outdated")
		{
			return null;
		}
		return (!string.IsNullOrEmpty(_lastPlayedGamemodePreset)) ? AssetDispensary.presets.SendCodenameQuery(_lastPlayedGamemodePreset) : null;
	}

	protected override string GetThumbnailPath()
	{
		return $"{base.path}{__F_THUMBNAIL}";
	}

	private string GetHeightmapPath()
	{
		return $"{base.path}{__F_HEIGHTMAP}";
	}

	public int TryGetMarkersCount(string name)
	{
		if (!IsShell())
		{
			List<Dictionary<string, object>> list = null;
			try
			{
				list = Disk_ReadLuaMarkersCount();
			}
			catch (Exception message)
			{
				logger.Error(message);
			}
			if (list == null)
			{
				return 0;
			}
			List<Dictionary<string, object>> list2 = LuaMarker.GetGroup(name, list);
			if (list2 != null)
			{
				return list2.Count;
			}
		}
		return 0;
	}

	public bool IsNewGame()
	{
		return mapType == MapType.Map;
	}

	public bool IsUpgradeable()
	{
		return SaveUpdater.IsUpdatable(this);
	}

	private void SetSupportedGamemodes(List<Asset_Gamemode> gms = null)
	{
		if (gms != null)
		{
			_supportedGamemodes = gms;
			if (restrictGamemode)
			{
				_supportedGamemodes = _supportedGamemodes.Slinq().Where((Asset_Gamemode x, Asset_Map self) => self._supportedGMCodenames.Slinq().Contains(x.codename), this).ToList();
			}
			return;
		}
		List<Asset_Gamemode> list = AssetDispensary.gamemodes.SendQuery();
		if (restrictGamemode)
		{
			_supportedGamemodes = list.Slinq().Where((Asset_Gamemode x, Asset_Map self) => self._supportedGMCodenames.Slinq().Contains(x.codename), this).ToList();
			return;
		}
		List<Dictionary<string, object>> lmc = null;
		try
		{
			lmc = Disk_ReadLuaMarkersCount();
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		if (lmc != null)
		{
			_supportedGamemodes = new List<Asset_Gamemode>();
			list.Slinq().ForEach(delegate(Asset_Gamemode x)
			{
				try
				{
					if (x.Disk_RunValidator(lmc).Table.Get(1).Boolean)
					{
						_supportedGamemodes.Add(x);
					}
				}
				catch (Exception message2)
				{
					logger.Error(message2);
				}
			});
		}
		else
		{
			_supportedGamemodes = new List<Asset_Gamemode> { AssetDispensary.gamemodes.SendCodenameQuery("nothing") };
		}
	}

	public void SetRestrictedGamemodes(List<Asset_Gamemode> gamemodes)
	{
		if (gamemodes == null)
		{
			_supportedGMCodenames = new string[0];
			return;
		}
		_supportedGMCodenames = (from x in gamemodes.Slinq()
			select x.codename).ToList().ToArray();
	}

	public static Asset_Map CreateShell(MapType type)
	{
		Asset_Map asset_Map = new Asset_Map(string.Empty);
		asset_Map.mapType = type;
		return asset_Map;
	}

	public static Asset_Map CreateShellFrom(Asset_Map otherMap, MapType type)
	{
		Asset_Map asset_Map = new Asset_Map(string.Empty);
		asset_Map.FillFrom(otherMap, includeMapType: false);
		asset_Map.mapType = type;
		return asset_Map;
	}

	public void FillFrom(Asset_Map otherMap, bool includeMapType)
	{
		base.codename = otherMap.codename;
		base.rawDisplayName = otherMap.rawDisplayName;
		customName = otherMap.customName;
		base.rawDescription = otherMap.rawDescription;
		base.playableContext = otherMap.playableContext;
		isDebug = otherMap.isDebug;
		if (includeMapType)
		{
			mapType = otherMap.mapType;
		}
		mapSize = otherMap.mapSize;
		saveType = otherMap.saveType;
		_supportedGMCodenames = otherMap._supportedGMCodenames;
		_lastPlayedGamemode = otherMap._lastPlayedGamemode;
		_lastPlayedGamemodePreset = otherMap._lastPlayedGamemodePreset;
		thumbnail = otherMap.thumbnail;
	}

	public override void Disk_ReadMeta()
	{
		string text = $"{base.path}{__F_META}";
		if (File.Exists(text))
		{
			Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(text));
			if (hashtable == null)
			{
				logger.ErrorFormat("Disk_ReadMeta() unexpected null table at {0}", base.path);
				SetInvalidMapInfo();
				return;
			}
			ReadBaseMeta(hashtable);
			if (hashtable.ContainsKey("version"))
			{
				SetMapInfoFromLegacy(hashtable);
				return;
			}
			if (hashtable.ContainsKey("__ISDEBUG"))
			{
				isDebug = (bool)hashtable["__ISDEBUG"];
			}
			if (hashtable.ContainsKey("__MAPTYPE"))
			{
				mapType = ((hashtable["__MAPTYPE"].ToString() == "Map") ? MapType.Map : MapType.Save);
			}
			if (hashtable.ContainsKey("__MAPSIZE"))
			{
				mapSize = (MapSize)Enum.Parse(typeof(MapSize), hashtable["__MAPSIZE"].ToString(), ignoreCase: true);
			}
			if (hashtable.ContainsKey("__SAVETYPE"))
			{
				saveType = (SaveType)Enum.Parse(typeof(SaveType), hashtable["__SAVETYPE"].ToString(), ignoreCase: true);
			}
			if (hashtable.ContainsKey("__BIOMETYPE"))
			{
				biomeName = (string)hashtable["__BIOMETYPE"];
			}
			if (hashtable.ContainsKey("__SUPPORTED_GAMEMODES"))
			{
				object obj = hashtable["__SUPPORTED_GAMEMODES"];
				if (obj == null)
				{
					_supportedGMCodenames = new string[0];
				}
				else
				{
					_supportedGMCodenames = ((JArray)obj).ToObject<string[]>();
				}
			}
			if (hashtable.ContainsKey("__LASTPLAYED_GAMEMODE"))
			{
				object obj2 = hashtable["__LASTPLAYED_GAMEMODE"];
				if (obj2 == null)
				{
					_lastPlayedGamemode = string.Empty;
				}
				else
				{
					_lastPlayedGamemode = obj2.ToString();
				}
			}
			if (hashtable.ContainsKey("__LASTPLAYED_GAMEMODEPRESET"))
			{
				object obj3 = hashtable["__LASTPLAYED_GAMEMODEPRESET"];
				if (obj3 == null)
				{
					_lastPlayedGamemodePreset = ((mapType != MapType.Map) ? "Outdated" : "None");
				}
				else
				{
					_lastPlayedGamemodePreset = obj3.ToString();
				}
			}
			else
			{
				_lastPlayedGamemodePreset = ((mapType != MapType.Map) ? "Outdated" : "None");
			}
		}
		else
		{
			SetInvalidMapInfo();
		}
	}

	public override Texture2D Disk_ReadThumbnail()
	{
		thumbnail = SubAsset_Texture2D.GetAssetSync(GetThumbnailPath(), clearOnSceneChanged: true);
		return thumbnail;
	}

	public Texture2D Disk_ReadHeightmap()
	{
		heightmap = SubAsset_Texture2D.GetAssetSync(GetHeightmapPath(), clearOnSceneChanged: true);
		return heightmap;
	}

	public override void Disk_ReadThumbnailAsync(Action<Texture2D> callback)
	{
		SubAsset_Texture2D.GetAssetAsync(GetThumbnailPath(), delegate(Texture2D aTex, Action<Texture2D> aCallback, Asset_Map aMapAsset)
		{
			aMapAsset.thumbnail = aTex;
			aCallback(aTex);
		}, callback, this, clearOnSceneChanged: true);
	}

	public void Disk_ReadHeightmapAsync(Action<Texture2D> callback)
	{
		SubAsset_Texture2D.GetAssetAsync(GetHeightmapPath(), delegate(Texture2D aTex, Action<Texture2D> aCallback, Asset_Map aMapAsset)
		{
			aMapAsset.heightmap = aTex;
			aCallback(aTex);
		}, callback, this, clearOnSceneChanged: true);
	}

	public TreeInfo[] Disk_ReadTrees()
	{
		if (Disk_ReadJsonData<TreeInfo[]>(__F_TREES, out var data))
		{
			return data;
		}
		return new TreeInfo[0];
	}

	public void Disk_EstimateTrees(out int treeCount, out int treeMvCount)
	{
		string fileName = base.path + __F_TREES;
		if (File.Exists(fileName))
		{
			treeCount = (int)(new FileInfo(fileName).Length / 26);
			treeMvCount = treeCount * 20 / 256;
		}
		else
		{
			treeCount = 0;
			treeMvCount = 0;
		}
	}

	public PlantSerializedData[] Disk_ReadPlants()
	{
		if (Disk_ReadJsonData<PlantSerializedData[]>(__F_PLANTS, out var data))
		{
			return data;
		}
		return new PlantSerializedData[0];
	}

	public BoulderSerializedData[] Disk_ReadBoulders()
	{
		if (Disk_ReadJsonData<BoulderSerializedData[]>(__F_BOULDERS, out var data))
		{
			return data;
		}
		return new BoulderSerializedData[0];
	}

	public MineralSerializedData[] Disk_ReadIrons()
	{
		if (Disk_ReadJsonData<MineralSerializedData[]>(__F_IRON, out var data))
		{
			return data;
		}
		return new MineralSerializedData[0];
	}

	public MineralSerializedData[] Disk_ReadPyrites()
	{
		if (Disk_ReadJsonData<MineralSerializedData[]>(__F_PYRITE, out var data))
		{
			return data;
		}
		return new MineralSerializedData[0];
	}

	public string Disk_ReadGameObjects()
	{
		string text = base.path + __F_GAMEOBJECTS;
		if (File.Exists(text))
		{
			return File.ReadAllText(text);
		}
		return string.Empty;
	}

	public Faction.FactionMeta[] Disk_ReadFactionMeta()
	{
		Disk_ReadJsonData<Faction.FactionMeta[]>(__F_FACTIONS, out var data);
		return data;
	}

	public Hashtable Disk_ReadNoise()
	{
		Hashtable data = null;
		if (File.Exists(base.path + __F_NOISE))
		{
			Disk_ReadJsonData<Hashtable>(__F_NOISE, out data);
		}
		return data;
	}

	public List<Dictionary<string, object>> Disk_ReadLuaMarkersCount()
	{
		List<Dictionary<string, object>> data = null;
		Disk_ReadJsonData<List<Dictionary<string, object>>>(__F_LUAMARKERSCOUNT, out data);
		if (data != null)
		{
			for (int i = 0; i != data.Count; i++)
			{
				if (data[i]["position"].GetType() == typeof(JObject))
				{
					string value = ((JObject)data[i]["position"]).ToString();
					data[i]["position"] = JsonConvert.DeserializeObject<Vector3>(value);
				}
				if (data[i]["localPosition"].GetType() == typeof(JObject))
				{
					string value2 = ((JObject)data[i]["localPosition"]).ToString();
					data[i]["localPosition"] = JsonConvert.DeserializeObject<Vector3>(value2);
				}
			}
		}
		return data;
	}

	public Texture2D Disk_ReadWorldEditorGuide()
	{
		string text = base.path + __F_WEGUIDE;
		if (File.Exists(text))
		{
			textGuide = new Texture2D(1024, 1024);
			textGuide.LoadImage(File.ReadAllBytes(text));
			return textGuide;
		}
		return null;
	}

	public override void FreeTextureAlloc()
	{
		base.FreeTextureAlloc();
		UnityEngine.Object.Destroy(textGuide);
	}

	~Asset_Map()
	{
	}

	protected override void Disk_WriteMeta()
	{
		Disk_WriteMeta(mapType);
	}

	protected void Disk_WriteMeta(MapType type)
	{
		string value = ((mapType != MapType.Save) ? "None" : ((_lastPlayedGamemodePreset == "None") ? GameParam.Preset.codename : ((!(_lastPlayedGamemodePreset == "Outdated")) ? _lastPlayedGamemodePreset : "Outdated")));
		Hashtable hashtable = new Hashtable();
		hashtable.Add("__MAPTYPE", Enum.GetName(typeof(MapType), type));
		hashtable.Add("__MAPSIZE", Enum.GetName(typeof(MapSize), mapSize));
		hashtable.Add("__SAVETYPE", Enum.GetName(typeof(SaveType), saveType));
		hashtable.Add("__BIOMETYPE", FlatWorldInitializer.Instance.biomeName);
		hashtable.Add("__SUPPORTED_GAMEMODES", _supportedGMCodenames);
		hashtable.Add("__RESTRICT_GAMEMODES", restrictGamemode);
		hashtable.Add("__LASTPLAYED_GAMEMODE", (mapType != MapType.Map) ? GameParam.Gamemode.codename : string.Empty);
		hashtable.Add("__LASTPLAYED_GAMEMODEPRESET", value);
		hashtable.Add("__ISDEBUG", isDebug);
		Hashtable hashtable2 = hashtable;
		FillBaseMeta(hashtable2);
		Asset_Base<Asset_Map>.Disk_WriteJsonData(hashtable2, base.path + __F_META, humanReadable: true);
	}

	private void Disk_WriteVoxelEngineData()
	{
		VoxelEngine.Save(base.path);
	}

	protected override void Disk_WriteThumbnail()
	{
		if (thumbnail != null)
		{
			File.WriteAllBytes(GetThumbnailPath(), thumbnail.EncodeToPNG());
		}
	}

	private void Disk_WriteTrees(TreeInfo[] treePositions)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(treePositions, base.path + __F_TREES);
	}

	private void Disk_WriteTrees(XYZ[] treePositions)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(treePositions, base.path + __F_TREES);
	}

	private void Disk_WritePlants(PlantSerializedData[] plants)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(plants, base.path + __F_PLANTS);
	}

	private void Disk_WriteBoulders(BoulderSerializedData[] boulders)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(boulders, base.path + __F_BOULDERS);
	}

	private void Disk_WriteIrons(MineralSerializedData[] iron)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(iron, base.path + __F_IRON);
	}

	private void Disk_WritePyrites(MineralSerializedData[] pyrite)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(pyrite, base.path + __F_PYRITE);
	}

	private void Disk_WriteTerrainDamage(Dictionary<XYZ, float> allTerrainDamageData)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(allTerrainDamageData, base.path + __F_TERRAINDAMAGE);
	}

	private void Disk_WriteGameObjects(string gameObjectsInJson)
	{
		File.WriteAllText(base.path + __F_GAMEOBJECTS, gameObjectsInJson);
	}

	private void Disk_WritePlacedBlocks(BlockDataStruct[] allRootBlockData)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(allRootBlockData, base.path + __F_PLACEDBLOCKS);
	}

	private void Disk_WriteFactionMeta(Faction.FactionMeta[] allFactionsData)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(allFactionsData, base.path + __F_FACTIONS);
	}

	private void Disk_WriteLuaMarkersCount(List<Dictionary<string, object>> mapLuaMarkers)
	{
		Asset_Base<Asset_Map>.Disk_WriteJsonData(mapLuaMarkers, base.path + __F_LUAMARKERSCOUNT, humanReadable: true);
	}

	private void Disk_WriteWorldEditorGuide()
	{
		Texture2D guide = Picking.Instance.WorldEditorGrid.GetGuide();
		if (guide != null)
		{
			File.WriteAllBytes(base.path + __F_WEGUIDE, guide.EncodeToJPG(100));
		}
	}

	private void Disk_WriteNoiseData()
	{
		if (InputModeController.GetCurrentMode() != InputMode.worldEditor)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("__GLOBALSEED", VoxelNoiseConfigurator.GetGlobalSeed());
			hashtable.Add("__NOISETYPES", VoxelNoiseConfigurator.GetAllNoiseTypes());
			Hashtable data = hashtable;
			Asset_Base<Asset_Map>.Disk_WriteJsonData(data, base.path + __F_NOISE);
		}
	}

	public void Disk_WriteSpecific(MapType meta = MapType.Unknown, bool voxelEngine = false, bool thumbnail = false, XYZ[] treePositions = null, PlantSerializedData[] plants = null, Dictionary<XYZ, float> allTerrainDamageData = null, string gameObjectsInJson = null, BlockDataStruct[] allRootBlockData = null, Faction.FactionMeta[] allFactionsData = null, List<Dictionary<string, object>> mapMarkerTable = null)
	{
		if (meta != MapType.Unknown)
		{
			Disk_WriteMeta(meta);
		}
		if (voxelEngine)
		{
			Disk_WriteVoxelEngineData();
		}
		if (thumbnail)
		{
			Disk_WriteThumbnail();
		}
		if (treePositions != null)
		{
			Disk_WriteTrees(treePositions);
		}
		if (plants != null)
		{
			Disk_WritePlants(plants);
		}
		if (allTerrainDamageData != null)
		{
			Disk_WriteTerrainDamage(allTerrainDamageData);
		}
		if (gameObjectsInJson != null)
		{
			Disk_WriteGameObjects(gameObjectsInJson);
		}
		if (allRootBlockData != null)
		{
			Disk_WritePlacedBlocks(allRootBlockData);
		}
		if (allFactionsData != null)
		{
			Disk_WriteFactionMeta(allFactionsData);
		}
		if (mapMarkerTable != null)
		{
			Disk_WriteLuaMarkersCount(mapMarkerTable);
		}
	}

	public override void Disk_Overwrite()
	{
		MoveToStaging();
		Disk_Save();
		ApplyStaging();
		if (mapType == MapType.Map)
		{
			GameSignals.Invoke(GameSignals.OnAutoFadeDialog, I2Helper.TryGet("##saved_map"), 1f);
			return;
		}
		GameSignals.Invoke(GameSignals.GameSaved);
		GameSignals.Invoke(GameSignals.OnAutoFadeDialog, I2Helper.TryGet("##saved_game"), 1f);
	}

	public override void Disk_WriteAsNew(AssetAuthorType author)
	{
		base.authorType = author;
		MoveToStaging();
		Disk_Save();
		if (mapType == MapType.Map)
		{
			base.path = AssetDispensary.maps.GetProperPath(this) + ((!string.IsNullOrEmpty(base.codename)) ? base.codename : Path.GetRandomFileName()) + "/";
		}
		else
		{
			base.path = AssetDispensary.saves.GetProperPath(this) + ((!string.IsNullOrEmpty(customName)) ? customName : Path.GetRandomFileName()) + "/";
		}
		ApplyStaging();
		if (mapType == MapType.Map)
		{
			GameSignals.Invoke(GameSignals.OnAutoFadeDialog, I2Helper.TryGet("##saved_map"), 5f);
			return;
		}
		GameSignals.Invoke(GameSignals.GameSaved);
		GameSignals.Invoke(GameSignals.OnAutoFadeDialog, I2Helper.TryGet("##saved_game"), 2f);
	}

	private void Disk_Save()
	{
		Disk_WriteMeta(mapType);
		if (thumbnail != null)
		{
			Disk_WriteThumbnail();
		}
		logger.Debug("Architecte.Save() BigIsland Data to be saved");
		Disk_WriteVoxelEngineData();
		Disk_WriteFactionMeta(Faction.GetAllFactionsMeta());
		Disk_WriteTrees(TreeRegistry.Instance.ListAllTreePositionAndSize().Slinq().ToArray());
		Disk_WritePlants(BrixSingleton<PlantRegistry>.Instance.SerializedData);
		Disk_WriteBoulders(BoulderRegistry.GetSerializedBoulders());
		Disk_WriteIrons(IronRegistry.GetSerializedMinerals());
		Disk_WritePyrites(PyriteRegistry.GetSerializedMinerals());
		Disk_WriteTerrainDamage(TerrainDamageTracker.Instance.AllTerrainDamageData());
		Disk_WritePlacedBlocks(BrixSingleton<BlockEngine>.Instance.AllRootBlockData());
		Disk_WriteLuaMarkersCount(LuaMarker.GetSerialisedMarkers());
		Disk_WriteWorldEditorGuide();
		Disk_WriteNoiseData();
		GameObjectSerializer gameObjectSerializer = UnityEngine.Object.FindObjectOfType<GameObjectSerializer>();
		Disk_WriteGameObjects(gameObjectSerializer.GetJsonGameState((mapType != MapType.Map) ? "save" : "map"));
	}

	private void SetMapInfoFromLegacy(Hashtable table)
	{
		SetInvalidMapInfo();
		string text = $"{base.path}/meta/info.txt";
		if (File.Exists(text))
		{
			Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(text));
			if (hashtable.ContainsKey("GameModes"))
			{
				_supportedGMCodenames = (from mode in ((JArray)hashtable["GameModes"]).ToObject<string[]>().Slinq()
					select mode.ToLower()).ToArray();
			}
		}
		if (table.ContainsKey("type"))
		{
			saveType = ((table["type"].ToString() == "New") ? SaveType.Map : SaveType.Manualsave);
		}
		if (table.ContainsKey("version"))
		{
			version = int.Parse(table["version"].ToString());
		}
		base.rawDisplayName = $"{base.path.Remove(base.path.Length - 1).Split(TmpSplit).Slinq().Last()} (Outdated)";
		customName = base.rawDisplayName;
	}

	private void SetInvalidMapInfo()
	{
		mapType = MapType.Map;
		mapSize = MapSize.Unknown;
		saveType = SaveType.Map;
		base.playableContext = AssetPlayableContext.SPandMP;
		_codename = Path.GetRandomFileName();
		base.rawDisplayName = "Outdated";
		customName = "Outdated";
		base.rawDescription = "Outdated";
		_lastPlayedGamemodePreset = "Outdated";
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
