using Brix.UI.Builder.Menu;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Widget;

public abstract class BaseWidget : IWidget, ILuaId, IHierarchyName
{
	public string Name => "Widget";

	public string HierarchyName => Name;

	public string FullName => HierarchyName;

	public string SuffixName { get; set; }

	public long LuaId { get; set; }

	public virtual bool Is<TIWidgetHandle>() where TIWidgetHandle : IWidgetHandle
	{
		return this is TIWidgetHandle;
	}

	public virtual void Lua_SetHandle(string handleKey, Closure closure, DynValue dOnce)
	{
	}

	public virtual void Lua_ClearHandle(string handleKey)
	{
	}

	public virtual void Lua_ClearHandles(string[] handleKeys)
	{
	}

	public virtual void Lua_ClearAllHandles()
	{
	}

	public virtual void Lua_Init()
	{
	}
}
