using System;
using System.Collections.Generic;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public abstract class BaseLayoutPanel<TPanelComponent> : BasePanel<TPanelComponent>, ILayoutPanel, IRefreshAll, IPanel, IRefresh, IInstance, ILuaId, IHierarchyName where TPanelComponent : BasePanelComponent
{
	protected readonly List<BasePanel> _panels_2 = new List<BasePanel>();

	private IMenu _menu;

	private List<IPanel> _panels = new List<IPanel>();

	protected BaseLayoutPanel()
	{
	}

	protected BaseLayoutPanel(Action<TPanelComponent> onReset, Action<TPanelComponent> onRefresh, bool luaDriven = false)
		: base(onReset, onRefresh, luaDriven)
	{
	}

	public List<BasePanel> GetPanelsOld()
	{
		return _panels_2;
	}

	public BaseLayoutPanel<TPanelComponent> AddPanels_2(params BasePanel[] panels)
	{
		_panels_2.AddRange(panels);
		return this;
	}

	public virtual RectTransform GetSocket_2()
	{
		return GetLayoutTrs();
	}

	public IMenu GetMenu()
	{
		return _menu;
	}

	public ILayoutPanel SetMenu(IMenu menu)
	{
		_menu = menu;
		_menu.GetParent = GetLayoutTrs;
		return this;
	}

	public ILayoutPanel ClearMenu()
	{
		if (_menu != null)
		{
			_menu.GetParent = null;
		}
		_menu = null;
		return this;
	}

	public BaseLayoutPanel<TPanelComponent> SetMenu<TMenu>(out TMenu menu) where TMenu : BaseMenu
	{
		menu = BaseMenu.CreateInstance<TMenu>(GetLayoutTrs);
		return this;
	}

	public ILayoutPanel AddPanel(IPanel panel)
	{
		_panels.Add(panel);
		panel.ParentPanel = this;
		return this;
	}

	public ILayoutPanel AddPanels(PanelList panels)
	{
		foreach (IPanel panel in panels)
		{
			AddPanel(panel);
		}
		PanelList.Release(panels);
		return this;
	}

	public ILayoutPanel AddPanels_NoRelease(PanelList panels)
	{
		foreach (IPanel panel in panels)
		{
			AddPanel(panel);
		}
		return this;
	}

	public ILayoutPanel RemovePanel2(IPanel panel)
	{
		_panels.Remove(panel);
		panel.ParentPanel = null;
		return this;
	}

	public ILayoutPanel RemovePanelsNoLeak(PanelList panels)
	{
		foreach (IPanel panel in panels)
		{
			RemovePanel2(panel);
		}
		PanelList.Release(panels);
		return this;
	}

	public ILayoutPanel RemoveAllPanels()
	{
		for (int num = _panels.Count - 1; num >= 0; num--)
		{
			RemovePanel2(_panels[num]);
		}
		return this;
	}

	public List<IPanel> GetPanels()
	{
		return _panels;
	}

	public virtual RectTransform GetLayoutTrsForChild(IPanel child)
	{
		return GetLayoutTrs();
	}

	public abstract RectTransform GetLayoutTrs();

	public void ResetAll()
	{
		foreach (IPanel panel in _panels)
		{
			panel.Reset();
		}
	}

	public bool RefreshAll()
	{
		foreach (IPanel panel in _panels)
		{
			if (!panel.Refresh())
			{
				return false;
			}
		}
		return true;
	}
}
