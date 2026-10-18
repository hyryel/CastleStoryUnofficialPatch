using System;
using Brix.Assets;
using Brix.External.Signals;
using Brix.Game.Compounders;
using Brix.Steam;
using Steamworks;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class BlueprintWorkshopPublishMenu : BaseWorkshopPublishMenu
{
	private static ERemoteStoragePublishedFileVisibility _visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;

	public static Asset_Blueprints Blueprint => BlueprintCompounder.GetCurrentSelected();

	public override bool IsAssetNull => IsBlueprintNull;

	public static bool IsBlueprintNull => BlueprintCompounder.GetCurrentSelected() == null;

	public override WorkshopItem AssetItem => BlueprintItem;

	public static WorkshopItem BlueprintItem => (!IsBlueprintNull) ? Blueprint.GetWorkshopItem() : null;

	protected override ulong AssetItemId => BlueprintItemId;

	public static ulong BlueprintItemId => (!IsBlueprintNull) ? Blueprint.workshopID : 0;

	protected override bool CanPublishAsset => CanPublishBlueprint;

	public static bool CanPublishBlueprint => !IsBlueprintNull && WeSaveActionsMenu.IsDisplayNameValid;

	public static ERemoteStoragePublishedFileVisibility Visibility
	{
		get
		{
			return BlueprintItem?.visibility ?? _visibility;
		}
		set
		{
			WorkshopItem blueprintItem = BlueprintItem;
			if (blueprintItem != null)
			{
				blueprintItem.visibility = value;
			}
			_visibility = value;
		}
	}

	public static Signal RefreshSignal => BaseWorkshopPublishMenu.BaseRefreshSignal;

	public BlueprintWorkshopPublishMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static BlueprintWorkshopPublishMenu Build(Func<RectTransform> binGetter)
	{
		return new BlueprintWorkshopPublishMenu(binGetter);
	}

	public override void PublishAsset()
	{
		PublishBlueprint();
	}

	public static void PublishBlueprint()
	{
		BlueprintWorkshopReleaseNotesModalMenu.OpenSignal.Invoke();
	}

	public static void SendBlueprintToWorkshop()
	{
		if (!IsBlueprintNull)
		{
			Blueprint.SendToWorkshop(BlueprintCompounder.GetCurrentSelectedName(), string.Empty, string.Empty, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
		}
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, GameParam.OnSetMapSignal, BaseMenu.OnReopen, this).AddSignalConnection(MenuState.Opened, WeSaveActionsMenu.AfterMapInfoChangeSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, WeSaveActionsMenu.AfterMapOptionsChangeSignal, BaseMenu.OnRefresh, this);
	}
}
