using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Menu;

public interface IGetOnEndShowing : IWidgetHandle
{
	Closure GetOnEndShowing();
}
