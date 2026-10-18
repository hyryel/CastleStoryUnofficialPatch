using System;
using Brix.Assets;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class WeSaveActionsMenu : BaseSaveActionsMenu
{
	private const string VoidMapCodeName = "worldeditor_newmap";

	public static Signal AfterMapInfoChangeSignal = new Signal();

	public static Signal AfterMapOptionsChangeSignal = new Signal();

	public static bool IsValid => IsDisplayNameValid;

	public static bool IsDisplayNameValid => !string.IsNullOrEmpty(DisplayName);

	public static bool IsDescriptionValid => !string.IsNullOrEmpty(Description);

	public static string DisplayName => (GameParam.Map != null) ? GameParam.Map.displayName : string.Empty;

	public static string RawDisplayName
	{
		get
		{
			return (GameParam.Map != null) ? GameParam.Map.rawDisplayName : string.Empty;
		}
		set
		{
			GameParam.Map.rawDisplayName = value;
			AfterMapInfoChangeSignal.Invoke();
		}
	}

	public static string Description => (GameParam.Map != null) ? GameParam.Map.description : string.Empty;

	public static string RawDescription
	{
		get
		{
			return (GameParam.Map != null) ? GameParam.Map.rawDescription : string.Empty;
		}
		set
		{
			GameParam.Map.rawDescription = value;
			AfterMapInfoChangeSignal.Invoke();
		}
	}

	public WeSaveActionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static void Save(Action<bool> onComplete = null)
	{
		Texture2D thumbnail = UIGameObserver.TmpMapThumbnail ?? GameParam.Map.Disk_ReadThumbnail();
		if (GameParam.Map.codename == "worldeditor_newmap" || GameParam.Map.authorType == AssetAuthorType.Official)
		{
			Asset_Map asset_Map = Asset_Map.CreateShell(Asset_Map.MapType.Map);
			asset_Map.codename = BaseSaveActionsMenu.GetValidFileName(GameParam.Map.codename);
			asset_Map.rawDisplayName = RawDisplayName;
			asset_Map.customName = string.Empty;
			asset_Map.rawDescription = RawDescription;
			asset_Map.mapSize = Asset_Map.MapSize.Small;
			asset_Map.thumbnail = thumbnail;
			asset_Map.workshopType = GameParam.Map.workshopType;
			asset_Map.workshopTags = WeMapWorkshopEditOptionsMenu.Tags;
			asset_Map.playableContext |= AssetPlayableContext.WorldEditor;
			asset_Map.playableContext |= AssetPlayableContext.Singleplayer;
			asset_Map.playableContext |= AssetPlayableContext.Multiplayer;
			asset_Map.SetRestrictedGamemodes(null);
			asset_Map.Disk_WriteAsNew(AssetAuthorType.UGC);
			GameParam.Map = asset_Map;
		}
		else
		{
			Asset_Map map = GameParam.Map;
			map.rawDisplayName = RawDisplayName;
			map.rawDescription = RawDescription;
			map.mapSize = Asset_Map.MapSize.Small;
			map.thumbnail = thumbnail;
			map.playableContext |= AssetPlayableContext.WorldEditor;
			map.playableContext |= AssetPlayableContext.Singleplayer;
			map.playableContext |= AssetPlayableContext.Multiplayer;
			map.workshopTags = WeMapWorkshopEditOptionsMenu.Tags;
			map.SetRestrictedGamemodes(null);
			map.Disk_OverwriteOrWriteAsNew(AssetAuthorType.UGC);
		}
		onComplete?.Invoke(obj: true);
	}
}
