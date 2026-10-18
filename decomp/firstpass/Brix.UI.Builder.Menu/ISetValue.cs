namespace Brix.UI.Builder.Menu;

public interface ISetValue<in T> : IWidgetHandle
{
	void SetValue(T value);
}
