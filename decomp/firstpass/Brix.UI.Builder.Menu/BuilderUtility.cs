using System.Collections.Generic;
using Brix.Engine.Lifecycle;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public static class BuilderUtility
{
	private static bool _isDebug = false;

	public static BrixLogger logger = new BrixLogger(typeof(BuilderUtility).FullName);

	public static RectTransform _rootTrs;

	public static bool _isUnparent;

	public static bool _skipIfInstanced;

	public static List<string> _logLines;

	public static bool QueueRefreshOptimization = false;

	public static bool QueueTreeRefreshOptimization = false;

	private static HashSet<IPanel> PanelsToRefresh => SingletonProvider.Get<IPanel, HashSet<IPanel>>();

	private static HashSet<IMenu> MenusToRefresh => SingletonProvider.Get<IMenu, HashSet<IMenu>>();

	public static void PrintTree(IMenu menu, List<IMenu> excludedMenus = null)
	{
		_logLines = new List<string>();
		foreach (IPanel panel in menu.GetPanels())
		{
			PrintTreeRecursive(panel, excludedMenus);
		}
	}

	private static void PrintTreeRecursive(IPanel child, List<IMenu> excludedMenus)
	{
		if (excludedMenus != null && excludedMenus.Contains(child.ParentMenu))
		{
			return;
		}
		Print(child);
		if (!(child is ILayoutPanel))
		{
			return;
		}
		ILayoutPanel layoutPanel = child as ILayoutPanel;
		foreach (IPanel panel in layoutPanel.GetPanels())
		{
			PrintTreeRecursive(panel, excludedMenus);
		}
	}

	private static void Print(IPanel child)
	{
		_logLines.Add(child.ToString());
	}

	public static void AssignTree(IMenu menu)
	{
		AssignTree(menu, menu);
	}

	public static void UnassignTree(IMenu menu)
	{
		AssignTree(menu, null);
	}

	private static void AssignTree(IMenu menu, IMenu valueMenu)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(menu.GetType(), "\nAssignTree()"));
		}
		foreach (IPanel panel in menu.GetPanels())
		{
			AssignTreeRecursive(menu, panel, valueMenu);
		}
	}

	private static void AssignTreeRecursive(IMenu menu, IPanel child, IMenu menuValue)
	{
		if (!AssignIf(menu, child))
		{
			return;
		}
		Assign(child, menuValue);
		if (!(child is ILayoutPanel))
		{
			return;
		}
		ILayoutPanel layoutPanel = child as ILayoutPanel;
		foreach (IPanel panel in layoutPanel.GetPanels())
		{
			AssignTreeRecursive(menu, panel, menuValue);
		}
	}

	private static bool AssignIf(IMenu menu, IPanel child)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(menu.GetType(), "\nAssignTree?(", child.ParentMenu == null || child.ParentMenu == menu, ": ", child.ParentMenu == null, "/", child.ParentMenu == menu, ")"));
		}
		if (child.ParentMenu == null)
		{
			return true;
		}
		if (child.ParentMenu == menu)
		{
			return true;
		}
		return false;
	}

	private static void Assign(IPanel child, IMenu valueMenu)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(child, "\nAssign(", valueMenu, ")"));
		}
		child.ParentMenu = valueMenu;
	}

	public static void InstantiateTree(IMenu menu, bool skipIfInstanced = false, List<IMenu> excludedMenus = null)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(menu.GetType(), "\nInstantiateTree()"));
		}
		foreach (IPanel panel in menu.GetPanels())
		{
			InstantiateTreeRecursive(panel, skipIfInstanced, excludedMenus);
		}
	}

	private static void InstantiateTreeRecursive(IPanel child, bool skipIfInstanced, List<IMenu> excludedMenus)
	{
		Instantiate(child, skipIfInstanced);
		if (!(child is ILayoutPanel))
		{
			return;
		}
		ILayoutPanel layoutPanel = child as ILayoutPanel;
		foreach (IPanel panel in layoutPanel.GetPanels())
		{
			InstantiateTreeRecursive(panel, skipIfInstanced, excludedMenus);
		}
	}

	private static void Instantiate(IPanel child, bool skipIfInstanced)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(child, "\nInstantiate()"));
		}
		if (!skipIfInstanced || !child.HasInstance)
		{
			child.Request(_skipIfInstanced);
		}
	}

	public static void KillTree(IMenu menu, List<IMenu> excludedMenus = null)
	{
		foreach (IPanel panel in menu.GetPanels())
		{
			KillTreeRecursive(panel, excludedMenus);
		}
	}

	private static void KillTreeRecursive(IPanel child, List<IMenu> excludedMenus)
	{
		if (child is ILayoutPanel)
		{
			ILayoutPanel layoutPanel = child as ILayoutPanel;
			foreach (IPanel panel in layoutPanel.GetPanels())
			{
				KillTreeRecursive(panel, excludedMenus);
			}
		}
		Kill(child);
	}

	private static void Kill(IPanel child)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(child, "\nKill()"));
		}
		child.Release();
	}

	public static void ParentTree(IMenu menu, List<IMenu> excludedMenus = null)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(menu.GetType(), "\nParentTree()"));
		}
		foreach (IPanel panel in menu.GetPanels())
		{
			ParentRecursive(panel, null, excludedMenus);
		}
	}

	private static void ParentRecursive(IPanel child, ILayoutPanel parent, List<IMenu> excludedMenus)
	{
		if (excludedMenus != null && excludedMenus.Contains(child.ParentMenu))
		{
			return;
		}
		Parent(child, parent);
		if (!(child is ILayoutPanel))
		{
			return;
		}
		ILayoutPanel layoutPanel = child as ILayoutPanel;
		foreach (IPanel panel in layoutPanel.GetPanels())
		{
			ParentRecursive(panel, layoutPanel, excludedMenus);
		}
	}

	private static void Parent(IPanel child, ILayoutPanel parent)
	{
		RectTransform rectTransform = ((parent != null) ? parent.GetLayoutTrsForChild(child) : child.ParentMenu.GetRootTrs());
		if (_isDebug)
		{
			logger.Info(string.Concat(child, "\nParentTree(", parent, ", ", (parent != null) ? string.Concat(parent.GetLayoutTrsForChild(child), string.Empty) : string.Concat(child.ParentMenu.GetRootTrs(), string.Empty), ", ", rectTransform, ")"));
		}
		child.ParentToTrs(rectTransform);
	}

	public static void UnparentTree(IMenu menu, List<IMenu> excludedMenus = null)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(menu.GetType(), "\nParentTree()"));
		}
		foreach (IPanel panel in menu.GetPanels())
		{
			UnparentRecursive(panel, excludedMenus);
		}
	}

	private static void UnparentRecursive(IPanel child, List<IMenu> excludedMenus)
	{
		if ((excludedMenus != null && excludedMenus.Contains(child.ParentMenu)) || !(child is ILayoutPanel))
		{
			return;
		}
		ILayoutPanel layoutPanel = child as ILayoutPanel;
		foreach (IPanel panel in layoutPanel.GetPanels())
		{
			UnparentRecursive(panel, excludedMenus);
		}
		Unparent(child);
	}

	private static void Unparent(IPanel child)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(child, "\nParentTree()"));
		}
		child.ParentToTrs(null);
	}

	public static void ResetTree(IMenu menu, bool all = true, List<IMenu> excludedMenus = null)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(menu.GetType(), "\nResetTree()"));
		}
		foreach (IPanel panel in menu.GetPanels())
		{
			ResetTreeRecursive(panel, all, excludedMenus);
		}
	}

	private static void ResetTreeRecursive(IPanel child, bool all, List<IMenu> excludedMenus)
	{
		Reset(child);
		if (!all || !(child is ILayoutPanel))
		{
			return;
		}
		ILayoutPanel layoutPanel = child as ILayoutPanel;
		foreach (IPanel panel in layoutPanel.GetPanels())
		{
			ResetTreeRecursive(panel, all: true, excludedMenus);
		}
	}

	private static void Reset(IRefresh child)
	{
		child.Reset();
	}

	private static void ProcessQueuedPanelRefreshes()
	{
		foreach (IPanel item in PanelsToRefresh)
		{
			item.Refresh();
		}
		PanelsToRefresh.Clear();
	}

	private static void ProcessQueuedMenuRefreshes()
	{
		foreach (IMenu item in MenusToRefresh)
		{
			RefreshTree(item);
		}
		MenusToRefresh.Clear();
	}

	public static void QueueRefresh(this IPanel panel)
	{
		if (panel.HasInstance)
		{
			panel.Refresh();
		}
	}

	public static void QueueRefreshTree(IMenu menu)
	{
		RefreshTree(menu);
	}

	public static void RefreshTree(IMenu menu, bool all = true)
	{
		if (_isDebug)
		{
			logger.Info(string.Concat(menu.GetType(), "\nRefreshTree()"));
		}
		foreach (IPanel panel in menu.GetPanels())
		{
			RefreshTreeRecursive(panel, all);
		}
	}

	private static void RefreshTreeRecursive(IPanel child, bool all)
	{
		if (!Refresh(child) || !all || !(child is ILayoutPanel))
		{
			return;
		}
		ILayoutPanel layoutPanel = child as ILayoutPanel;
		foreach (IPanel panel in layoutPanel.GetPanels())
		{
			RefreshTreeRecursive(panel, all: true);
		}
	}

	private static bool Refresh(IRefresh child)
	{
		return child.Refresh();
	}
}
