using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseMapLibraryMenu : BaseAssetLibraryMenu<Asset_Map>
{
	private List<Asset_Map> _loadedAssets = new List<Asset_Map>();

	private List<Asset_Map> _assets = new List<Asset_Map>();

	protected bool AssetsLoaded;

	public static GameSignals.GameSignal OnSetLoadedSignal = new GameSignals.GameSignal();

	public static GameSignals.GameSignal OnSetSelectedMapSignal = new GameSignals.GameSignal();

	public static GameSignals.GameSignal OnSetSelectedModeSignal = new GameSignals.GameSignal();

	public static GameSignals.GameSignal OnSetSelectedPresetSignal = new GameSignals.GameSignal();

	protected const string MapSizePrefixname = "##mapmenu_size";

	protected const string TutorialMapCodename = "TutorialIsland_ad7e7180-1316-4cbc-b66c-b1c055f0068c";

	protected const string WeEmptyMapForestCodeName = "we_new_1c3b9622-d7d2-4b75-84c2-f487f81b599b";

	protected const string WeEmptyMapDesertCodeName = "we_newdesert_1c3b9622-d7d2-4b75-84c2-f487f81b599b";

	protected const string WeEmptyMapWinterCodeName = "we_newwinter_edf8b123-3f26-4582-ae33-aea5e488eb43";

	protected const string CreditIslandMapCodeName = "CreditIsland_d19201d7-aec4-4079-9df3-bf02ab8d3f96";

	protected const string WeEmptyMapSubtitle = "str_we_newmap_sublabel";

	protected const string WeGamemodeCodeName = "world_editor";

	public const string ConquestGamemodeCodeName = "conquest";

	public const string InvasionGamemodeCodeName = "invasion";

	public const string SandboxGamemodeCodeName = "sandbox";

	public const string CreditIslandGamemodeCodeName = "creditisland";

	public static Signal MapReopenSignal = new Signal();

	protected override bool LoadOnInit => false;

	protected override bool ReloadOnOpen => true;

	protected override bool LoadOnOpen => true;

	protected abstract Asset_Map.MapHandler AssetHandler { get; }

	protected override List<Asset_Map> Assets => _assets;

	protected virtual bool LoadSubassets => false;

	protected override Asset_Map SelectedAsset
	{
		get
		{
			return GetSelectedMap();
		}
		set
		{
			SetSelectedMap(value, PlayableContext, GameContext, TypeContext);
		}
	}

	public static Asset_Gamemode WorldEditorGamemode => AssetDispensary.gamemodes.SendCodenameQuery("world_editor", AssetQueryParameters.AllSources, AssetPlayableContext.Any);

	protected static Asset_Gamemode WeGamemode => AssetDispensary.gamemodes.SendCodenameQuery("world_editor", AssetQueryParameters.AllSources, AssetPlayableContext.Any);

	protected BaseMapLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override void LoadSelf()
	{
		LoadAssets();
		base.LoadSelf();
	}

	protected override void UnloadSelf()
	{
		base.UnloadSelf();
		UnloadAssets();
	}

	protected override void CloseSelf()
	{
		base.CloseSelf();
		UnloadAssets();
	}

	protected void UnloadAssets()
	{
		foreach (Asset_Map loadedAsset in _loadedAssets)
		{
			if (!loadedAsset.Alive)
			{
				BaseMenu.logger.Info("UnloadAssets(): Unexpected dead asset.\n\"" + loadedAsset.displayName + "\"");
			}
			if (loadedAsset != GetLoadedMap())
			{
				loadedAsset.FreeTextureAlloc();
			}
		}
		_loadedAssets.Clear();
		AssetsLoaded = false;
	}

	protected void LoadAssets()
	{
		if (AssetsLoaded)
		{
			return;
		}
		_loadedAssets = ((!LoadSubassets) ? AssetHandler.SendQuery(AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Any) : AssetHandler.SendQuery(AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Any, loadSubassets: true));
		foreach (Asset_Map loadedAsset in _loadedAssets)
		{
			if (!loadedAsset.Alive)
			{
				BaseMenu.logger.Info("LoadAssets(): Unexpected dead asset.\n\"" + loadedAsset.displayName + "\"");
			}
		}
		_assets = _loadedAssets.Slinq().Where((Asset_Map asset, BaseMapLibraryMenu self) => self.PlayableContext == AssetPlayableContext.Any || asset.playableContext.HasFlag(self.PlayableContext), this).ToList();
		AssetsLoaded = true;
	}

	protected void ReloadAssets()
	{
		UnloadAssets();
		LoadAssets();
	}

	public static Asset_Map GetSelectedMap()
	{
		return GameParam.SelectedMap;
	}

	public static void SetSelectedMap(Asset_Map map)
	{
		GameParam.SelectedMap = map;
		GameSignals.Invoke(OnSetSelectedMapSignal);
	}

	public static void SetSelectedMap(Asset_Map map, AssetPlayableContext playableContext, MenuGameContext gameContext, MenuTypeContext typeContext)
	{
		GameParam.SelectedMap = map;
		GameParam.SelectedGamemode = GetMapDefaultGamemode(GameParam.SelectedMap, gameContext, typeContext, playableContext);
		GameParam.SelectedPreset = GetGamemodeDefaultPreset(GameParam.SelectedMap, GameParam.SelectedGamemode, playableContext);
		GameSignals.Invoke(OnSetSelectedMapSignal);
		GameSignals.Invoke(OnSetSelectedModeSignal);
		GameSignals.Invoke(OnSetSelectedPresetSignal);
	}

	public static Asset_Gamemode GetSelectedGamemode()
	{
		return GameParam.SelectedGamemode;
	}

	public static void SetSelectedGamemode(Asset_Gamemode mode)
	{
		GameParam.SelectedGamemode = mode;
		GameSignals.Invoke(OnSetSelectedModeSignal);
	}

	public static void SetSelectedGamemode(Asset_Map map, Asset_Gamemode mode, AssetPlayableContext playableContext)
	{
		GameParam.SelectedGamemode = mode;
		GameParam.SelectedPreset = GetGamemodeDefaultPreset(map, GameParam.SelectedGamemode, playableContext);
		GameSignals.Invoke(OnSetSelectedModeSignal);
		GameSignals.Invoke(OnSetSelectedPresetSignal);
	}

	public static Asset_GamemodePreset GetSelectedPreset()
	{
		return GameParam.SelectedPreset;
	}

	public static void SetSelectedPreset(Asset_GamemodePreset preset)
	{
		GameParam.SelectedPreset = preset;
		GameSignals.Invoke(OnSetSelectedPresetSignal);
	}

	public static Asset_Map GetLoadedMap()
	{
		return GameParam.Map;
	}

	public static void SetLoadedMap(Asset_Map map)
	{
		GameParam.Map = map;
		GameSignals.Invoke(OnSetLoadedSignal);
	}

	public static void SetLoadedMap(Asset_Map map, AssetPlayableContext playableContext, MenuGameContext gameContext, MenuTypeContext typeContext)
	{
		GameParam.Map = map;
		SetLoadedGamemode(GetMapDefaultGamemode(map, gameContext, typeContext, playableContext));
		GameSignals.Invoke(OnSetLoadedSignal);
	}

	public static Asset_Gamemode GetLoadedGamemode()
	{
		return GameParam.Gamemode;
	}

	public static void SetLoadedGamemode(Asset_Gamemode mode)
	{
		GameParam.Gamemode = mode;
	}

	public static void SetLoadedGamemode(Asset_Map map, Asset_Gamemode mode, AssetPlayableContext playableContext)
	{
		SetLoadedGamemode(mode);
		GameParam.Preset = GetGamemodeDefaultPreset(map, mode, playableContext);
	}

	public static Asset_GamemodePreset GetLoadedPreset()
	{
		return GameParam.Preset;
	}

	public static void SetLoadedPreset(Asset_GamemodePreset preset)
	{
		GameParam.Preset = preset;
	}

	public static void SetLoadedAsSelected()
	{
		GameParam.SelectedMap = GameParam.Map;
		GameParam.SelectedGamemode = GameParam.Gamemode;
		GameParam.SelectedPreset = GameParam.Preset;
	}

	public static void SetSelectedAsLoaded(bool emptySelected = false)
	{
		GameParam.Map = GameParam.SelectedMap;
		GameParam.Gamemode = GameParam.SelectedGamemode;
		GameParam.Preset = GameParam.SelectedPreset;
		if (emptySelected)
		{
			EmptySelected();
		}
	}

	public static void EmptySelected()
	{
		GameParam.SelectedMap = null;
		GameParam.SelectedGamemode = null;
		GameParam.SelectedPreset = null;
	}

	public static Asset_Gamemode GetMapDefaultGamemode(Asset_Map map, MenuGameContext gameContext, MenuTypeContext typeContext, AssetPlayableContext playableContext)
	{
		if (map == null)
		{
			return null;
		}
		List<Asset_Gamemode> modes = CollectionPool.RequestList<Asset_Gamemode>();
		GetMapGamemodes(ref modes, map, gameContext, typeContext, playableContext);
		if (modes.Count == 0)
		{
			CollectionPool.Release(ref modes);
			return null;
		}
		Asset_Gamemode result = modes[0];
		CollectionPool.Release(ref modes);
		return result;
	}

	public static void GetMapGamemodes(ref List<Asset_Gamemode> modes, Asset_Map map, MenuGameContext gameContext, MenuTypeContext typeContext, AssetPlayableContext playableContext)
	{
		if (map == null)
		{
			return;
		}
		if (gameContext == MenuGameContext.WorldEditor)
		{
			modes.Add(WorldEditorGamemode);
		}
		else if (typeContext == MenuTypeContext.Existing)
		{
			Asset_Gamemode lastPlayedGamemode = map.GetLastPlayedGamemode();
			if (lastPlayedGamemode != null)
			{
				modes.Add(lastPlayedGamemode);
			}
		}
		else
		{
			modes.AddRange(map.GetGamemodes(playableContext));
		}
	}

	public static Asset_GamemodePreset GetGamemodeDefaultPreset(Asset_Map map, Asset_Gamemode gamemode, AssetPlayableContext playableContext)
	{
		if (map == null)
		{
			return null;
		}
		Asset_GamemodePreset lastPlayedGamemodePreset = map.GetLastPlayedGamemodePreset();
		if (lastPlayedGamemodePreset != null)
		{
			return lastPlayedGamemodePreset;
		}
		return gamemode?.GetDefaultPreset(AssetQueryParameters.AllSources, playableContext);
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, MapReopenSignal, BaseMenu.OnReopen, this);
	}
}
