namespace Brix.NewUI.GameModes;

public class StringSymbol : ContentSymbol
{
	public string value;

	public StringSymbol(Key key, string text)
		: base(key)
	{
		value = text;
	}
}
