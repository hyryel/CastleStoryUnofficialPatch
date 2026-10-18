using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class FieldValueIsNullFilter : IJObjectFilter
{
	private string _fieldname;

	public FieldValueIsNullFilter(string fieldname)
	{
		_fieldname = fieldname;
	}

	public bool Matches(JContainer container)
	{
		JToken jToken = container[_fieldname];
		return jToken == null || jToken.Type == JTokenType.Null || jToken.Value<JToken>() == null;
	}
}
