using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface ILayoutPanel : IRefreshAll, IPanel, IRefresh, IInstance, ILuaId, IHierarchyName
{
	ILayoutPanel AddPanel(IPanel panel);

	ILayoutPanel AddPanels(PanelList panels);

	ILayoutPanel RemovePanel2(IPanel panel);

	ILayoutPanel RemovePanelsNoLeak(PanelList panels);

	ILayoutPanel RemoveAllPanels();

	List<IPanel> GetPanels();

	RectTransform GetLayoutTrsForChild(IPanel child);

	RectTransform GetLayoutTrs();

	IMenu GetMenu();

	ILayoutPanel SetMenu(IMenu menu);

	ILayoutPanel ClearMenu();
}
public interface ILayoutPanel<TSocket> : IRefreshAll<TSocket>, ILayoutPanel, IRefreshAll, IRefresh, IPanel, IInstance, ILuaId, IHierarchyName where TSocket : struct
{
	Func<ILayoutPanel<TSocket>, RectTransform> LayoutSocketTrs { get; }

	ILayoutPanel<TSocket> AddPanelAtSocket(TSocket socket, IPanel panel);

	ILayoutPanel<TSocket> AddPanelsAtSocketNoLeak(TSocket socket, PanelList panels);

	ILayoutPanel<TSocket> RemovePanelAtSocket(TSocket socket, IPanel panel);

	ILayoutPanel<TSocket> RemovePanelsAtSocketNoLeak(TSocket socket, PanelList panels);

	ILayoutPanel<TSocket> RemoveAllPanelsAtSocket(TSocket socket);

	List<IPanel> GetPanelsAtSocket(TSocket socket);

	TSocket GetSocketForPanel(IPanel panel);

	RectTransform GetTrsAtSocket(TSocket socket);
}
