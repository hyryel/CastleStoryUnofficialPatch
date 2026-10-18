using System;
using Brix.Assets;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class SpNewGameStarterMenu : BaseMapStarterMenu
{
	private static readonly Action<SpNewGameStarterMenu> OnSetSelectedMap = delegate(SpNewGameStarterMenu menu)
	{
		BaseMapLibraryMenu.SetSelectedGamemode(BaseMapLibraryMenu.GetSelectedMap(), LegacyPlayGamemodeMenu.SelectedOperationGamemode, menu.PlayableContext);
	};

	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	private static readonly Signal RefreshSignal = new Signal();

	protected override bool LoadOnInit => true;

	protected override bool ReloadOnOpen => false;

	protected override bool LoadOnOpen => false;

	protected override bool AsModal => false;

	protected override bool IsReskin => true;

	protected override bool ThumbnailIsHeightmap => true;

	protected override bool ThumbnailShowsBg => true;

	protected override bool UseGamemodeFilter => true;

	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Singleplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Single;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.New;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	public SpNewGameStarterMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static SpNewGameStarterMenu New(Func<RectTransform> binGetter)
	{
		return new SpNewGameStarterMenu(binGetter);
	}

	protected override void OpenSelf()
	{
		base.OpenSelf();
		BaseMapLibraryMenu.SetSelectedGamemode(BaseMapLibraryMenu.GetSelectedMap(), LegacyPlayGamemodeMenu.SelectedOperationGamemode, PlayableContext);
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Opened, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, BaseMapLibraryMenu.OnSetSelectedMapSignal, OnSetSelectedMap, this);
	}
}
