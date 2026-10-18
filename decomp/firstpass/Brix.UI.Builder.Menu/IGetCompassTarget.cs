using Brix.Lua;

namespace Brix.UI.Builder.Menu;

public interface IGetCompassTarget : IWidgetHandle
{
	LuaDriver GetCompassTarget();
}
