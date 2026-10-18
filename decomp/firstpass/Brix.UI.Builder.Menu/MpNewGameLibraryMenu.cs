using System;
using System.Diagnostics;
using Brix.Assets;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class MpNewGameLibraryMenu : BaseNewGameLibraryMenu
{
	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Multiplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Multi;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.New;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	private MpNewGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static MpNewGameLibraryMenu Build(Func<RectTransform> binGetter)
	{
		return new MpNewGameLibraryMenu(binGetter);
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		base.ExecuteMenuOperation(assetQueryParameters);
		if (SelectedAsset == BaseNewGameLibraryMenu.SteamAddMap)
		{
			SelectedAsset = null;
			Process.Start("steam://url/SteamWorkshopPage/227860");
		}
		else
		{
			MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Confirm);
		}
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, GameParam.OnSetMapSignal, BaseMenu.OnRefresh, this).AddSignalConnection(MenuState.Opened, GameParam.OnSetPresetSignal, BaseMenu.OnRefresh, this);
	}
}
