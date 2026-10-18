using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class MpNewGameStarterMenu : BaseMpGameStarterMenu
{
	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	public static Signal RefreshSignal = new Signal();

	protected override MenuTypeContext TypeContext => MenuTypeContext.New;

	public MpNewGameStarterMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static MpNewGameStarterMenu New(Func<RectTransform> binGetter)
	{
		return new MpNewGameStarterMenu(binGetter);
	}

	protected override void LoadSelf()
	{
		base.LoadSelf();
		OnAfterSetMap();
	}

	protected override void OnAfterSetMap()
	{
		if (base.DisplayedMap != null && !base.DisplayedMap.IsNewGame())
		{
			Close();
		}
		else if (base.Opened)
		{
			Refresh();
		}
		else
		{
			Open();
		}
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Opened, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
