using System;
using Brix.UI.Builder.Panel;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseModalMenu : BaseMenu
{
	protected abstract Action<ModalPanelComponent> OnResetModal { get; }

	protected BaseModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel(GetModalPanel());
	}

	private IPanel GetModalPanel()
	{
		return ModalPanel.New(OnResetModal).AddPanelsAtSocketNoLeak(ModalPanel.Socket.Header, GetHeaderBuilders()).AddPanelsAtSocketNoLeak(ModalPanel.Socket.Content, GetContentBuilders())
			.AddPanelsAtSocketNoLeak(ModalPanel.Socket.Footer, GetFooterBuilders());
	}

	protected virtual PanelList GetHeaderBuilders()
	{
		return PanelList.Request();
	}

	protected virtual PanelList GetContentBuilders()
	{
		return PanelList.Request();
	}

	protected virtual PanelList GetFooterBuilders()
	{
		return PanelList.Request();
	}
}
