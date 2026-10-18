using System;
using Brix.Assets;
using Brix.External.Signals;
using Brix.Game.Compounders;
using Brix.Steam;
using Steamworks;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class MapWorkshopPublishMenu : BaseWorkshopPublishMenu
{
	private static ERemoteStoragePublishedFileVisibility _visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;

	public static Asset_Map Map => GameParam.Map;

	public static Asset_Blueprints Blueprint => BlueprintCompounder.GetCurrentSelected();

	public override bool IsAssetNull => IsMapNull;

	public static bool IsMapNull => Map == null;

	public override WorkshopItem AssetItem => MapItem;

	public static WorkshopItem MapItem => (!IsMapNull) ? Map.GetWorkshopItem() : null;

	protected override ulong AssetItemId => MapItemId;

	public static ulong MapItemId => (!IsMapNull) ? Map.workshopID : 0;

	protected override bool CanPublishAsset => CanPublishMap;

	public static bool CanPublishMap => !IsMapNull && WeSaveActionsMenu.IsDisplayNameValid;

	public static ERemoteStoragePublishedFileVisibility Visibility
	{
		get
		{
			return MapItem?.visibility ?? _visibility;
		}
		set
		{
			WorkshopItem mapItem = MapItem;
			if (mapItem != null)
			{
				mapItem.visibility = value;
			}
			_visibility = value;
		}
	}

	public static Signal RefreshSignal => BaseWorkshopPublishMenu.BaseRefreshSignal;

	public MapWorkshopPublishMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static MapWorkshopPublishMenu Build(Func<RectTransform> binGetter)
	{
		return new MapWorkshopPublishMenu(binGetter);
	}

	public override void PublishAsset()
	{
		PublishMap();
	}

	public static void PublishMap()
	{
		MapWorkshopReleaseNotesModalMenu.OpenSignal.Invoke();
	}

	public static void SendMapToWorkshop()
	{
		if (!IsMapNull)
		{
			Map.SendToWorkshop(WeSaveActionsMenu.DisplayName, WeSaveActionsMenu.Description, MapWorkshopReleaseNotesModalMenu.MapReleaseNote, Visibility);
		}
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, GameParam.OnSetMapSignal, BaseMenu.OnReopen, this).AddSignalConnection(MenuState.Opened, WeSaveActionsMenu.AfterMapInfoChangeSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, WeSaveActionsMenu.AfterMapOptionsChangeSignal, BaseMenu.OnRefresh, this);
	}
}
