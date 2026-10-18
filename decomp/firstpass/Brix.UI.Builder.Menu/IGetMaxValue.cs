namespace Brix.UI.Builder.Menu;

public interface IGetMaxValue<out T> : IWidgetHandle
{
	T GetMaxValue();
}
