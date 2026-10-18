namespace Brix.NewUI.GameModes;

public class Symbol
{
	public enum Key
	{
		_None,
		Title,
		Title2,
		Title3,
		Title4,
		Title5,
		Image,
		Image2,
		Image3,
		Image4,
		Image5,
		OnNew,
		OnLoad,
		OnSave,
		OnRename,
		OnChange,
		OnNavigate
	}

	public Key key;

	public Symbol(Key key)
	{
		this.key = key;
	}
}
