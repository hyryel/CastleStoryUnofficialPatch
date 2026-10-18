using Brix.UI.Builder.Widget;

namespace Brix.UI.Builder.Fields;

public class LuiField<T>
{
	private readonly LuiWidget _luiWidget;

	private bool _isFirstRun = true;

	private T _value;

	public T Value
	{
		get
		{
			return _value;
		}
		set
		{
			Set(value);
		}
	}

	public LuiField(LuiWidget luiWidget)
	{
		_luiWidget = luiWidget;
	}

	private void Set(T value)
	{
		if (_isFirstRun || !value.Equals(_value))
		{
			_value = value;
			_isFirstRun = false;
			_luiWidget.SetDirty();
		}
	}
}
