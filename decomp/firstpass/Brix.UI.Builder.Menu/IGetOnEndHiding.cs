using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Menu;

public interface IGetOnEndHiding : IWidgetHandle
{
	Closure GetOnEndHiding();
}
