namespace Brix.UI.Builder.Menu;

public interface IGetMinValue<out T> : IWidgetHandle
{
	T GetMinValue();
}
