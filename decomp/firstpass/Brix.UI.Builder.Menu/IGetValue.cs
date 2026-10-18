namespace Brix.UI.Builder.Menu;

public interface IGetValue<out T> : IWidgetHandle
{
	T GetValue();
}
