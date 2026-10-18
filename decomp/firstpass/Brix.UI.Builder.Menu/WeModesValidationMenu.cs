using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Lua;
using Brix.Steam;
using Brix.UI.Builder.Panel;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class WeModesValidationMenu : BaseMenu
{
	private const string _tutorialGamemodeCodeName = "tutorial";

	private const string _weGamemodeCodeName = "world_editor";

	private static List<WorkshopItemTags> _gameModeWorkshopItemTags = new List<WorkshopItemTags>();

	private static List<Asset_Gamemode> _standardGameModes;

	private static Dictionary<Asset_Gamemode, List<KeyValuePair<bool, string>>> _modeMessageMap;

	private static List<Asset_Gamemode> _supportedGameModes;

	private static List<Dictionary<string, object>> _mapLuaTable;

	private static readonly Action<Transform, WeModesValidationMenu> OnAfterChangeMarkers = delegate(Transform trs, WeModesValidationMenu menu)
	{
		menu.Reopen();
	};

	public static Signal ReOpenSignal = new Signal();

	public static List<WorkshopItemTags> GameModeWorkshopItemTags => _gameModeWorkshopItemTags;

	public static List<Asset_Gamemode> StandardGameModes => _standardGameModes;

	private static Dictionary<Asset_Gamemode, List<KeyValuePair<bool, string>>> ModeMessageMap => _modeMessageMap;

	private static List<Asset_Gamemode> SupportedGameModes => _supportedGameModes;

	private static List<Dictionary<string, object>> MapLuaTable => _mapLuaTable;

	protected override bool ReloadOnOpen => true;

	public WeModesValidationMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static WeModesValidationMenu Build(Func<RectTransform> binGetter)
	{
		return new WeModesValidationMenu(binGetter);
	}

	public static List<BasePanel> GetBuilders(WeModesValidationMenu menuN)
	{
		List<BasePanel> list = new List<BasePanel>();
		List<Asset_Gamemode> standardGameModes = StandardGameModes;
		for (int i = 0; i < standardGameModes.Count; i++)
		{
			int index = i;
			Asset_Gamemode mode = standardGameModes[index];
			list.AddRange(GetSubBuilders(mode, menuN));
		}
		return list;
	}

	public static List<BasePanel> GetSubBuilders(Asset_Gamemode mode, WeModesValidationMenu menuN)
	{
		List<BasePanel> list = new List<BasePanel>();
		bool isSupported = SupportedGameModes.Contains(mode);
		Action<CheckboxButtonPanelComponent> onReset = delegate(CheckboxButtonPanelComponent panelN)
		{
			panelN._label.text = mode.displayName;
			panelN._toggle.isOn = isSupported;
			panelN._toggle.graphic.color = Colors.Gray;
			panelN._toggle.interactable = false;
			panelN._spacerLayoutElement.gameObject.SetActive(value: false);
		};
		list.Add(CheckboxButtonPanelComponent.Build(onReset));
		foreach (KeyValuePair<bool, string> item in ModeMessageMap[mode])
		{
			bool isSupported2 = item.Key;
			string supportMessage = item.Value;
			Action<CheckboxButtonPanelComponent> onReset2 = delegate(CheckboxButtonPanelComponent panelN)
			{
				panelN._label.text = supportMessage;
				panelN._toggle.isOn = isSupported2;
				panelN._toggle.graphic.color = Colors.Gray;
				panelN._toggle.interactable = false;
				panelN._spacerLayoutElement.gameObject.SetActive(value: true);
			};
			list.Add(CheckboxButtonPanelComponent.Build(onReset2));
		}
		return list;
	}

	public static void ReloadGameModeWorkshopItemTags()
	{
		_gameModeWorkshopItemTags = SupportedGameModes.Where((Asset_Gamemode mode) => WorkshopItem.WorkshopItemTagList.Contains(WorkshopItem.GetWorkshopItemTag(mode))).Select(WorkshopItem.GetWorkshopItemTag).ToList();
	}

	public static void ReloadStandardGameModes()
	{
		List<Asset_Gamemode> source = AssetDispensary.gamemodes.SendQuery(AssetQueryParameters.AllSPSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.SPandMP);
		source = source.Where((Asset_Gamemode mode) => mode.codename != "tutorial" && mode.codename != "world_editor" && (mode.isSupported || Neo.showUnsupportedGameModes)).ToList();
		source.Reverse();
		_standardGameModes = source;
	}

	public static void ReloadSupportedGameModes()
	{
		Dictionary<Asset_Gamemode, List<KeyValuePair<bool, string>>> dictionary = new Dictionary<Asset_Gamemode, List<KeyValuePair<bool, string>>>();
		List<Asset_Gamemode> list = new List<Asset_Gamemode>();
		foreach (Asset_Gamemode standardGameMode in StandardGameModes)
		{
			Table table = standardGameMode.Disk_RunValidator(MapLuaTable).Table;
			if (table.Get(1).Boolean)
			{
				list.Add(standardGameMode);
			}
			List<KeyValuePair<bool, string>> list2 = new List<KeyValuePair<bool, string>>();
			dictionary.Add(standardGameMode, list2);
			for (int i = 2; i <= table.Length; i++)
			{
				bool boolean = table.Get(i).Table.Get(1).Boolean;
				string value = table.Get(i).Table.Get(2).String;
				list2.Add(new KeyValuePair<bool, string>(boolean, value));
			}
		}
		_supportedGameModes = list;
		_modeMessageMap = dictionary;
	}

	public static void ReloadMapLuaTable()
	{
		_mapLuaTable = LuaMarker.GetSerialisedMarkers();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, ReOpenSignal, BaseMenu.OnReopen, this).AddSignalConnection(MenuState.Opened, We.AfterChangeTabSignal, BaseMenu.OnReopen, this)
			.AddSignalConnection(MenuState.Opened, UIGameObserver.worldEditorObjects.Added, OnAfterChangeMarkers, this)
			.AddSignalConnection(MenuState.Opened, UIGameObserver.worldEditorObjects.Removed, OnAfterChangeMarkers, this);
	}
}
