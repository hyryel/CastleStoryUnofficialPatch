using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Menu;

public interface IGetOutlines : IWidgetHandle
{
	DynValue GetOutlines();
}
