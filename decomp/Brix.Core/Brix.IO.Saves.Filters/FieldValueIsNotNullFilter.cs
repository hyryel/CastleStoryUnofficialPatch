using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class FieldValueIsNotNullFilter : IJObjectFilter
{
	private string _fieldname;

	public FieldValueIsNotNullFilter(string fieldname)
	{
		_fieldname = fieldname;
	}

	public bool Matches(JContainer container)
	{
		JToken jToken = container[_fieldname];
		return jToken != null && jToken.Value<object>() != null;
	}
}
