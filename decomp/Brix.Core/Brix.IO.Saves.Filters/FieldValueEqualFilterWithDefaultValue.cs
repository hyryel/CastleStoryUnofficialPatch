using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class FieldValueEqualFilterWithDefaultValue<T> : IJObjectFilter
{
	private string _fieldname;

	private T _wantedValue;

	private T _defaultValue;

	public FieldValueEqualFilterWithDefaultValue(string fieldname, T wantedValue, T defaultValue)
	{
		_fieldname = fieldname;
		_wantedValue = wantedValue;
		_defaultValue = defaultValue;
	}

	public bool Matches(JContainer jobject)
	{
		return ((jobject[_fieldname] == null) ? _defaultValue : jobject[_fieldname].Value<T>()).Equals(_wantedValue);
	}
}
