namespace Brix.UI.Builder.Menu;

public interface IToValidValue<T> : IWidgetHandle
{
	T ToValidValue(T value);
}
