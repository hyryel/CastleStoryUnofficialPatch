using System;
using Brix.Assets;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class WorldEditorStarterMenu : BaseMapStarterMenu
{
	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	public static readonly Signal RefreshSignal = new Signal();

	protected override bool LoadOnInit => true;

	protected override bool ReloadOnOpen => false;

	protected override bool LoadOnOpen => false;

	protected override bool AsModal => false;

	protected override bool IsReskin => true;

	protected override bool ThumbnailShowsBg => true;

	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.WorldEditor;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Both;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.New;

	protected override MenuGameContext GameContext => MenuGameContext.WorldEditor;

	public WorldEditorStarterMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static WorldEditorStarterMenu New(Func<RectTransform> binGetter)
	{
		return new WorldEditorStarterMenu(binGetter);
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Loaded, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
