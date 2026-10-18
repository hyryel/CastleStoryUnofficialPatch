using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Menu;

public interface IWidget : ILuaId, IHierarchyName
{
	bool Is<TIWidgetHandle>() where TIWidgetHandle : IWidgetHandle;

	void Lua_SetHandle(string handleKey, Closure closure, DynValue dOnce);

	void Lua_ClearHandle(string handleKey);

	void Lua_ClearHandles(string[] handleKeys);

	void Lua_ClearAllHandles();

	void Lua_Init();
}
