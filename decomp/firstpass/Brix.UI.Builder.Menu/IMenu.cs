using System;
using System.Collections.Generic;
using Brix.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IMenu : ILuaId, IHierarchyName
{
	string LuaPath { get; }

	Table LuaTable { get; set; }

	Closure LuaMenuTableReader { get; set; }

	LuaHook OnLoadedHook { get; set; }

	bool Loaded { get; }

	bool Opened { get; }

	bool Closed { get; }

	bool CanOpen { get; }

	bool CanClose { get; }

	bool CanToggle { get; }

	Func<RectTransform> GetParent { get; set; }

	void RemoveAllPanels();

	void RemoveAllMenus();

	List<IPanel> GetPanels();

	void AddRootLuaPanel(Table panel);

	void AddLuaPanel(Table panel);

	void AddLuaMenu(IMenu menu);

	void AddLuaConnection(BaseMenu.HookConnection hookConnection);

	void RemoveAllLuaPanels();

	void RemoveAllLuaMenus();

	void RemoveAllLuaConnections();

	void AddHookConnection(BaseMenu.MenuState a, LuaHookBase h, Closure c);

	void RemoveAllHookConnections();

	void Connect(BaseMenu.MenuState e, bool includeChildren);

	void ConnectSignals(BaseMenu.MenuState e, bool includeChildren);

	void ConnectHooks(BaseMenu.MenuState e, bool includeChildren);

	void ConnectAtSubMenus(BaseMenu.MenuState e, bool includeChildren);

	void ConnectSignalsAtSubMenus(BaseMenu.MenuState e, bool includeChildren);

	void ConnectHooksAtSubMenus(BaseMenu.MenuState e, bool includeChildren);

	void Disconnect(BaseMenu.MenuState e, bool includeChildren);

	void DisconnectSignals(BaseMenu.MenuState e, bool includeChildren);

	void DisconnectHooks(BaseMenu.MenuState e, bool includeChildren);

	void DisconnectAtSubMenus(BaseMenu.MenuState e, bool includeChildren);

	void DisconnectSignalsAtSubMenus(BaseMenu.MenuState e, bool includeChildren);

	void DisconnectHooksAtSubMenus(BaseMenu.MenuState e, bool includeChildren);

	void DisconnectAll(bool includeChildren);

	void DisconnectAllSignals(bool includeChildren);

	void DisconnectAllHooks(bool includeChildren);

	void DisconnectAllAtSubMenus(bool includeChildren);

	void DisconnectAllSignalsAtSubMenus(bool includeChildren);

	void DisconnectAllHooksAtSubMenus(bool includeChildren);

	bool GetToggleSkipFlag();

	bool HasLuaBuilderFlag(BaseMenu.BuilderFlag builderFlag);

	void SetLuaBuilderFlag(BaseMenu.BuilderFlag builderFlag, Closure c);

	void ClearLuaBuilderFlag(BaseMenu.BuilderFlag builderFlag);

	void ClearAllLuaBuilderFlags();

	void Toggle();

	void Close(bool includeChildren = true);

	void Open();

	void Reopen();

	void Reset();

	void Refresh();

	void Init(bool includeChildren = true);

	void Dispose();

	void Reload(bool includeChildren = true, bool canOpen = true);

	void Load(bool includeChildren = true, bool canOpen = true);

	void Unload(bool includeChildren = true);

	RectTransform GetRootTrs();
}
public interface IMenu<in TSocket> : IMenu, ILuaId, IHierarchyName where TSocket : struct
{
	void Close(TSocket socket);

	void Open(TSocket socket);

	void ReOpen(TSocket socket);

	bool Refresh(TSocket socket);
}
