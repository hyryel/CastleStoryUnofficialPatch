using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class HasNoComponentOfTypeFilter : IJObjectFilter
{
	private string _typeName;

	public HasNoComponentOfTypeFilter(string typename)
	{
		_typeName = typename;
	}

	public bool Matches(JContainer container)
	{
		JArray jArray = container["components"].Value<JArray>();
		foreach (JToken item in jArray)
		{
			if (item["$type"] != null && item["$type"].Value<string>() == _typeName)
			{
				return false;
			}
		}
		return true;
	}
}
