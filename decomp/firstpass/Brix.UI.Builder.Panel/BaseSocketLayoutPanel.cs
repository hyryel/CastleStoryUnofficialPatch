using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public abstract class BaseSocketLayoutPanel<TPanelComponent, TSocket> : BaseLayoutPanel<TPanelComponent>, ILayoutPanel<TSocket>, IRefreshAll<TSocket>, ILayoutPanel, IRefreshAll, IRefresh, IPanel, IInstance, ILuaId, IHierarchyName where TPanelComponent : BaseSocketLayoutPanelComponent where TSocket : struct
{
	private static readonly Func<ILayoutPanel<TSocket>, RectTransform> LayoutSocketTrsFunc = (ILayoutPanel<TSocket> panel) => panel.GetTrsAtSocket(default(TSocket));

	private TSocket[] _sockets;

	protected readonly Dictionary<TSocket, List<IPanel>> SocketChildren = new Dictionary<TSocket, List<IPanel>>(FastEnumIntEqualityComparer<TSocket>.Instance);

	private readonly Dictionary<TSocket, List<BasePanel>> _socketPanelsMap2 = new Dictionary<TSocket, List<BasePanel>>(FastEnumIntEqualityComparer<TSocket>.Instance);

	public Func<ILayoutPanel<TSocket>, RectTransform> LayoutSocketTrs => LayoutSocketTrsFunc;

	public TSocket[] Sockets => _sockets;

	protected BaseSocketLayoutPanel(Action<TPanelComponent> onReset, Action<TPanelComponent> onRefresh, bool luaDriven = false)
		: base(onReset, onRefresh, luaDriven)
	{
		InitSocketPanels();
		InitSocketPanels_2();
	}

	public new ILayoutPanel<TSocket> AddPanel(IPanel panel)
	{
		return AddPanelAtSocket(default(TSocket), panel);
	}

	public ILayoutPanel<TSocket> RemoveAllPanelsAtSocket(TSocket socket)
	{
		foreach (IPanel item in SocketChildren[socket])
		{
			RemovePanelAtSocket(socket, item);
		}
		return this;
	}

	public ILayoutPanel<TSocket> AddPanelAtSocket(TSocket socket, IPanel panel)
	{
		base.AddPanel(panel);
		SocketChildren[socket].Add(panel);
		return this;
	}

	public ILayoutPanel<TSocket> AddPanelsAtSocket(TSocket socket, IEnumerable<IPanel> panels)
	{
		foreach (IPanel item in panels.Slinq().NoLeak())
		{
			AddPanelAtSocket(socket, item);
		}
		return this;
	}

	public ILayoutPanel<TSocket> AddPanelAtSocketNoLeak(TSocket socket, IPanel panel)
	{
		AddPanelAtSocket(socket, panel);
		return this;
	}

	public ILayoutPanel<TSocket> AddPanelsAtSocketNoLeak(TSocket socket, PanelList panels)
	{
		foreach (IPanel panel in panels)
		{
			AddPanelAtSocket(socket, panel);
		}
		PanelList.Release(panels);
		return this;
	}

	public ILayoutPanel<TSocket> RemovePanelAtSocket(TSocket socket, IPanel panel)
	{
		if (!SocketChildren[socket].Contains(panel))
		{
			Logger.WarnFormat("Child not found in socket.\n socket == {0}, iLayoutElement == {1}", socket, panel);
			return this;
		}
		SocketChildren[socket].Remove(panel);
		RemovePanel2(panel);
		return this;
	}

	public ILayoutPanel<TSocket> RemovePanelsAtSocketNoLeak(TSocket socket, PanelList panels)
	{
		foreach (IPanel panel in panels)
		{
			RemovePanelAtSocket(GetSocketForPanel(panel), panel);
		}
		PanelList.Release(panels);
		return this;
	}

	public List<IPanel> GetPanelsAtSocket(TSocket socket)
	{
		return SocketChildren[socket];
	}

	public TSocket GetSocketForPanel(IPanel panel)
	{
		TSocket[] sockets = _sockets;
		foreach (TSocket val in sockets)
		{
			if (SocketChildren[val].Contains(panel))
			{
				return val;
			}
		}
		throw BaseSocketLayoutPanelComponent.TmpChildNotFoundException;
	}

	public override RectTransform GetLayoutTrsForChild(IPanel child)
	{
		return GetTrsAtSocket(GetSocketForPanel(child));
	}

	public abstract RectTransform GetTrsAtSocket(TSocket socket);

	public override RectTransform GetLayoutTrs()
	{
		return GetTrsAtSocket(default(TSocket));
	}

	public void ResetAtSocket(TSocket socket)
	{
		foreach (IPanel item in SocketChildren[socket])
		{
			item.Reset();
		}
	}

	public void RefreshAtSocket(TSocket socket)
	{
		foreach (IPanel item in SocketChildren[socket])
		{
			item.Refresh();
		}
	}

	public BaseSocketLayoutPanel<TPanelComponent, TSocket> SetMenuAtSocket<TMenu>(out TMenu menu, TSocket socket) where TMenu : BaseMenu
	{
		menu = BaseMenu.CreateInstance<TMenu>(() => GetTrsAtSocket(socket));
		return this;
	}

	public new BaseSocketLayoutPanel<TPanelComponent, TSocket> SetMenu<TMenu>(out TMenu menu) where TMenu : BaseMenu
	{
		return SetMenuAtSocket<TMenu>(out menu, default(TSocket));
	}

	private void InitSocketPanels()
	{
		_sockets = (TSocket[])Enum.GetValues(typeof(TSocket));
		TSocket[] sockets = _sockets;
		foreach (TSocket key in sockets)
		{
			SocketChildren.Add(key, new List<IPanel>());
		}
	}

	public List<BasePanel> GetChildrenInSocket_2(TSocket socket)
	{
		return _socketPanelsMap2[socket];
	}

	private void InitSocketPanels_2()
	{
		_sockets = (TSocket[])Enum.GetValues(typeof(TSocket));
		TSocket[] sockets = _sockets;
		foreach (TSocket key in sockets)
		{
			_socketPanelsMap2.Add(key, new List<BasePanel>());
		}
	}

	public override RectTransform GetSocket_2()
	{
		return GetTrsAtSocket(default(TSocket));
	}
}
