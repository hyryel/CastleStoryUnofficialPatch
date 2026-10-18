using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class FieldValueEqualFilter<T> : IJObjectFilter
{
	private string _fieldname;

	private T _wantedValue;

	public FieldValueEqualFilter(string fieldname, T wantedValue)
	{
		_fieldname = fieldname;
		_wantedValue = wantedValue;
	}

	public bool Matches(JContainer container)
	{
		return container[_fieldname] != null && container[_fieldname].Value<T>().Equals(_wantedValue);
	}
}
