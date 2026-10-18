namespace Brix.UI.Builder.Menu;

public interface IOnValidateInput : IWidgetHandle
{
	char OnValidateInput(string value, int index, char addedChar);
}
