namespace Brix.UI.Builder.Menu;

public interface IToTextValue<in T> : IWidgetHandle
{
	string ToTextValue(T value);
}
