using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Menu;

public interface IGetOptions : IWidgetHandle
{
	DynValue GetOptions();
}
