namespace Brix.UI.Builder.Menu;

public interface IFromTextValue<out T> : IWidgetHandle
{
	T FromTextValue(string textValue);
}
